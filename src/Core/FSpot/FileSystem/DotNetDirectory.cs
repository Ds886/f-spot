//
// DotNetDirectory.cs
//
// Author:
//   Stephen Shaw <sshaw@decriptor.com>
//   Daniel Köb <daniel.koeb@peony.at>
//
// Copyright (C) 2019 Stephen Shaw
// Copyright (C) 2016 Daniel Köb
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections.Generic;
using System.IO;

using FSpot.Utils;

using Hyena;

namespace FSpot.FileSystem
{
	internal class DotNetDirectory : IDirectory
	{
		public bool Exists (SafeUri uri)
		{
			return Directory.Exists (uri.AbsolutePath);
		}

		public void CreateDirectory (SafeUri uri)
		{
			Directory.CreateDirectory (uri.AbsolutePath);
		}

		public void Delete (SafeUri uri)
		{
			if (!Exists (uri)) {
				//FIXME to be consistent with System.IO.Directory.Delete we should throw an exception in this case
				return;
			}
			Directory.Delete (uri.AbsolutePath);
		}

		public IEnumerable<SafeUri> Enumerate (SafeUri uri)
		{
			if (!Exists (uri))
				yield break;

			foreach (var file in Directory.EnumerateDirectories (uri.AbsolutePath))
				yield return uri.Append (file);
		}
	}
}
