//  GrabHandle.cs
//
//  Author:
//       Stephen Shaw <sshaw@decriptor.com>
//
//  Copyright (c) 2013 SUSE LINUX Products GmbH, Nuernberg, Germany.
//
//  Permission is hereby granted, free of charge, to any person obtaining
//  a copy of this software and associated documentation files (the
//  "Software"), to deal in the Software without restriction, including
//  without limitation the rights to use, copy, modify, merge, publish,
//  distribute, sublicense, and/or sell copies of the Software, and to
//  permit persons to whom the Software is furnished to do so, subject to
//  the following conditions:
//
//  The above copyright notice and this permission notice shall be
//  included in all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND,
//  EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//  MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//  LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//  OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//  WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//

using System;
using System.Collections.Generic;

using Mono.Unix;

using Gtk;
using Gdk;

using FSpot.Core;
using FSpot.Utils;
using FSpot.Query;

namespace FSpot.Query
{
	public class GrabHandle : DrawingArea
	{
		public GrabHandle (int w, int h)
		{
			// GTK3: Size
//			Size (w, h);
			Orientation = Gtk.Orientation.Horizontal;
			Show ();
		}

		private Gtk.Orientation orientation;
		public Gtk.Orientation Orientation {
			get { return orientation; }
			set { orientation = value; }
		}

		// GTK3: OnExposeEvent
//		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
//		{
//			bool ret = base.OnExposeEvent(evnt);
//
//			if (evnt.Window != GdkWindow) {
//				return ret;
//			}
//
//			Gtk.Style.PaintHandle(Style, GdkWindow, State, ShadowType.In,
//					      evnt.Area, this, "entry", 0, 0, Allocation.Width, Allocation.Height, Orientation);
//
//			//(Style, GdkWindow, StateType.Normal, ShadowType.In,
//			//evnt.Area, this, "entry", 0, y_mid - y_offset, Allocation.Width,
//			//Height + (y_offset * 2));
//
//			return ret;
//		}
	}
	
}