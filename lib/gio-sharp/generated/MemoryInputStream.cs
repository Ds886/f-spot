// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLib {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

#region Autogenerated code
	public class MemoryInputStream : GLib.InputStream, GLib.Seekable {

		[Obsolete]
		protected MemoryInputStream(GLib.GType gtype) : base(gtype) {}
		public MemoryInputStream(IntPtr raw) : base(raw) {}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_memory_input_stream_new();

		public MemoryInputStream () : base (IntPtr.Zero)
		{
			if (GetType () != typeof (MemoryInputStream)) {
				CreateNativeObject (new string [0], new GLib.Value[0]);
				return;
			}
			Raw = g_memory_input_stream_new();
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_memory_input_stream_new_from_data(IntPtr data, IntPtr len, GLib.DestroyNotify destroy);

		public MemoryInputStream (IntPtr data, long len, GLib.DestroyNotify destroy) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (MemoryInputStream)) {
				throw new InvalidOperationException ("Can't override this constructor.");
			}
			Raw = g_memory_input_stream_new_from_data(data, new IntPtr (len), destroy);
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_memory_input_stream_get_type();

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = g_memory_input_stream_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern void g_memory_input_stream_add_data(IntPtr raw, IntPtr data, IntPtr len, GLib.DestroyNotify destroy);

		public void AddData(IntPtr data, long len, GLib.DestroyNotify destroy) {
			g_memory_input_stream_add_data(Handle, data, new IntPtr (len), destroy);
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern bool g_seekable_can_truncate(IntPtr raw);

		public bool CanTruncate() {
			bool raw_ret = g_seekable_can_truncate(Handle);
			bool ret = raw_ret;
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern long g_seekable_tell(IntPtr raw);

		public long Position { 
			get {
				long raw_ret = g_seekable_tell(Handle);
				long ret = raw_ret;
				return ret;
			}
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern bool g_seekable_truncate(IntPtr raw, long offset, IntPtr cancellable, out IntPtr error);

		public bool Truncate(long offset, GLib.Cancellable cancellable) {
			IntPtr error = IntPtr.Zero;
			bool raw_ret = g_seekable_truncate(Handle, offset, cancellable == null ? IntPtr.Zero : cancellable.Handle, out error);
			bool ret = raw_ret;
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern bool g_seekable_seek(IntPtr raw, long offset, GLib.SeekType type, IntPtr cancellable, out IntPtr error);

		public bool Seek(long offset, GLib.SeekType type, GLib.Cancellable cancellable) {
			IntPtr error = IntPtr.Zero;
			bool raw_ret = g_seekable_seek(Handle, offset, type, cancellable == null ? IntPtr.Zero : cancellable.Handle, out error);
			bool ret = raw_ret;
			if (error != IntPtr.Zero) throw new GLib.GException (error);
			return ret;
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern bool g_seekable_can_seek(IntPtr raw);

		public bool CanSeek { 
			get {
				bool raw_ret = g_seekable_can_seek(Handle);
				bool ret = raw_ret;
				return ret;
			}
		}

#endregion
	}
}