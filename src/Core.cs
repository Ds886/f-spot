using System.IO;
using System;
using Mono.Posix;

namespace FSpot {
	[DBus.Interface ("org.gnome.FSpot.Core")]
	public abstract class CoreControl {
		[DBus.Method]
		public abstract void Import (string path);

		[DBus.Method]
		public abstract void Organize ();
		
		[DBus.Method]
		public abstract void View (string path);

		[DBus.Method]
		public abstract void Shutdown ();
	}

	public class Core : CoreControl
	{
		MainWindow organizer;
		private static Db db;
		static DBus.Connection connection;
		System.Collections.ArrayList toplevels;

		public Core ()
		{
			toplevels = new System.Collections.ArrayList ();
					
			// Load the database, upgrading/creating it as needed
			string base_directory = FSpot.Global.BaseDirectory;
			if (! File.Exists (base_directory))
				Directory.CreateDirectory (base_directory);
			
			db = new Db ();
			db.Init (Path.Combine (base_directory, "photos.db"), true);
		}

		public static DBus.Connection Connection {
			get {
				if (connection == null)
					connection = DBus.Bus.GetSessionBus ();

				return connection;
			}
		}

		public static Db Database {
			get { return db; }
		}

		public static Core FindInstance ()
		{
			DBus.Service service = DBus.Service.Get (Connection, "org.gnome.FSpot");
			return (Core)service.GetObject (typeof (Core), "/org/gnome/FSpot/Core");
		}

		public void RegisterServer ()
		{
			DBus.Service service = new DBus.Service (Connection, "org.gnome.FSpot");
			service.RegisterObject (this, "/org/gnome/FSpot/Core");
		}
		
		private class ImportCommand 
		{
			string path;
			MainWindow main;

			public ImportCommand (MainWindow main, string path) 
			{
				this.main = main;
				this.path = path;
			}

			public bool Execute ()
			{
				if (path != null && path.StartsWith ("gphoto2:"))
					main.ImportCamera (path);
				else
					main.ImportFile (path);
				
				return false;
			}
		}

		public override void Import (string path) 
		{
			ImportCommand cmd = new ImportCommand (MainWindow, path);
			//cmd.Execute ();
			GLib.Idle.Add (new GLib.IdleHandler (cmd.Execute));
		}

		public MainWindow MainWindow {
			get {
				if (organizer == null) {
					organizer = new MainWindow (db);
					Register (organizer.Window);
				}
				
				return organizer;
			}
		}
			
		public override void Organize ()
		{
			MainWindow.Window.Present ();
		}
		
		public override void View (string path)
		{
			if (System.IO.File.Exists (path) || System.IO.Directory.Exists (path))
				Register (new FSpot.SingleView (path).Window);
			else {
				try {
					Register (new FSpot.SingleView (new Uri (path)).Window);
				} catch (System.Exception e) {
					System.Console.WriteLine (e.ToString ());
					System.Console.WriteLine ("no real valid path to view from {0}", path);
				}
			} 
		}
		
		private class SlideShow
		{
			SlideView slideview;
			Gtk.Window window;
			
			public Gtk.Window Window {
				get { return window; }
			}

			public SlideShow (string name)
			{
				Tag tag;
				
				if (name != null)
					tag = db.Tags.GetTagByName (name);
				else {
					int id = (int) Preferences.Get (Preferences.SCREENSAVER_TAG);
					tag = db.Tags.GetTagById (id);
				}
				
				Photo [] photos;
				if (tag != null)
					photos = db.Photos.Query (new Tag [] { tag } );
				else
					photos = new Photo [0];

				window = new XScreenSaverSlide ();
				SetStyle (window);
				if (photos.Length > 0) {
					Array.Sort (photos, new Photo.RandomSort ());
					
					Gdk.Pixbuf black = new Gdk.Pixbuf (Gdk.Colorspace.Rgb, false, 8, 1, 1);
					black.Fill (0x00000000);
					slideview = new SlideView (black, photos);
					window.Add (slideview);
				} else {
					Gtk.HBox outer = new Gtk.HBox ();
					Gtk.HBox hbox = new Gtk.HBox ();
					Gtk.VBox vbox = new Gtk.VBox ();

					outer.PackStart (new Gtk.Label (""));
					outer.PackStart (vbox, false, false, 0);
					vbox.PackStart (new Gtk.Label (""));
					vbox.PackStart (hbox, false, false, 0);
					hbox.PackStart (new Gtk.Image (Gtk.Stock.DialogWarning, Gtk.IconSize.Dialog),
							false, false, 0);
					outer.PackStart (new Gtk.Label (""));

					string msg;
					string long_msg;

					if (tag != null) {
						msg = String.Format (Catalog.GetString ("No photos matching {0} found"), tag.Name);
						long_msg = String.Format (Catalog.GetString ("The tag \"{0}\" is not applied to any photos. Try adding\n" +
											     "the tag to some photos or selecting a different tag in the\n" +
											     "F-Spot preference dialog."), tag.Name);
					} else {
						msg = Catalog.GetString ("Search returned no results");
						long_msg = Catalog.GetString ("The tag F-Spot is looking for does not exist. Try\n" +
									      "selecting a different tag in the F-Spot preference\n" +
									      "dialog.");
					}

					Gtk.Label label = new Gtk.Label (msg);
					hbox.PackStart (label, false, false, 0);

					Gtk.Label long_label = new Gtk.Label (long_msg);
					long_label.Markup  = String.Format ("<small>{0}</small>", long_msg);

					vbox.PackStart (long_label, false, false, 0);
					vbox.PackStart (new Gtk.Label (""));

					window.Add (outer);
					SetStyle (label);
					SetStyle (long_label);
					//SetStyle (image);
				}
				window.ShowAll ();
			}

			private void SetStyle (Gtk.Widget w) 
			{
				w.ModifyFg (Gtk.StateType.Normal, new Gdk.Color (127, 127, 127));
				w.ModifyBg (Gtk.StateType.Normal, new Gdk.Color (0, 0, 0));
			}

			public bool Execute ()
			{
				if (slideview != null)
					slideview.Play ();
				return false;
			}
		}

		public void ShowSlides (string name)
		{
			SlideShow show = new SlideShow (name);
			Register (show.Window);
			GLib.Idle.Add (new GLib.IdleHandler (show.Execute));
		}


		public override void Shutdown ()
		{
			System.Environment.Exit (0);
		}

		public void Register (Gtk.Window window)
		{
			toplevels.Add (window);
			window.Destroyed += HandleDestroyed;
		}

		public void HandleDestroyed (object sender, System.EventArgs args)
		{
			toplevels.Remove (sender);
			if (toplevels.Count == 0) {
				// FIXME
				// Should use Application.Quit(), but for that to work we need to terminate the threads
				// first too.
				System.Environment.Exit (0);
			}
			if (organizer != null && organizer.Window == sender)
				organizer = null;
		}
	}
}
