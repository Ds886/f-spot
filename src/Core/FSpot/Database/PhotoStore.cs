// Copyright (C) 2016 Daniel Köb
// Copyright (C) 2013-2020 Stephen Shaw
// Copyright (C) 2004-2010 Novell, Inc.
// Copyright (C) 2009-2010 Mike Gemünde
// Copyright (C) 2004-2007 Larry Ewing
// Copyright (C) 2008-2010 Ruben Vermeersch
// Copyright (C) 2006-2009 Stephane Delcroix
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FSpot.Core;
using FSpot.Models;
using FSpot.Query;
using FSpot.Utils;

using Hyena;

namespace FSpot.Database
{
	public class PhotoStore : DbStore<Photo>
	{
		public int TotalPhotos {
			get {
				return Context.Photos.Count ();
			}
		}

		//readonly IImageFileFactory imageFileFactory;
		//readonly IThumbnailService thumbnailService;

		public PhotoStore (/*IImageFileFactory imageFileFactory, IThumbnailService thumbnailService,*/)
		{
			//this.imageFileFactory = imageFileFactory;
			//this.thumbnailService = thumbnailService;
		}

		public bool HasDuplicate (IPhoto item)
		{
			//var uri = item.DefaultVersion.Uri;

			//// Check if the exact given uri already exists.
			//const string query = "SELECT COUNT(*) AS count FROM photo_versions WHERE base_uri = ? AND filename = ?";
			//var photos = Context.PhotoVersions.Where (x => x.BaseUri == uri.GetBaseUri ().ToString () && x.Filename == uri.GetFilename ());
			//int count = photos.Count ();
			//if (count > 0)
			//	return true;

			//// Check by MD5. Won't import if there are photos with the same ImportMD5.
			//string hash = item.DefaultVersion.ImportMD5;
			//var condition = new ConditionWrapper ($"import_md5 = \"{hash}\"");
			//var dupes_by_hash = Count ("photo_versions", condition);
			//if (dupes_by_hash > 0)
			//	return true;

			//// This is a very lame check to overcome the lack of duplicate detect data right after transition.
			////
			//// Does filename matching if there are files with no hash for the original version.
			//condition = new ConditionWrapper ("version_id = 1 AND (import_md5 = \"\" OR import_md5 IS NULL)");
			//var have_no_hashes = Count ("photo_versions", condition);
			//if (have_no_hashes <= 0)
			//	// No matches
			//	return false;

			//var name = uri.GetFilename ();
			//DateTime? time = null;

			//// Look for a filename match.
			//using (var reader = Database.Query (new HyenaSqliteCommand ("SELECT photos.id, photos.time, pv.filename FROM photos LEFT JOIN photo_versions AS pv ON pv.photo_id = photos.id WHERE pv.filename = ?", name))) {
			//	while (reader.Read ()) {
			//		Log.DebugFormat ("Found one possible duplicate for {0}", reader ["filename"]);
			//		if (!time.HasValue) {
			//			// Only read time when needed
			//			time = item.Time;
			//		}

			//		if (reader ["time"].ToString () == DateTimeUtil.FromDateTime (time.Value).ToString ()) {
			//			Log.Debug ("Skipping duplicate", uri);

			//			// Schedule a hash calculation job on the existing file.
			//			CalculateHashJob.Create (Db.Jobs, Convert.ToUInt32 (reader ["id"]));

			//			return true;
			//		}
			//	}
			//}
			return false;
		}

