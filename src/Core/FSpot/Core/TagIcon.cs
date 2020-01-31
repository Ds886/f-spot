// Copyright (C) 2020 Stephen Shaw
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;

using FSpot.Models;
using FSpot.Settings;
using FSpot.Utils;

using Gdk;

namespace FSpot.Core
{
	public class TagIcon : IDisposable
	{
		readonly Tag tag;
		Pixbuf icon;

		public TagIcon (Tag tag)
		{
			this.tag = tag;
		}

		public Pixbuf Icon {
			get {
				if (icon == null && tag.ThemeIconName != null) {
					cached_icon_size = IconSize.Hidden;
					icon = GtkUtil.TryLoadIcon (FSpotConfiguration.IconTheme, tag.ThemeIconName, 48,
						(Gtk.IconLookupFlags)0);
				}

				return icon;
			}
			set {
				tag.ThemeIconName = null;
				icon?.Dispose ();
				icon = value;
				cached_icon_size = IconSize.Hidden;
				tag.IconWasCleared = value == null;
			}
		}

		public static IconSize TagIconSize { get; } = IconSize.Large;

		Pixbuf cached_icon;
		IconSize cached_icon_size = IconSize.Hidden;

		// We can use a SizedIcon everywhere we were using an Icon
		public Pixbuf SizedIcon {
			get {
				if (TagIconSize == IconSize.Hidden) //Hidden
					return null;
				if (TagIconSize == cached_icon_size)
					return cached_icon;
				if (tag.ThemeIconName != null) { //Theme icon
					cached_icon?.Dispose ();
					try {
						cached_icon = GtkUtil.TryLoadIcon (FSpotConfiguration.IconTheme, tag.ThemeIconName, (int)TagIconSize, (Gtk.IconLookupFlags)0);

						if (Math.Max (cached_icon.Width, cached_icon.Height) <= (int)TagIconSize)
							return cached_icon;
					} catch (Exception) {
						Console.WriteLine ($"missing theme icon: {tag.ThemeIconName}");
						return null;
					}
				}
				if (tag.Icon == null)
					return null;

				if (Math.Max (icon.Width, icon.Height) >= (int)TagIconSize) { //Don't upscale
					cached_icon?.Dispose ();
					cached_icon = icon.ScaleSimple ((int)TagIconSize, (int)TagIconSize, InterpType.Bilinear);
					cached_icon_size = TagIconSize;
					return cached_icon;
				}
				return icon;
			}
		}

		public void Dispose ()
		{
			icon?.Dispose ();
			cached_icon?.Dispose ();
		}
	}
}
