// Copyright (C) 2014 Daniel Köb
// Copyright (C) 2010 Novell, Inc.
// Copyright (C) 2010 Ruben Vermeersch
// Copyright (C) 2020 Stephen Shaw
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using FSpot.Core;
using FSpot.Database;
using FSpot.Models;
using FSpot.FileSystem;
using FSpot.Services;
using FSpot.Settings;
using FSpot.Thumbnail;
using FSpot.Utils;

using Hyena;

namespace FSpot.Import
{
	internal class ImportController : IImportController
	{
		readonly IFileSystem fileSystem;
		readonly IThumbnailLoader thumbnailLoader;

		PhotoFileTracker photo_file_tracker;
		MetadataImporter metadata_importer;
		Stack<SafeUri> created_directories;
		List<Guid> imported_photos;
		readonly List<SafeUri> failedImports = new List<SafeUri> ();
		Roll createdRoll;

		public IEnumerable<SafeUri> FailedImports { get { return failedImports.AsEnumerable (); } }
		public int PhotosImported { get { return imported_photos.Count; } }

		public ImportController (IFileSystem fileSystem, IThumbnailLoader thumbnailLoader)
		{
			this.fileSystem = fileSystem;
			this.thumbnailLoader = thumbnailLoader;
		}

		#region IImportConroller

		public void DoImport (IDb db, IBrowsableCollection photos, IList<Tag> tagsToAttach, bool duplicateDetect,
			bool copyFiles, bool removeOriginals, Action<int, int> reportProgress, CancellationToken token)
		{
			//db.Sync = false;
			created_directories = new Stack<SafeUri> ();
			imported_photos = new List<Guid> ();
			photo_file_tracker = new PhotoFileTracker (fileSystem);
			metadata_importer = new MetadataImporter (db.Tags);

			createdRoll = new RollStore ().Create ();

			fileSystem.Directory.CreateDirectory (FSpotConfiguration.PhotoUri);

			try {
				int i = 0;
				int total = photos.Count;
				foreach (var info in photos.Items) {
					if (token.IsCancellationRequested) {
						RollbackImport (db);
						return;
					}

					reportProgress (i++, total);
					try {
						ImportPhoto (db, info, createdRoll, tagsToAttach, duplicateDetect, copyFiles);
					} catch (Exception e) {
						Log.Debug ($"Failed to import {info.DefaultVersion.Uri}");
						Log.DebugException (e);
						failedImports.Add (info.DefaultVersion.Uri);
					}
				}

				FinishImport (removeOriginals);
			} catch (Exception e) {
				RollbackImport (db);
				throw;
			} finally {
				Cleanup (db);
			}
		}

		#endregion

		#region private

		void ImportPhoto (IDb db, IPhoto item, Roll roll, IList<Tag> tagsToAttach, bool duplicateDetect, bool copyFiles)
		{
			if (item is IInvalidPhotoCheck check && check.IsInvalid) {
				throw new Exception ("Failed to parse metadata, probably not a photo");
			}

			// Do duplicate detection
			if (duplicateDetect && db.Photos.HasDuplicate (item)) {
				return;
			}

			if (copyFiles) {
				var destinationBase = FindImportDestination (item, FSpotConfiguration.PhotoUri);
				fileSystem.Directory.CreateDirectory (destinationBase);
				// Copy into photo folder.
				photo_file_tracker.CopyIfNeeded (item, destinationBase);
			}

			// Import photo
			var photo = db.Photos.CreateFrom (item, false, roll.Id);

			bool needs_commit = false;

			// Add tags
			if (tagsToAttach.Count > 0) {
				TagService.Instance.Add (photo, tagsToAttach);
				needs_commit = true;
			}

			// Import XMP metadata
			needs_commit |= metadata_importer.Import (photo, item);

			if (needs_commit) {
				db.Photos.Commit (photo);
			}

			// Prepare thumbnail (Import is I/O bound anyway)
			thumbnailLoader.Request (item.DefaultVersion.Uri, ThumbnailSize.Large, 10);

			imported_photos.Add (photo.Id);
		}

		void RollbackImport (IDb db)
		{
			// Remove photos
			foreach (var id in imported_photos) {
				db.Photos.Remove (db.Photos.Get (id));
			}

			foreach (var uri in photo_file_tracker.CopiedFiles) {
				fileSystem.File.Delete (uri);
			}

			// Clean up directories
			while (created_directories.Count > 0) {
				var uri = created_directories.Pop ();
				try {
					fileSystem.Directory.Delete (uri);
				} catch (Exception e) {
					Log.Warning ($"Failed to clean up directory '{uri}': {e.Message}");
				}
			}

			// Clean created tags
			metadata_importer.Cancel ();

			// Remove created roll
			db.Rolls.Remove (createdRoll);
		}

		void Cleanup (IDb db)
		{
			if (imported_photos != null && imported_photos.Count == 0)
				db.Rolls.Remove (createdRoll);

			//FIXME: we are cleaning a cache that is never used, that smells...
			//Photo.ResetMD5Cache ();
			GC.Collect ();
		}

		void FinishImport (bool removeOriginals)
		{
			if (!removeOriginals) return;

			foreach (var uri in photo_file_tracker.OriginalFiles) {
				try {
					fileSystem.File.Delete (uri);
				} catch (Exception e) {
					Log.Warning ($"Failed to remove original file '{uri}': {e.Message}");
				}
			}
		}

		internal static SafeUri FindImportDestination (IPhoto item, SafeUri baseUri)
		{
			DateTime time = item.UtcTime;
			return baseUri
				.Append (time.Year.ToString ())
				.Append ($"{time.Month:D2}")
				.Append ($"{time.Day:D2}");
		}

		#endregion
	}
}