		public Photo CreateFrom (IPhoto item, bool defaultVersionOnly, Guid rollId)
		{
			//Photo photo = new Photo {
			//	UtcTime = item.Time,
			//	BaseUri = item.DefaultVersion.BaseUri,
			//	Filename = item.DefaultVersion.Filename,
			//	Description = item.Description ?? string.Empty,
			//	RollId = rollId,
			//	DefaultVersionId = ???,
			//	Rating = 0
			//};

			//Context.Photos.Add (photo);
			//Context.SaveChanges ();
			//uint id = (uint)Database.Execute (
			//	new HyenaSqliteCommand (
			//		"INSERT INTO photos (time, base_uri, filename, description, roll_id, default_version_id, rating) " +
			//		"VALUES (?, ?, ?, ?, ?, ?, ?)",
			//		item.Time,
			//		item.DefaultVersion.BaseUri.ToString (),
			//		item.DefaultVersion.Filename,
			//		description,
			//		rollId,
			//		Photo.OriginalVersionId,
			//		"0"
			//	));

			//photo = new Photo (imageFileFactory, thumbnailService, id, unix_time);

			//uint versionId = Photo.OriginalVersionId;
			//var versions = defaultVersionOnly ? new[] { item.DefaultVersion } : item.Versions;
			//foreach (IPhotoVersion version in versions) {
			//	// rename original version to "Original" if we import default version only
			//	// this applies when a version is detached from another photo
			//	string name = defaultVersionOnly && versionId == Photo.OriginalVersionId ? Catalog.GetString ("Original") : version.Name;
			//	photo.AddVersionUnsafely (versionId++, version.BaseUri, version.Filename, version.ImportMD5, name, true);
			//	InsertVersion (photo, photo.Versions.Last () as PhotoVersion);
			//}
			//photo.DefaultVersionId = versionId - 1;
			//photo.AllVersionsLoaded = true;

			//EmitAdded (photo);
			//return photo;
			return null;
		}

		void InsertVersion (BaseDbSet photo, PhotoVersion version)
		{
			var importMd5 = !string.IsNullOrEmpty (version.ImportMd5) ? version.ImportMd5 : null;

			var newVersion = new PhotoVersion {
				PhotoId = photo.Id,
				VersionId = version.VersionId,
				Name = version.Name,
				BaseUri = version.BaseUri,
				Filename = version.Filename,
				Protected = version.Protected,
				ImportMd5 = importMd5
			};

			Context.PhotoVersions.Add (newVersion);
			Context.SaveChanges ();
		}

		public List<Photo> GetAllPhotos ()
		{
			return Context.Photos.ToList ();
		}

		void GetVersions (Photo photo)
		{
			var versions1 = photo.Versions;
			var versions = Context.PhotoVersions.Where (x => x.Id == photo.Id);
			//using (var reader = Database.Query (new HyenaSqliteCommand (
			//	"SELECT version_id, name, base_uri, filename, import_md5, protected " +
			//	"FROM photo_versions " +
			//	"WHERE photo_id = ?",
			//	photo.Id))) {

			foreach (var version in versions) {
				//while (reader.Read ()) {
				//uint version_id = Convert.ToUInt32 (reader ["version_id"]);
				//string name = reader ["name"].ToString ();
				//var base_uri = new SafeUri (reader ["base_uri"].ToString (), true);
				//var filename = reader ["filename"].ToString ();
				//string import_md5 = reader ["import_md5"] != null ? reader ["import_md5"].ToString () : null;
				//bool is_protected = Convert.ToBoolean (reader ["protected"]);

				//photo.AddVersionUnsafely (version.Id, version.BaseUri, version.Filename, version.ImportMd5, version.Name, version.Protected);
			}
			//}
		}

		void GetTags (Photo photo)
		{
			var photoTags = Context.PhotoTags.Where (x => x.PhotoId == photo.Id);
			//using (var reader = Database.Query (new HyenaSqliteCommand ("SELECT tag_id FROM photo_tags WHERE photo_id = ?", photo.Id))) {
			//while (reader.Read ()) {
			//	uint tag_id = Convert.ToUInt32 (reader ["tag_id"]);
			//	Tag tag = Db.Tags.Get (tag_id);
			//	photo.AddTagUnsafely (tag);
			//}

			foreach (var photoTag in photoTags) {
				//photo.AddTagUnsafely (photoTag.Tag);
			}
			//}
		}

