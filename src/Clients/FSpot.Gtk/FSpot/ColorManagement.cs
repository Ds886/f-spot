//
// ColorManagement.cs
//
// Author:
//   Stephane Delcroix <stephane@delcroix.org>
//   Stephen Shaw <sshaw@decriptor.com>
//
// Copyright (C) 2014 Stephen Shaw
// Copyright (C) 2008-2010 Novell, Inc.
// Copyright (C) 2008-2010 Stephane Delcroix
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Collections.Generic;

using FSpot.Cms;
using FSpot.Settings;

namespace FSpot
{
	public static class ColorManagement
	{
		static string [] search_dir = { "/usr/share/color/icc", Path.Combine (Configuration.HomeDirectory, ".color/icc"), "/usr/local/share/color/icc " };

		static Dictionary<string, Profile> profiles;
		public static IDictionary<string, Profile> Profiles {
			get {
				if (profiles == null)
					BuildProfiles ();

				return profiles;
			}
		}

		static Profile x_profile;
		public static Profile XProfile {
			get {
				// FIXME, Windows Port
				return null;
				return x_profile ??= Profile.GetScreenProfile(Gdk.Screen.Default);
			}
		}

		static void BuildProfiles ()
		{
			profiles = new Dictionary<string, Profile> ();
			// FIXME,
			return;

			var p = Profile.CreateStandardRgb ();
			if (!profiles.ContainsKey (p.ProductDescription))
				profiles.Add (p.ProductDescription, p);

			p = Profile.CreateAlternateRgb ();
			if (!profiles.ContainsKey (p.ProductDescription))
				profiles.Add (p.ProductDescription, p);

			foreach (var path in search_dir)
				if (!profiles.ContainsKey (path))
					AddProfiles (path);

			if (XProfile != null)
				if (!profiles.ContainsKey ("_x_profile_"))
					profiles.Add ("_x_profile_", XProfile);
		}

		static void AddProfiles (string path)
		{
			//recursive search, only RGB color profiles would be added
			if (!Directory.Exists (path))
				return;

			AddProfilesByExtension (path, "*.icc");
			AddProfilesByExtension (path, "*.icm");

			var DirList = Directory.GetDirectories (path);
				foreach (string dir in DirList)
					AddProfiles (dir);
		}

		static void AddProfilesByExtension (string path, string fileExtension)
		{
			var colorProfilList = Directory.GetFiles (path, fileExtension);
			foreach (string ColorProfilePath in colorProfilList) {
				try {
					var profile = new Profile (ColorProfilePath);
					if (profile.ColorSpace == IccColorSpace.Rgb && profile.ProductDescription != null && !profiles.ContainsKey (profile.ProductDescription))
						profiles.Add (profile.ProductDescription, profile);
				}
				catch (CmsException CmsEx) {
					Console.WriteLine (CmsEx);
				}
			}
		}

		public static void ApplyProfile (Gdk.Pixbuf pixbuf, Profile destinationProfile)
		{
			ApplyProfile (pixbuf, Profile.CreateStandardRgb (), destinationProfile);
		}

		public static void ApplyProfile (Gdk.Pixbuf pixbuf, Profile imageProfile, Profile destinationProfile)
		{
			if (pixbuf == null || pixbuf.HasAlpha)
				return;

			imageProfile = imageProfile ?? Profile.CreateStandardRgb ();

			Profile [] list = { imageProfile, destinationProfile };
			var transform = new Transform (list,
							     PixbufUtils.PixbufCmsFormat (pixbuf),
							     PixbufUtils.PixbufCmsFormat (pixbuf),
							     Intent.Perceptual,
							     0x0000);
			PixbufUtils.ColorAdjust (pixbuf, pixbuf, transform);
		}
	}
}
