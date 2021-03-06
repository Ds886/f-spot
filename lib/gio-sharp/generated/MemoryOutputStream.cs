// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace GLib {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

#region Autogenerated code
	public class MemoryOutputStream : GLib.OutputStream, GLib.Seekable {

		[Obsolete]
		protected MemoryOutputStream(GLib.GType gtype) : base(gtype) {}
		public MemoryOutputStream(IntPtr raw) : base(raw) {}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_memory_output_stream_new(IntPtr data, UIntPtr len, GLibSharp.ReallocFuncNative realloc_fn, GLib.DestroyNotify destroy);

		public MemoryOutputStream (IntPtr data, ulong len, GLib.ReallocFunc realloc_fn, GLib.DestroyNotify destroy) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (MemoryOutputStream)) {
				throw new InvalidOperationException ("Can't override this constructor.");
			}
			GLibSharp.ReallocFuncWrapper realloc_fn_wrapper = new GLibSharp.ReallocFuncWrapper (realloc_fn);
			Raw = g_memory_output_stream_new(data, new UIntPtr (len), realloc_fn_wrapper.NativeDelegate, destroy);
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_memory_output_stream_get_data(IntPtr raw);

		public new IntPtr Data { 
			get {
				IntPtr raw_ret = g_memory_output_stream_get_data(Handle);
				IntPtr ret = raw_ret;
				return ret;
			}
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern UIntPtr g_memory_output_stream_get_size(IntPtr raw);

		public ulong Size { 
			get {
				UIntPtr raw_ret = g_memory_output_stream_get_size(Handle);
				ulong ret = (ulong) raw_ret;
				return ret;
			}
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern UIntPtr g_memory_output_stream_get_data_size(IntPtr raw);

		public ulong DataSize { 
			get {
				UIntPtr raw_ret = g_memory_output_stream_get_data_size(Handle);
				ulong ret = (ulong) raw_ret;
				return ret;
			}
		}

		[DllImport("libgio-2.0-0.dll")]
		static extern IntPtr g_memory_output_stream_get_type();

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = g_memory_output_stream_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
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