		void GetAllVersions (IEnumerable<Guid> ids)
		{
			var photoVersions = Context.PhotoVersions.Where (x => ids.Contains (x.PhotoId));
			//using (var reader = Database.Query ("SELECT photo_id, version_id, name, base_uri, filename, import_md5, protected FROM photo_versions WHERE photo_id IN " + ids)) {
			//	while (reader.Read ()) {
			//		uint id = Convert.ToUInt32 (reader ["photo_id"]);
			//		Photo photo = LookupInCache (id);
			foreach (var photoVersion in photoVersions) {
				if (photoVersion == null)
					continue;

				//if (photoVersion.AllVersionsLoaded)
				//	continue;

				//photoVersion.AddVersionUnsafely (photoVersion.VersionId, photoVersion.BaseUri, photoVersion.Filename,
				//	photoVersion.ImportMd5, photoVersion.Name, photoVersion.Protected);

			}
			//		if (photo == null)
			//			continue;

			//		if (photo.AllVersionsLoaded)
			//			continue;

			//		if (reader ["version_id"] != null) {
			//			uint version_id = Convert.ToUInt32 (reader ["version_id"]);
			//			string name = reader ["name"].ToString ();
			//			var base_uri = new SafeUri (reader ["base_uri"].ToString (), true);
			//			var filename = reader ["filename"].ToString ();
			//			string import_md5 = reader ["import_md5"] != null ? reader ["import_md5"].ToString () : null;
			//			bool is_protected = Convert.ToBoolean (reader ["protected"]);

			//			photo.AddVersionUnsafely (version_id, base_uri, filename, import_md5, name, is_protected);
			//		}
			//	}
			//}
		}

		void GetAllTags (List<Guid> ids)
		{
			var photoTags = Context.PhotoTags.Where (x => ids.Contains (x.PhotoId));
			//using (var reader = Database.Query ("SELECT photo_id, tag_id FROM photo_tags WHERE photo_id IN " + ids)) {
			//while (reader.Read ()) {
			//	uint id = Convert.ToUInt32 (reader ["photo_id"]);
			//	Photo photo = LookupInCache (id);

			foreach (var photoTag in photoTags) {

				if (photoTag == null)
					continue;

				//if (photoTag.AllVersionsLoaded)
				//	continue;

				//if (reader[1] != null) {
				//	uint tag_id = Convert.ToUInt32 (reader["tag_id"]);
				//	Tag tag = Db.Tags.Get (tag_id);
				//photoTag.AddTagUnsafely (photoTag.Tag);
				//}
			}
		}

		public override Photo Get (Guid id)
		{
			var photo = Context.Photos.FirstOrDefault (x => x.Id == id);
			//using (var reader = Database.Query (new HyenaSqliteCommand (
			//	"SELECT time, description, roll_id, default_version_id, rating " +
			//	"FROM photos " +
			//	"WHERE id = ?", id))) {

			//photo = new Photo (imageFileFactory, thumbnailService, id, Convert.ToInt64 (reader ["time"]));
			//photo.Description = reader ["description"].ToString ();
			//photo.RollId = Convert.ToUInt32 (reader ["roll_id"]);
			//photo.DefaultVersionId = Convert.ToUInt32 (reader ["default_version_id"]);
			//photo.Rating = Convert.ToUInt32 (reader ["rating"]);

			GetTags (photo);
			GetVersions (photo);

			return photo;
			//}
		}

		public Photo GetByUri (SafeUri uri)
		{
			Photo photo = null;

			var base_uri = uri.GetBaseUri ();
			var filename = uri.GetFilename ();

			//using (var reader = Database.Query (new HyenaSqliteCommand (
			//	"SELECT id, time, description, roll_id, default_version_id, rating " +
			//	" FROM photos " +
			//	" LEFT JOIN photo_versions AS pv ON photos.id = pv.photo_id" +
			//	" WHERE (photos.base_uri = ? AND photos.filename = ?)" +
			//	" OR (pv.base_uri = ? AND pv.filename = ?)",
			//	base_uri.ToString (), filename,
			//	base_uri.ToString (), filename))) {

			//	if (reader.Read ()) {
			//		photo = new Photo (imageFileFactory, thumbnailService, Convert.ToUInt32 (reader ["id"]),
			//			Convert.ToInt64 (reader ["time"]));

			//		photo.Description = reader ["description"].ToString ();
			//		photo.RollId = Convert.ToUInt32 (reader ["roll_id"]);
			//		photo.DefaultVersionId = Convert.ToUInt32 (reader ["default_version_id"]);
			//		photo.Rating = Convert.ToUInt32 (reader ["rating"]);
			//	}

			//	if (photo == null)
			//		return null;

			//	Photo cached = LookupInCache (photo.Id);

			//	if (cached != null)
			//		return cached;

			//	GetTags (photo);
			//	GetVersions (photo);

			return photo;
			//}
		}

