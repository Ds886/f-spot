//
// ImportDialogController.cs
//
// Author:
//   Daniel Köb <daniel.koeb@peony.at>
//   Ruben Vermeersch <ruben@savanne.be>
//
// Copyright (C) 2014 Daniel Köb
// Copyright (C) 2010 Novell, Inc.
// Copyright (C) 2010 Ruben Vermeersch
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FSpot.Core;
using FSpot.Database;
using FSpot.FileSystem;
using FSpot.Imaging;
using FSpot.Models;

using Gtk;

using Hyena;

using Mono.Unix;

using Thread = System.Threading.Thread;

namespace FSpot.Import
{
	public class ImportDialogController
	{
		public ImportPreferences Preferences { get; }
		public PhotoList Photos { get; }
		public int PhotosImported { get; private set; }
		public List<SafeUri> FailedImports { get; }

		List<ImportSource> sources;
		public List<ImportSource> Sources {
			get => sources ??= ScanSources ();
		}

		public ImportDialogController (bool persistPreferences)
		{
			Preferences = new ImportPreferences (persistPreferences);

			Photos = new PhotoList ();
			FailedImports = new List<SafeUri> ();
		}

		static List<ImportSource> ScanSources ()
		{
			var sources = new List<ImportSource> ();

			foreach (var drive in DriveInfo.GetDrives ()) {
				var root = new SafeUri (drive.RootDirectory.FullName);
				var label = !string.IsNullOrEmpty (drive.VolumeLabel) ? drive.VolumeLabel : "Local Disk";
				sources.Add (new ImportSource (root, $"{label} ({drive.Name})", null));
			}

			return sources;
		}

		#region Status Reporting

		public delegate void ImportProgressHandler (int current, int total);
		public event ImportProgressHandler ProgressUpdated;

		public delegate void ImportEventHandler (ImportEvent evnt);
		public event ImportEventHandler StatusEvent;

		void FireEvent (ImportEvent evnt)
		{
			ThreadAssist.ProxyToMain (() => { StatusEvent?.Invoke (evnt); });
		}

		void ReportProgress (int current, int total)
		{
			ProgressUpdated?.Invoke (current, total);
		}

		#endregion

		ImportSource activeSource;
		public ImportSource ActiveSource {
			set {
				if (value == activeSource)
					return;
				activeSource = value;

				CancelScan ();
				StartScan ();

				FireEvent (ImportEvent.SourceChanged);
			}
			get {
				return activeSource;
			}
		}

		#region Photo Scanning

		//Thread scanThread;
		CancellationTokenSource scanTokenSource;
		Task scan;
		void StartScan ()
		{
			//if (scanThread != null)
			//	CancelScan ();
			if (activeSource == null)
				return;

			var source = activeSource.GetFileImportSource (
				App.Instance.Container.Resolve<IImageFileFactory> (),
				App.Instance.Container.Resolve<IFileSystem> ());
			Photos.Clear ();

			scanTokenSource = new CancellationTokenSource ();
			//scanThread = ThreadAssist.Spawn (() => DoScan (source, recurseSubdirectories, mergeRawAndJpeg, scanTokenSource.Token));
			scan = Task.Run (() =>
				DoScan (source, Preferences, scanTokenSource.Token), scanTokenSource.Token);
		}

		void CancelScan ()
		{
			////if (scanThread == null)
			//	return;

			scanTokenSource?.Cancel ();
			//scanThread.Join ();
			//scanThread = null;
			//scanTokenSource = null;

			// Make sure all photos are added. This is needed to prevent
			// a race condition where a source is deactivated, yet photos
			// are still added to the collection because they are
			// queued on the mainloop.
			while (Application.EventsPending ()) {
				Application.RunIteration (false);
			}
		}

		void DoScan (IImportSource source, ImportPreferences preferences, CancellationToken token)
		{
			FireEvent (ImportEvent.PhotoScanStarted);

			var scanPhotos = source.ScanPhotos (preferences);
			foreach (var info in scanPhotos) {
				ThreadAssist.ProxyToMain (() => Photos.Add (info));
				if (token.IsCancellationRequested)
					break;
			}

			FireEvent (ImportEvent.PhotoScanFinished);
		}

		#endregion

		#region Importing

		Thread importThread;
		CancellationTokenSource importTokenSource;

		public void StartImport ()
		{
			if (importThread != null)
				throw new Exception ("Import already running!");

			importTokenSource = new CancellationTokenSource ();
			importThread = ThreadAssist.Spawn (() => DoImport (importTokenSource.Token));
		}

		public void CancelImport ()
		{
			CancelScan ();

			importTokenSource?.Cancel ();
			//FIXME: there is a race condition between the null check and calling join
			importThread?.Join ();
		}

		void DoImport (CancellationToken token)
		{
			//scanThread?.Join ();

			FireEvent (ImportEvent.ImportStarted);

			var importer = App.Instance.Container.Resolve<IImportController> ();
			importer.DoImport (App.Instance.Database, Photos, attachTags, Preferences, (current, total) => ThreadAssist.ProxyToMain (() => ReportProgress (current, total)),
				token);

			PhotosImported = importer.PhotosImported;
			FailedImports.Clear ();
			FailedImports.AddRange (importer.FailedImports);

			if (!token.IsCancellationRequested)
				importThread = null;

			FireEvent (ImportEvent.ImportFinished);
		}

		#endregion

		#region Tagging

		readonly List<Tag> attachTags = new List<Tag> ();
		readonly TagStore tagStore = new TagStore ();

		// Set the tags that will be added on import.
		public void AttachTags (IEnumerable<string> tags)
		{
			var importCategory = GetImportedTagsCategory ();
			foreach (var tagname in tags) {
				var tag = tagStore.GetTagByName (tagname);
				if (tag == null) {
					tag = tagStore.CreateTag (importCategory, tagname, false, true);
					tagStore.Commit (tag);
				}
				attachTags.Add (tag);
			}
		}

		Tag GetImportedTagsCategory ()
		{
			var defaultCategory = tagStore.GetTagByName (Catalog.GetString ("Imported Tags"));
			if (defaultCategory == null) {
				defaultCategory = tagStore.CreateTag (null, Catalog.GetString ("Imported Tags"), false, true);
				defaultCategory.ThemeIconName = "gtk-new";
			}
			return defaultCategory;
		}

		#endregion
	}
}
