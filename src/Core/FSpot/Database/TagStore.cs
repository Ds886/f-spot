// Copyright (C) 2016 Daniel Köb
// Copyright (C) 2003-2009 Novell, Inc.
// Copyright (C) 2003 Ettore Perazzoli
// Copyright (C) 2007-2009 Stephane Delcroix
// Copyright (C) 2004-2006 Larry Ewing
// Copyright (C) 2020 Stephen Shaw
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;

using FSpot.Models;

using Hyena;

using Mono.Unix;

namespace FSpot.Database
{
	public class InvalidTagOperationException : InvalidOperationException
	{
		public Tag Tag { get; }

		public InvalidTagOperationException (Tag t, string message) : base (message)
		{
			Tag = t;
		}
	}

	// Sorts tags into an order that it will be safe to delete
	// them in (eg children first).
	public class TagRemoveComparer : IComparer<Tag>
	{
		public int Compare (Tag t1, Tag t2)
		{
			if (t1.IsAncestorOf (t2))
				return 1;

			if (t2.IsAncestorOf (t1))
				return -1;

			return 0;
		}
	}

	public class TagStore : DbStore<Tag>
	{
		const string StockIconDbPrefix = "stock_icon:";

		Tag hidden;

		public Category RootCategory { get; }

		public Tag Hidden {
			get { return hidden; }
			private set {
				hidden = value;
				// FIXME, where did HiddenTag go?
				//HiddenTag.Tag = value;
			}
		}

		//static void SetIconFromString (Tag tag, string iconString)
		//{
		//	if (iconString == null) {
		//		tag.Icon = null;
		//		// IconWasCleared automatically set already, override
		//		// it in this case since it was NULL in the db.
		//		tag.IconWasCleared = false;
		//	} else if (string.IsNullOrEmpty (iconString))
		//		tag.Icon = null;
		//	else if (iconString.StartsWith (StockIconDbPrefix, StringComparison.Ordinal))
		//		tag.Icon = iconString.Substring (StockIconDbPrefix.Length);
		//	else
		//		tag.Icon = GdkUtils.Deserialize (Convert.FromBase64String (iconString));
		//}

		public Tag GetTagByName (string name)
		{
			var tag = Context.Tags.FirstOrDefault (x => string.Compare (x.Name, name, StringComparison.OrdinalIgnoreCase) == 0);
			return tag;
		}

		public Tag GetTagById (Guid id)
		{
			var tag = Context.Tags.FirstOrDefault (x => x.Id == id);
			return tag;
		}

		public List<Tag> TagsStartWith (string s)
		{
			var tags = Context.Tags
				.Where (x => s.ToLower ().StartsWith (x.Name.ToLower ()))
				.ToList ();

			if (!tags.Any())
				return null;

			tags.Sort ((t1, t2) => t2.Popularity.CompareTo (t1.Popularity));

			return tags;
		}

		// In this store we keep all the items (i.e. the tags) in memory at all times.  This is
		// mostly to simplify handling of the parent relationship between tags, but it also makes it
		// a little bit faster.  We achieve this by passing "true" as the cache_is_immortal to our
		// base class.
		void LoadAllTags ()
		{
			// Pass 1, get all the tags.
			var tags = Context.Tags;
			//IDataReader reader = Database.Query ("SELECT id, name, is_category, sort_priority, icon FROM tags");

			foreach (var tag in tags) { }
			//while (reader.Read ()) {
			//	uint id = Convert.ToUInt32 (reader["id"]);
			//	string name = reader["name"].ToString ();
			//	bool is_category = (Convert.ToUInt32 (reader["is_category"]) != 0);

			//	Tag tag;
			//	if (is_category)
			//		tag = new Category (null, id, name);
			//	else
			//		tag = new Tag (null, id, name);

			//	if (reader["icon"] != null)
			//		try {
			//			SetIconFromString (tag, reader["icon"].ToString ());
			//		} catch (Exception ex) {
			//			Log.Exception ("Unable to load icon for tag " + name, ex);
			//		}

			//	tag.SortPriority = Convert.ToInt32 (reader["sort_priority"]);
			//}

			// Pass 2, set the parents.
			foreach (var tag in tags) {
				if (tag.CategoryId == Guid.Empty)
					tag.Category = RootCategory;
				else {
					tag.Category = Get (tag.CategoryId) as Category;
					if (tag.Category == null)
						Log.Warning ("Tag Without category found");
				}
			}

			//Pass 3, set popularity
			var groups = Context.PhotoTags
				.GroupBy (x => x.TagId)
				.Select (n => new {
					TagId = n.Key,
					PhotoCount = n.Count ()
				});

			foreach (var tag in tags) {
				tag.Popularity = groups.First (x => x.TagId == tag.Id).PhotoCount;
			}

			//if (Context.Meta.HiddenTagId.Value != null)
			//	Hidden = LookupInCache ((uint)Db.Meta.HiddenTagId.ValueAsInt);
		}

