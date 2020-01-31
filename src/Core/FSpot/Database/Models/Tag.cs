// Copyright (C) 2020 Stephen Shaw
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using FSpot.Core;
using FSpot.Settings;

namespace FSpot.Models
{
	public partial class Tag : BaseDbSet, IComparable<Tag>
	{
		[NotMapped]
		public long OldId { get; set; }
		[NotMapped]
		public long OldCategoryId { get; set; }
		public string Name { get; set; }
		public Guid CategoryId { get; set; }
		public bool IsCategory { get; set; }
		public long SortPriority { get; set; }
		public string Icon { get; set; }


		[NotMapped]
		public int Popularity { get; set; }

		Category category;
		[NotMapped]
		public Category Category {
			get { return category; }
			set {
				Category?.RemoveChild (this);

				category = value;
				category?.AddChild (this);
			}
		}

		[NotMapped]
		public bool IconWasCleared { get; set; }

		// Icon.  If ThemeIconName is not null, then we save the name of the icon instead
		// of the actual icon data.
		[NotMapped]
		public string ThemeIconName { get; set; }

		[NotMapped]
		public static IconSize TagIconSize { get; set; } = IconSize.Large;

		public Tag ()
		{
			Popularity = 0;
			IconWasCleared = false;
		}

		public Tag (Category category)
		{
			Category = category;
			Popularity = 0;
			IconWasCleared = false;
			TagIcon = new TagIcon (this);
		}

		[NotMapped]
		public TagIcon TagIcon { get; }

		public int CompareTo (Tag otherTag)
		{
			if (otherTag == null)
				throw new ArgumentException (nameof (otherTag));

			if (Category == otherTag.Category) {
				if (SortPriority == otherTag.SortPriority)
					return string.Compare (Name, otherTag.Name, StringComparison.OrdinalIgnoreCase);

				return (int)(SortPriority - otherTag.SortPriority);
			}

			return Category.CompareTo (otherTag.Category);
		}

		public bool IsAncestorOf (Tag tag)
		{
			if (tag == null)
				throw new ArgumentNullException (nameof (tag));

			for (Category parent = tag.Category; parent != null; parent = parent.Category) {
				if (parent == this)
					return true;
			}

			return false;
		}
	}
}