		public void Remove (IEnumerable<Tag> tags)
		{
			//Photo[] photos = Query (new OrOperator (tags.Select (t => new TagTerm (t)).ToArray ()));

			//foreach (Photo photo in photos)
			//	photo.RemoveCategory (tags);
			//Commit (photos);

			//foreach (Tag tag in tags)
			//	Db.Tags.Remove (tag);
		}

		public void Remove (Photo[] items)
		{
			EmitRemoved (items);

			var query_builder = new List<string> (items.Length);
			//for (int i = 0; i < items.Length; i++) {
			//	query_builder.Add ($"{items[i].Id}");
			//	RemoveFromCache (items [i]);
			//}

			//var id_list = string.Join ("','", query_builder.ToArray ());
			//Database.Execute ($"DELETE FROM photos WHERE id IN ('{id_list}')");
			//Database.Execute ($"DELETE FROM photo_tags WHERE photo_id IN ('{id_list}')");
			//Database.Execute ($"DELETE FROM photo_versions WHERE photo_id IN ('{id_list}')");
		}

		public override void Remove (Photo item)
		{
			Remove (new[] { item });
		}

		public override void Commit (Photo item)
		{
			Commit (new[] { item });
		}

		public void Commit (Photo[] items)
		{
			uint timer = Log.DebugTimerStart ();

			//var changes = new PhotosChanges ();
			//foreach (IDbItem item in items)
			//	changes |= Update ((Photo)item);

			//EmitChanged (items, new PhotoEventArgs (items, changes));
			Log.DebugTimerPrint (timer, "Commit took {0}");
		}

		//PhotoChanges Update (Photo photo)
		//{
		//	PhotoChanges changes = photo.Changes;
		//	// Update photo.
		//	if (changes.DescriptionChanged || changes.DefaultVersionIdChanged || changes.TimeChanged || changes.UriChanged || changes.RatingChanged || changes.MD5SumChanged)
		//		Database.Execute (
		//			new HyenaSqliteCommand (
		//				"UPDATE photos " +
		//				"SET description = ?, " +
		//				"    default_version_id = ?, " +
		//				"    time = ?, " +
		//				"    base_uri = ?, " +
		//				"    filename = ?, " +
		//				"    rating = ? " +
		//				"WHERE id = ? ",
		//				photo.Description,
		//				photo.DefaultVersionId,
		//				DateTimeUtil.FromDateTime (photo.Time),
		//				photo.VersionUri (Photo.OriginalVersionId).GetBaseUri ().ToString (),
		//				photo.VersionUri (Photo.OriginalVersionId).GetFilename (),
		//				$"{photo.Rating}",
		//				photo.Id
		//			)
		//		);

		//	// Update tags.
		//	if (changes.TagsRemoved != null)
		//		foreach (Tag tag in changes.TagsRemoved)
		//			Database.Execute (new HyenaSqliteCommand (
		//				"DELETE FROM photo_tags WHERE photo_id = ? AND tag_id = ?",
		//				photo.Id,
		//				tag.Id));

		//	if (changes.TagsAdded != null)
		//		foreach (Tag tag in changes.TagsAdded)
		//			Database.Execute (new HyenaSqliteCommand (
		//				"INSERT OR IGNORE INTO photo_tags (photo_id, tag_id) " +
		//				"VALUES (?, ?)",
		//				photo.Id,
		//				tag.Id));

		//	// Update versions.
		//	if (changes.VersionsRemoved != null)
		//		foreach (uint version_id in changes.VersionsRemoved)
		//			Database.Execute (new HyenaSqliteCommand (
		//				"DELETE FROM photo_versions WHERE photo_id = ? AND version_id = ?",
		//				photo.Id,
		//				version_id));