		public TagStore ()
		{
			// The label for the root category is used in new and edit tag dialogs
			RootCategory = new Category (null, Guid.Empty, Catalog.GetString ("(None)"));
		}

		Tag InsertTagIntoTable (Category parentCategory, string name, bool isCategory, bool autoicon)
		{
			var tag = new Tag (parentCategory) {
				Name = name,
				IsCategory = isCategory,
				CategoryId = parentCategory.Id,
				Icon = autoicon ? null : string.Empty,
				SortPriority = 0
			};

			Context.Tags.Add (tag);
			Context.SaveChanges ();

			// The table in the database is setup to be an INTEGER.
			return tag;
		}

		public Tag CreateTag (Category category, string name, bool autoicon)
		{
			if (category == null)
				category = RootCategory;

			var tag = InsertTagIntoTable (category, name, false, autoicon);
			tag.IconWasCleared = !autoicon;

			EmitAdded (tag);

			return tag;
		}

		public Category CreateCategory (Category parentCategory, string name, bool autoicon)
		{
			if (parentCategory == null)
				parentCategory = RootCategory;

			var tag = InsertTagIntoTable (parentCategory, name, true, autoicon);

			tag.IconWasCleared = !autoicon;

			EmitAdded (tag);

			return tag as Category;
		}

		public override Tag Get (Guid id)
		{
			return id == Guid.Empty ? RootCategory : Context.Tags.FirstOrDefault (x => x.Id == id);
		}

		public override void Remove (Tag item)
		{
			var category = item as Category;
			if (category?.Children?.Count > 0)
				throw new InvalidTagOperationException (category, "Cannot remove category that contains children");

			//Why set this to null ?
			//item.Category = null;

			Context.Remove (item);
			Context.SaveChanges ();

			EmitRemoved (item);
		}

		string GetIconString (Tag tag)
		{
			//if (tag.ThemeIconName != null)
			//	return StockIconDbPrefix + tag.Icon;

			//if (tag.Icon == null) {
			//	if (tag.IconWasCleared)
			//		return string.Empty;
			//	return null;
			//}

			//byte[] data = GdkUtils.Serialize (tag.Icon);
			//return Convert.ToBase64String (data);
			return string.Empty;
		}

		public override void Commit (Tag item)
		{
			Commit (item, false);
		}

		public void Commit (Tag tag, bool updateXmp = false)
		{
			Commit (new[] { tag }, updateXmp);
		}

		public void Commit (Tag[] tags, bool updateXmp)
		{
			//foreach (Tag tag in tags) {
			//	Database.Execute (new HyenaSqliteCommand ("UPDATE tags SET name = ?, category_id = ?, "
			//				+ "is_category = ?, sort_priority = ?, icon = ? WHERE id = ?",
			//					  tag.Name,
			//					  tag.Category.Id,
			//					  tag is Category ? 1 : 0,
			//					  tag.SortPriority,
			//					  GetIconString (tag),
			//					  tag.Id));

			//	if (updateXmp && Preferences.Get<bool> (Preferences.MetadataEmbedInImage)) {
			//		Photo[] photos = Db.Photos.Query (new TagTerm (tag));
			//		foreach (Photo p in photos)
			//			if (p.HasTag (tag)) // the query returns all the pics of the tag and all its child. this avoids updating child tags
			//				SyncMetadataJob.Create (Db.Jobs, p);
			//	}
			//	context.Tags.AddRange (tag);
			//}
			//context.SaveChanges ();

			//EmitChanged (tags);
		}
	}
}
