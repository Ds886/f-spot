using System;
using System.IO;
using FSpot.Xmp;
using FSpot.Tiff;
using FSpot.Utils;
using Hyena;
using TagLib;

namespace FSpot {
	public interface IThumbnailContainer {
		Gdk.Pixbuf GetEmbeddedThumbnail ();
	}

	public class JpegFile : ImageFile, IThumbnailContainer {
        public Image.File Metadata {
            get { return metadata_file; }
        }

        private Image.File metadata_file;
		
		public JpegFile (SafeUri uri) : base (uri)
		{
            metadata_file = TagLib.File.Create (new GIOTagLibFileAbstraction () { Uri = uri }) as Image.File;
		}
		
		public override Cms.Profile GetProfile ()
		{
			return null;
		}

		public override string Description {
			get {
                return metadata_file.ImageTag.Comment;
			}
		}

		public void SetDescription (string value)
		{
            metadata_file.GetTag (TagTypes.XMP, true); // Ensure XMP tag
            metadata_file.ImageTag.Comment = value;
		}

		private void UpdateMeta ()
		{
            metadata_file.GetTag (TagTypes.XMP, true); // Ensure XMP tag
            metadata_file.ImageTag.Software = FSpot.Defines.PACKAGE + " version " + FSpot.Defines.VERSION;
		}

		/*private void SaveMetaData (System.IO.Stream input, System.IO.Stream output)
		{
			JpegHeader header = new JpegHeader (input);
			UpdateMeta ();
			
			// Console.WriteLine ("updated metadata");
			header.SetExif (this.ExifData);
			// Console.WriteLine ("set exif");
			if (xmp != null)
				header.SetXmp (xmp);
			// Console.WriteLine ("set xmp");
			header.Save (output);
			// Console.WriteLine ("saved");
		}*/
		
		public void SaveMetaData (string path)
		{
            // FIXME: This currently copies the file out to a tmp file, overwrites it
            // and restores the tmp file in case of failure. Should obviously be the
            // other way around, but Taglib# doesn't have an interface to do this.
            // https://bugzilla.gnome.org/show_bug.cgi?id=618768

            var uri = UriUtils.PathToFileUri (path);
            var tmp = System.IO.Path.GetTempFileName ();
            var tmp_uri = UriUtils.PathToFileUri (tmp);

            var orig_file = GLib.FileFactory.NewForUri (uri);
            var tmp_file = GLib.FileFactory.NewForUri (tmp_uri);

            orig_file.Copy (tmp_file, GLib.FileCopyFlags.AllMetadata, null, null);

            try {
                metadata_file.Save ();
            } catch (Exception) {
                tmp_file.Copy (orig_file, GLib.FileCopyFlags.AllMetadata, null, null);
            }
		}

		public void SetThumbnail (Gdk.Pixbuf source)
		{
			/*// Then create the thumbnail
			// The DCF spec says thumbnails should be 160x120 always
			Gdk.Pixbuf thumbnail = PixbufUtils.ScaleToAspect (source, 160, 120);
			byte [] thumb_data = PixbufUtils.Save (thumbnail, "jpeg", null, null);
			
			// System.Console.WriteLine ("saving thumbnail");				

			// now update the exif data
			ExifData.Data = thumb_data;*/
            // FIXME: needs to be readded https://bugzilla.gnome.org/show_bug.cgi?id=618769
		}

		public void SetDimensions (int width, int height)
		{
			/* FIXME: disabled, related to metadata copying
             * https://bugzilla.gnome.org/show_bug.cgi?id=618770
             * Exif.ExifEntry e;
			Exif.ExifContent thumb_content;
			
			// update the thumbnail related image fields if they exist.
			thumb_content = this.ExifData.GetContents (Exif.Ifd.One);
			e = thumb_content.Lookup (Exif.Tag.RelatedImageWidth);
			if (e != null)
				e.SetData ((uint)width);

			e = thumb_content.Lookup (Exif.Tag.RelatedImageHeight);
			if (e != null)
				e.SetData ((uint)height);
			
			Exif.ExifContent image_content;
			image_content = this.ExifData.GetContents (Exif.Ifd.Zero);
			image_content.GetEntry (Exif.Tag.Orientation).SetData ((ushort)PixbufOrientation.TopLeft);
			//image_content.GetEntry (Exif.Tag.ImageWidth).SetData ((uint)pixbuf.Width);
			//image_content.GetEntry (Exif.Tag.ImageHeight).SetData ((uint)pixbuf.Height);
			image_content.GetEntry (Exif.Tag.PixelXDimension).SetData ((uint)width);
			image_content.GetEntry (Exif.Tag.PixelYDimension).SetData ((uint)height);*/
		}

		public override void Save (Gdk.Pixbuf pixbuf, System.IO.Stream stream)
		{

			// Console.WriteLine ("starting save");
			// First save the imagedata
			int quality = metadata_file.Properties.PhotoQuality;
			quality = quality == 0 ? 75 : quality;
			byte [] image_data = PixbufUtils.Save (pixbuf, "jpeg", new string [] {"quality" }, new string [] { quality.ToString () });
			System.IO.MemoryStream buffer = new System.IO.MemoryStream ();
			buffer.Write (image_data, 0, image_data.Length);
/*			FIXME: Metadata copying doesn't work yet https://bugzilla.gnome.org/show_bug.cgi?id=618770
			buffer.Position = 0;
			
			// Console.WriteLine ("setting thumbnail");
			SetThumbnail (pixbuf);
			SetDimensions (pixbuf.Width, pixbuf.Height);
			pixbuf.Dispose ();
			
			// Console.WriteLine ("saving metatdata");
			SaveMetaData (buffer, stream);
			// Console.WriteLine ("done");*/
			buffer.Close ();
		}
		
		public Gdk.Pixbuf GetEmbeddedThumbnail ()
		{
			/*if (this.ExifData.Data.Length > 0) {
				MemoryStream mem = new MemoryStream (this.ExifData.Data);
				Gdk.Pixbuf thumb = new Gdk.Pixbuf (mem);
				Gdk.Pixbuf rotated = FSpot.Utils.PixbufUtils.TransformOrientation (thumb, this.Orientation);
				thumb.Dispose ();
				
				mem.Close ();
				return rotated;
			}*/
            // FIXME: No thumbnail support in TagLib# https://bugzilla.gnome.org/show_bug.cgi?id=618769
			return null;
		}
		
		public override PixbufOrientation GetOrientation () 
		{
            var orientation = metadata_file.ImageTag.Orientation;
			return (PixbufOrientation) orientation;
		}
		
		public void SetOrientation (PixbufOrientation orientation)
		{
            metadata_file.ImageTag.Orientation = (Image.ImageOrientation) orientation;
		}
		
		public void SetDateTimeOriginal (DateTime time)
		{
            metadata_file.ImageTag.DateTime = time;
		}

		public override System.DateTime Date {
			get {
                var date = metadata_file.ImageTag.DateTime;
                return date.HasValue ? date.Value : base.Date;
			}
		}

	}
}