		//	if (changes.VersionsAdded != null)
		//		foreach (uint version_id in changes.VersionsAdded) {
		//			PhotoVersion version = photo.GetVersion (version_id);
		//			InsertVersion (photo, version);
		//		}

		//	if (changes.VersionsModified != null)
		//		foreach (uint version_id in changes.VersionsModified) {
		//			PhotoVersion version = photo.GetVersion (version_id);
		//			Database.Execute (new HyenaSqliteCommand (
		//				"UPDATE photo_versions SET name = ?, " +
		//				"base_uri = ?, filename = ?, protected = ?, import_md5 = ? " +
		//				"WHERE photo_id = ? AND version_id = ?",
		//				version.Name,
		//				version.BaseUri.ToString (),
		//				version.Filename,
		//				version.IsProtected,
		//				(version.ImportMD5 != string.Empty ? version.ImportMD5 : null),
		//				photo.Id,
		//				version_id));
		//		}

		//	photo.Changes = null;
		//	return changes;
		//}

		public void CalculateMD5Sum (Photo photo)
		{
			//foreach (uint version_id in photo.VersionIds) {
			//	PhotoVersion version = photo.GetVersion (version_id);

			//	// Don't overwrite MD5 sums that are already calculated.
			//	if (version.ImportMD5 != string.Empty && version.ImportMD5 != null)
			//		continue;

			//	string version_md5_sum = HashUtils.GenerateMD5 (version.Uri);
			//	version.ImportMD5 = version_md5_sum;
			//	photo.Changes.ChangeVersion (version_id);
			//}

			//Commit (photo);
		}

		public int Count (string tableName, params IQueryCondition[] conditions)
		{
			//	var query_builder = new StringBuilder ("SELECT COUNT(*) AS count FROM " + tableName + " ");
			//	bool where_added = false;
			//	foreach (IQueryCondition condition in conditions) {
			//		if (condition == null)
			//			continue;
			//		if (condition is IOrderCondition)
			//			continue;
			//		query_builder.Append (where_added ? " AND " : " WHERE ");
			//		query_builder.Append (condition.SqlClause ());
			//		where_added = true;
			//	}

			//	using (var reader = Database.Query (query_builder.ToString ())) {
			//		reader.Read ();
			//		return Convert.ToInt32 (reader ["count"]);
			//	}
			return 0;
		}

		public int[] IndicesOf (string tableName, uint[] items)
		{
			var query_builder = new StringBuilder ($"SELECT ROWID AS row_id FROM {tableName} WHERE id IN (");
			for (int i = 0; i < items.Length; i++) {
				query_builder.Append (items[i]);
				query_builder.Append ((i != items.Length - 1) ? ", " : ")");
			}
			return IndicesOf (query_builder.ToString ());
		}

		public int IndexOf (string tableName, Photo photo)
		{
			string query = $"SELECT ROWID AS row_id FROM {tableName} WHERE id = {photo.Id}";
			return IndexOf (query);
		}

		public int IndexOf (string tableName, DateTime time, bool asc)
		{
			string query =
				$"SELECT ROWID AS row_id FROM {tableName} WHERE time {(asc ? ">=" : "<=")} {time} ORDER BY time {(asc ? "ASC" : "DESC")} LIMIT 1";
			return IndexOf (query);
		}

		int IndexOf (string query)
		{
			//uint timer = Log.DebugTimerStart ();
			//using (var reader = Database.Query (query)) {
			//	int index = - 1;
			//	if (reader.Read ())
			//		index = Convert.ToInt32 (reader ["row_id"]);
			//	Log.DebugTimerPrint (timer, "IndexOf took {0} : " + query);
			//	return index - 1; //ROWID starts counting at 1
			//}
			return -1;
		}

		int[] IndicesOf (string query)
		{
			//uint timer = Log.DebugTimerStart ();
			//var list = new List<int> ();
			//using (var reader = Database.Query (query)) {
			//	while (reader.Read ())
			//		list.Add (Convert.ToInt32 (reader ["row_id"]) - 1);
			//	Log.DebugTimerPrint (timer, "IndicesOf took {0} : " + query);
			//	return list.ToArray ();
			//}
			return Array.Empty<int> ();
		}

