//
// Copyright (C) 2009 Novell, Inc.
// Copyright (C) 2009 Stephane Delcroix
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  

using GtkBeans;

namespace FSpot.UI.Dialog
{
	public abstract class BuilderDialog : Gtk.Dialog
	{
		protected BuilderDialog (string resourceName, string dialogName) : this (null, resourceName, dialogName)
		{
		}

		protected BuilderDialog (System.Reflection.Assembly assembly, string resourceName, string dialogName) : this (new Builder (assembly, resourceName, null), dialogName)
		{
		}

		protected BuilderDialog (Builder builder, string dialogName) : base (builder.GetRawObject (dialogName))
		{
			builder.Autoconnect (this);
		}
	}
}