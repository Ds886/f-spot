using System;
using System.IO;

namespace FSpot {
	public class ImageFormatException : ApplicationException {
		public ImageFormatException (string msg) : base (msg)
		{
		}
	}

	public class ImageFile {
		protected Uri uri;

		static System.Collections.Hashtable name_table;

		public ImageFile (string path) 
		{
			this.uri = UriList.PathToFileUri (path);
		}
		
		public ImageFile (Uri uri)
		{
			this.uri = uri;
		}
		
		protected Stream Open ()
		{
			if (uri.Scheme == "file:")
				return File.OpenRead (uri.LocalPath);

			System.Console.WriteLine ("open uri = {0}", uri.ToString ());
			return new Gnome.Vfs.VfsStream (uri.ToString (), FileMode.Open);
		}

		public virtual Stream PixbufStream ()
		{
			return Open ();
		}

		static ImageFile ()
		{
			name_table = new System.Collections.Hashtable ();
			name_table [".svg"] = typeof (FSpot.Svg.SvgFile);
			name_table [".gif"] = typeof (ImageFile);
			name_table [".jpeg"] = typeof (JpegFile);
			name_table [".jpg"] = typeof (JpegFile);
			name_table [".png"] = typeof (FSpot.Png.PngFile);
			name_table [".cr2"] = typeof (FSpot.Tiff.Cr2File);
			name_table [".nef"] = typeof (FSpot.Tiff.NefFile);
			name_table [".pef"] = typeof (FSpot.Tiff.NefFile);
			name_table [".raw"] = typeof (FSpot.Tiff.NefFile);
			name_table [".tiff"] = typeof (FSpot.Tiff.TiffFile);
			name_table [".tif"] = typeof (FSpot.Tiff.TiffFile);
			name_table [".orf"] =  typeof (FSpot.Tiff.NefFile);
			name_table [".srf"] = typeof (FSpot.Tiff.NefFile);
			name_table [".dng"] = typeof (FSpot.Tiff.DngFile);
			name_table [".crw"] = typeof (FSpot.Ciff.CiffFile);
			name_table [".ppm"] = typeof (FSpot.Pnm.PnmFile);
			name_table [".mrw"] = typeof (FSpot.Mrw.MrwFile);
			name_table [".raf"] = typeof (FSpot.Raf.RafFile);
			name_table [".x3f"] = typeof (FSpot.X3f.X3fFile);
		}

		public Uri Uri {
			get { return this.uri; }
		}

		public PixbufOrientation Orientation {
			get { return GetOrientation (); }
		}

		public virtual string Description
		{
			get { return null; }
		}
		
		public virtual void Save (Gdk.Pixbuf pixbuf, System.IO.Stream stream)
		{
			throw new NotImplementedException ();
		}

		protected Gdk.Pixbuf TransformAndDispose (Gdk.Pixbuf orig)
		{
			if (orig == null)
				return null;

			Gdk.Pixbuf rotated = PixbufUtils.TransformOrientation (orig, this.Orientation, true);
			//ValidateThumbnail (photo, rotated);
			if (rotated != orig)
				orig.Dispose ();
			
			return rotated;
		}
		
		public virtual Gdk.Pixbuf Load ()
		{
			Gdk.Pixbuf orig = new Gdk.Pixbuf (Open ());
			return TransformAndDispose (orig);
		}
		
		public virtual Gdk.Pixbuf Load (int max_width, int max_height)
		{
			System.IO.Stream stream = PixbufStream ();
			if (stream == null) {
				Gdk.Pixbuf orig = this.Load ();
				Gdk.Pixbuf scaled = PixbufUtils.ScaleToMaxSize (orig,  max_width, max_height);	
				orig.Dispose ();
				return scaled;
			}

			using (stream) {
				PixbufUtils.AspectLoader aspect = new PixbufUtils.AspectLoader (max_width, max_height);
				return aspect.Load (stream, Orientation);
			}	
		}
	
		public virtual PixbufOrientation GetOrientation () 
		{
			return PixbufOrientation.TopLeft;
		}
		
		// FIXME this need to have an intent just like the loading stuff.
		public virtual Cms.Profile GetProfile ()
		{
			return null;
		}
		
		public virtual System.DateTime Date 
		{
			get {
				// FIXME mono uses the file change time (ctime) incorrectly
				// as the creation time so we try to work around that slightly
				Gnome.Vfs.FileInfo info = new Gnome.Vfs.FileInfo (uri.ToString ());

				DateTime create = info.Ctime;
				DateTime write = info.Mtime;

				if (create < write)
					return create;
				else 
					return write;
			}
		}

		public static bool HasLoader (string path)
		{
			return HasLoader (UriList.PathToFileUri (path));
		}
		
		public static bool HasLoader (Uri uri)
		{
			string path = uri.AbsolutePath;
			string extension = System.IO.Path.GetExtension (path).ToLower ();
			System.Type t = (System.Type) name_table [extension];
			
			return (t != null);
		}

		public static ImageFile Create (string path)
		{
			return Create (UriList.PathToFileUri (path));
		}

		public static ImageFile Create (Uri uri)
		{
			string path = uri.AbsolutePath;
			string extension = System.IO.Path.GetExtension (path).ToLower ();
			System.Type t = (System.Type) name_table [extension];
			ImageFile img;

			if (t != null)
				img = (ImageFile) System.Activator.CreateInstance (t, new object[] { uri });
			else 
				img = new ImageFile (uri);

			return img;
		}
	} 
}
