//
// FSpot.Widgets.ImageView.cs
//
// Author(s):
//	Stephane Delcroix  <stephane@delcroix.org>
//
// This is free software. See COPYING for details.
//

using System;

using Gtk;
using Gdk;

namespace FSpot.Widgets
{
	public class ImageView : Layout
	{
		public static double ZOOM_FACTOR = 1.1;

		protected double max_zoom = 10.0;
		protected double MAX_ZOOM {
			get { return max_zoom; }
		}

		protected double min_zoom = 0.1;
		protected double MIN_ZOOM {
			get { return min_zoom; }
		}

		public ImageView () : base (null, null)
		{
		}

		public Pixbuf Pixbuf {
			get { throw new NotImplementedException ();} 
			set { throw new NotImplementedException ();} 
		}

		public int CheckSize {
			get { throw new NotImplementedException ();} 
			set { throw new NotImplementedException ();} 
		}

		public PointerMode PointerMode {
			get { throw new NotImplementedException ();} 
			set { throw new NotImplementedException ();} 
		}

		public double SelectionXyRatio {
			get { throw new NotImplementedException ();} 
			set { throw new NotImplementedException ();} 
		}

		Cms.Transform transform;
		public Cms.Transform Transform {
			get { return transform; } 
			set { transform = value;} 
		}

		public Gdk.InterpType Interpolation {
			get { throw new NotImplementedException ();} 
			set { throw new NotImplementedException ();} 
		}


		public void GetZoom (out double zoomx, out double zoomy)
		{
			throw new NotImplementedException ();	
		}

		public void SetZoom (double zoom_x, double zoom_y)
		{
			throw new NotImplementedException ();	
		}
		
		Gdk.Color transparent_color = this.Style.BaseColors [(int)Gtk.StateType.Normal];
		public Gdk.Color TransparentColor {
			get { return transparent_color; }
			set { transparent_color = value; }
		}

		[Obsolete ("Use the TransparentColor property")]
		public void SetTransparentColor (Gdk.Color color)
		{
			TransparentColor = color;
		} 

		public void SetTransparentColor (string color) //format "#000000"
		{
			TransparentColor  = new Gdk.Color (
					Byte.Parse (color.Substring (1,2), System.Globalization.NumberStyles.AllowHexSpecifier),
					Byte.Parse (color.Substring (3,2), System.Globalization.NumberStyles.AllowHexSpecifier),
					Byte.Parse (color.Substring (5,2), System.Globalization.NumberStyles.AllowHexSpecifier)
			);
		}

		[Obsolete ("use the CheckSize Property instead")]
		public void SetCheckSize (int size)
		{
			CheckSize = size;
		}

		public Gdk.Point WindowCoordsToImage (Point win)
		{
			throw new NotImplementedException ();
		}

		public Gdk.Rectangle ImageCoordsToWindow (Gdk.Rectangle image)
		{
			int x, y;
			int width, height;
	
			if (this.Pixbuf == null)
				return Gdk.Rectangle.Zero;

			throw new NotImplementedException ();
			
//			this.GetOffsets (out x, out y, out width, out height);
//	
//			Gdk.Rectangle win = Gdk.Rectangle.Zero;
//			win.X = (int) Math.Floor (image.X * (double) (width - 1) / (this.Pixbuf.Width - 1) + 0.5) + x;
//			win.Y = (int) Math.Floor (image.Y * (double) (height - 1) / (this.Pixbuf.Height - 1) + 0.5) + y;
//			win.Width = (int) Math.Floor ((image.X + image.Width) * (double) (width - 1) / (this.Pixbuf.Width - 1) + 0.5) - win.X + x;
//			win.Height = (int) Math.Floor ((image.Y + image.Height) * (double) (height - 1) / (this.Pixbuf.Height - 1) + 0.5) - win.Y + y;
//	
//			return win;
		}

		public bool GetSelection (out int x, out int y, out int width, out int height)
		{
			throw new NotImplementedException ();	
		}

		public void UnsetSelection () 
		{
			throw new NotImplementedException ();
		}

		protected void UpdateMinZoom ()
		{
			throw new NotImplementedException ();	
		}


		public event EventHandler ZoomChanged;
		public event EventHandler SelectionChanged;

	}
}
