//
// PreferencesTests.cs
//
// Author:
//   Stephen Shaw <sshaw@decriptor.com>
//
// Copyright (C) 2019 Stephen Shaw
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO;

using FSpot.Platform;

using Newtonsoft.Json.Linq;

using NUnit.Framework;

using Shouldly;

namespace FSpot.Preferences.UnitTest
{
	[TestFixture]
	public class PreferencesTests
	{
		string TestSettingsFile;
		PreferenceJsonBackend jsonBackend;

		JObject LoadSettings (string location)
		{
			var settingsFile = File.ReadAllText (location);
			var o = JObject.Parse (settingsFile);
			return (JObject)o[PreferenceJsonBackend.SettingsRoot];
		}

		[SetUp]
		public void Setup ()
		{
			var tmpFile = Path.GetTempFileName ();
			var jsonfile = Path.ChangeExtension (tmpFile, "json");
			File.Move (tmpFile, jsonfile);
			PreferenceJsonBackend.PreferenceLocationOverride = TestSettingsFile = jsonfile;
			jsonBackend = new PreferenceJsonBackend ();
		}

		[TearDown]
		public void TearDown ()
		{
			if (File.Exists (TestSettingsFile))
				File.Delete (TestSettingsFile);
		}

		[TestCase (Settings.Preferences.StoragePath, "StoragePathString")]
		[TestCase (Settings.Preferences.ExportKey, true)]
		[TestCase (Settings.Preferences.CustomCropRatios, 0.0)]
		public void CanSetSetting (string key, object v)
		{
			jsonBackend.Set (key, v);
			jsonBackend.SaveSettings ();
			Assert.That (new FileInfo (TestSettingsFile).Length > 0);

			var settings = LoadSettings (TestSettingsFile);
			var result = settings[key].ToString ();

			Assert.AreEqual (v.ToString (), result);
		}

		[TestCase (Settings.Preferences.StoragePath, "StoragePathString")]
		public void CanGetStringSettings (string key, string v)
		{
			jsonBackend.Set (key, v);
			jsonBackend.SaveSettings ();
			Assert.That (new FileInfo (TestSettingsFile).Length > 0);

			var result = jsonBackend.Get<string> (key);

			Assert.AreEqual (v, result);
		}

		[TestCase (Settings.Preferences.ExportKey, true)]
		public void CanGetBoolSettings (string key, bool v)
		{
			jsonBackend.Set (key, v);
			jsonBackend.SaveSettings ();
			Assert.That (new FileInfo (TestSettingsFile).Length > 0);

			var result = jsonBackend.Get<bool> (key);

			Assert.AreEqual (v, result);
		}


		[TestCase (Settings.Preferences.CustomCropRatios, 0.0)]
		public void CanGetDoubleSettings (string key, double v)
		{
			jsonBackend.Set (key, v);
			jsonBackend.SaveSettings ();
			Assert.That (new FileInfo (TestSettingsFile).Length > 0);

			var result = jsonBackend.Get<double> (key);

			Assert.AreEqual (v, result);
		}

		[Test]
		public void UnsetSettingThrowsNoSuchKeyException ()
		{
			Assert.Throws<NoSuchKeyException> (() => jsonBackend.Get<bool> ("RandomKey123"));
		}

		// Preferences returns the default value instead of an exception
		[Test]
		public void PreferencesGetDefaultSetting ()
		{
			var result = Settings.Preferences.Get<bool> (Settings.Preferences.TagIconAutomatic);
			Assert.True (result);
		}

		[Test]
		public void ResetIncorrectTypeInSettings ()
		{
			Settings.Preferences.Set ("RandomKey", null);
			var result = Settings.Preferences.TryGet ("RandomKey", out double randomValue);

			result.ShouldBeTrue();
			randomValue.ShouldBe (default);
		}
	}
}