		static object populationTableLock = new object ();
		public Dictionary<int, int[]> PhotosPerMonth ()//params IQueryCondition [] conditions)
		{
			//lock (populationTableLock) {
			//	uint timer = Log.DebugTimerStart ();
			//	var val = new Dictionary<int, int []> ();

			//	//Sqlite is way more efficient querying to a temp then grouping than grouping at once
			//	Database.Execute ("DROP TABLE IF EXISTS population");
			//	var query_builder = new StringBuilder ("CREATE TEMPORARY TABLE population AS SELECT strftime('%Y%m', datetime(time, 'unixepoch')) AS month FROM photos");
			//	bool where_added = false;
			//	foreach (IQueryCondition condition in conditions) {
			//		if (condition == null)
			//			continue;
			//		if (condition is IOrderCondition)
			//			continue;
			//		query_builder.Append (where_added ? " AND " : " WHERE ");
			//		query_builder.Append (condition.SqlClause ());
			//		where_added = true;
			//	}
			//	Database.Execute (query_builder.ToString ());

			//	int minyear = Int32.MaxValue;
			//	int maxyear = Int32.MinValue;

			//	using (var reader = Database.Query ("SELECT COUNT (*) as count, month from population GROUP BY month")) {
			//		while (reader.Read ()) {
			//			string yyyymm = reader ["month"].ToString ();
			//			int count = Convert.ToInt32 (reader ["count"]);
			//			int year = Convert.ToInt32 (yyyymm.Substring (0, 4));
			//			maxyear = Math.Max (year, maxyear);
			//			minyear = Math.Min (year, minyear);
			//			int month = Convert.ToInt32 (yyyymm.Substring (4));
			//			if (!val.ContainsKey (year))
			//				val.Add (year, new int [12]);
			//			val [year] [month - 1] = count;
			//		}
			//	}

			//	//Fill the blank
			//	for (int i = minyear; i <= maxyear; i++)
			//		if (!val.ContainsKey (i))
			//			val.Add (i, new int [12]);

			//	Log.DebugTimerPrint (timer, "PhotosPerMonth took {0}");
			//	return val;
			//}
			return new Dictionary<int, int[]> ();
		}

		// Queries.
		public static string BuildQuery ()//params IQueryCondition [] conditions)
		{
			//var query_builder = new StringBuilder ("SELECT * FROM photos ");

			//bool where_added = false;
			//bool hidden_contained = false;
			//foreach (IQueryCondition condition in conditions) {

			//	if (condition == null)
			//		continue;

			//	hidden_contained |= condition is HiddenTag;

			//	if (condition is IOrderCondition)
			//		continue;

			//	string sql_clause = condition.SqlClause ();

			//	if (sql_clause == null || sql_clause.Trim () == string.Empty)
			//		continue;
			//	query_builder.Append (where_added ? " AND " : " WHERE ");
			//	query_builder.Append (sql_clause);
			//	where_added = true;
			//}

			///* if a HiddenTag condition is not explicitly given, we add one */
			//if (!hidden_contained) {
			//	string sql_clause = HiddenTag.HideHiddenTag.SqlClause ();

			//	if (sql_clause != null && sql_clause.Trim () != string.Empty) {
			//		query_builder.Append (where_added ? " AND " : " WHERE ");
			//		query_builder.Append (sql_clause);
			//	}
			//}

			//bool order_added = false;
			//foreach (IQueryCondition condition in conditions) {
			//	if (condition == null)
			//		continue;

			//	if (!(condition is IOrderCondition))
			//		continue;

			//	string sql_clause = condition.SqlClause ();

			//	if (sql_clause == null || sql_clause.Trim () == string.Empty)
			//		continue;
			//	query_builder.Append (order_added ? " , " : "ORDER BY ");
			//	query_builder.Append (sql_clause);
			//	order_added = true;
			//}

			//return query_builder.ToString ();
			return string.Empty;
		}

		//public Photo [] Query (params IQueryCondition [] conditions)
		//{
		//	return Query (BuildQuery (conditions));
		//}

		//public void QueryToTemp (string tempTable, params IQueryCondition [] conditions)
		//{
		//	QueryToTemp (tempTable, BuildQuery (conditions));
		//}

		public void QueryToTemp (string tempTable, string query)
		{
			//uint timer = Log.DebugTimerStart ();
			//Log.Debug ($"Query Started : {query}");
			//Database.BeginTransaction ();
			//Database.Execute ($"DROP TABLE IF EXISTS {tempTable}");
			//Database.Execute ($"CREATE TEMPORARY TABLE {tempTable} AS {query}");
			//// For Hyena.Data.Sqlite, we need to call Execute. Calling Query here does fail.
			////Database.Query (string.Format ("CREATE TEMPORARY TABLE {0} AS {1}", temp_table, query)).Close ();
			//Database.CommitTransaction ();
			//Log.DebugTimerPrint (timer, "QueryToTemp took {0} : " + query);
		}

		public Photo[] QueryFromTemp (string tempTable, int offset = 0, int limit = -1)
		{
			return Query ($"SELECT * FROM {tempTable} LIMIT {limit} OFFSET {offset}");
		}

		//public Photo [] Query (string query)
		//{
		//	return Query ("new HyenaSqliteCommand (query)");
		//}

		public Photo[] Query (string query)
		{
			//uint timer = Log.DebugTimerStart ();
			//var new_photos = new List<Photo> ();
			//var query_result = new List<Photo> ();

			//using (var reader = Database.Query (query)) {
			//	while (reader.Read ()) {
			//		uint id = Convert.ToUInt32 (reader ["id"]);
			//		Photo photo = LookupInCache (id);

			//		if (photo == null) {
			//			photo = new Photo (imageFileFactory, thumbnailService, id, Convert.ToInt64 (reader ["time"]));
			//			photo.Description = reader ["description"].ToString ();
			//			photo.RollId = Convert.ToUInt32 (reader ["roll_id"]);
			//			photo.DefaultVersionId = Convert.ToUInt32 (reader ["default_version_id"]);
			//			photo.Rating = Convert.ToUInt32 (reader ["rating"]);
			//			new_photos.Add (photo);
			//		}

			//		query_result.Add (photo);
			//	}
			//}

			//bool need_load = false;
			//string photo_ids = "(";
			//foreach (Photo photo in new_photos) {
			//	AddToCache (photo);
			//	photo_ids = photo_ids + Convert.ToString (photo.Id) + ",";
			//	need_load |= !photo.AllVersionsLoaded;
			//}

			//photo_ids = photo_ids + "-1)";

			//if (need_load) {
			//	GetAllTags (photo_ids);
			//	GetAllVersions (photo_ids);
			//	foreach (Photo photo in new_photos)
			//		photo.AllVersionsLoaded = true;
			//}

			//foreach (Photo photo in new_photos)
			//	photo.Changes = null;

			//Log.DebugTimerPrint (timer, "Query took {0} : " + query.Text);
			//return query_result.ToArray ();
			return Array.Empty<Photo> ();
		}

		public Photo[] Query (SafeUri uri)
		{
			//	string filename = uri.GetFilename ();

			//	/* query by file */
			//	if (!string.IsNullOrEmpty (filename)) {
			//		return Query (new HyenaSqliteCommand (
			//			"SELECT id, " +
			//				"time, " +
			//				"base_uri, " +
			//				"filename, " +
			//				"description, " +
			//				"roll_id, " +
			//				"default_version_id, " +
			//				"rating " +
			//			"FROM photos " +
			//			"WHERE base_uri LIKE ?" +
			//			"AND filename LIKE ?",
			//			uri.GetBaseUri ().ToString (),
			//			filename));
			//	}

			//	/* query by directory */
			//	return Query (new HyenaSqliteCommand (
			//		"SELECT id, " +
			//			"time, " +
			//			"base_uri, " +
			//			"filename, " +
			//			"description, " +
			//			"roll_id, " +
			//			"default_version_id, " +
			//			"rating " +
			//		"FROM photos " +
			//		"WHERE base_uri LIKE ?" +
			//		"AND base_uri NOT LIKE ?",
			//		uri + "%",
			//		uri + "/%/%"));
			return Array.Empty<Photo> ();
		}
	}
}
