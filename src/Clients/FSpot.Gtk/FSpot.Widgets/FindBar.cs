//
// FindBar.cs
//
// Author:
//   Gabriel Burt <gabriel.burt@gmail.com>
//   Daniel Köb <daniel.koeb@peony.at>
//   Stephane Delcroix <sdelcroix@src.gnome.org>
//
// Copyright (C) 2007-2010 Novell, Inc.
// Copyright (C) 2007 Gabriel Burt
// Copyright (C) 2010 Daniel Köb
// Copyright (C) 2007-2008 Stephane Delcroix
//
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Text.RegularExpressions;

using Gtk;

using Mono.Unix;

using FSpot.Core;
using FSpot.Query;

using Hyena;
using FSpot.Models;

namespace FSpot.Widgets
{
	public class FindBar : HighlightedBox
	{
		string last_entry_text = string.Empty;
		int open_parens;
		int close_parens;
		PhotoQuery query;
		HBox box;
		readonly object lockObject = new object ();

		public bool Completing {
			get {
				return (Entry.Completion as LogicEntryCompletion).Completing;
			}
		}

		public Entry Entry { get; }

		public Term RootTerm { get; private set; }

		public FindBar (PhotoQuery query, TreeModel model) : base (new HBox ())
		{
			this.query = query;
			box = Child as HBox;

			box.Spacing = 6;
			box.BorderWidth = 2;

			box.PackStart (new Label (Catalog.GetString ("Find:")), false, false, 0);

			Entry = new Entry ();
			Entry.Completion = new LogicEntryCompletion (Entry, model);

			Entry.TextInserted += HandleEntryTextInserted;
			Entry.TextDeleted += HandleEntryTextDeleted;
			Entry.KeyPressEvent += HandleEntryKeyPress;

			box.PackStart (Entry, true, true, 0);

			using var clearButton = new Button { new Image ("gtk-close", IconSize.Button) };
			clearButton.Clicked += HandleCloseButtonClicked;
			clearButton.Relief = ReliefStyle.None;
			box.PackStart (clearButton, false, false, 0);
		}

		void HandleCloseButtonClicked (object sender, EventArgs args)
		{
			Clear ();
		}

		void HandleEntryTextInserted (object sender, TextInsertedArgs args)
		{
			//Log.DebugFormat ("inserting {0}, ( = {1}  ) = {2}", args.Text, open_parens, close_parens);

			//int start = args.Position - args.Length;

			foreach (var c in args.Text)
			{
				if (c == '(')
					open_parens++;
				else if (c == ')')
					close_parens++;
			}

			int pos = Entry.Position + 1;
			int closeParensNeeded = open_parens - close_parens;
			for (int i = 0; i < closeParensNeeded; i++) {
				Entry.TextInserted -= HandleEntryTextInserted;
				Entry.InsertText (")", ref pos);
				close_parens++;
				Entry.TextInserted += HandleEntryTextInserted;
				pos++;
			}
			//Log.DebugFormat ("done w/ insert, {0}, ( = {1}  ) = {2}", args.Text, open_parens, close_parens);
			last_entry_text = Entry.Text;

			QueueUpdate ();
		}

		void HandleEntryTextDeleted (object sender, TextDeletedArgs args)
		{
			int length = args.EndPos - args.StartPos;
			//Log.DebugFormat ("start {0} end {1} len {2} last {3}", args.StartPos, args.EndPos, length, last_entry_text);
			string txt = length < 0 ? last_entry_text : last_entry_text.Substring (args.StartPos, length);

			foreach (var t in txt) {
				if (t == '(')
					open_parens--;
				else if (t == ')')
					close_parens--;
			}

			last_entry_text = Entry.Text;

			QueueUpdate ();
		}

		void HandleEntryKeyPress (object sender, KeyPressEventArgs args)
		{
			//bool shift = ModifierType.ShiftMask == (args.Event.State & ModifierType.ShiftMask);

			switch (args.Event.Key) {
			case (Gdk.Key.Escape):
				Clear ();
				args.RetVal = true;
				break;

			case (Gdk.Key.Tab):
				// If we are at the end of the entry box, let the normal Tab handler do its job
				if (Entry.Position == Entry.Text.Length) {
					args.RetVal = false;
					return;
				}

				// Go until the current character is an open paren
				while (Entry.Position < Entry.Text.Length && Entry.Text[Entry.Position] != '(')
					Entry.Position++;

				// Put the cursor right after the open paren
				Entry.Position++;

				args.RetVal = true;
				break;

			default:
				args.RetVal = false;
				break;
			}
		}

		void Clear ()
		{
			Entry.Text = string.Empty;
			Hide ();
		}

		// OPS The operators we support, case insensitive
		//private static string op_str = "(?'Ops' or | and |, | \\s+ )";
		static readonly string OpStr = "(?'Ops' " + Catalog.GetString ("or") + " | " + Catalog.GetString ("and") + " |, )";

		// Match literals, eg tags or other text to search on
		static readonly string LiteralStr = "[^{0}{1}]+?";
		//private static string not_literal_str = "not\\s*\\((?'NotTag'[^{0}{1}]+)\\)";

		// Match a group surrounded by parenthesis and one or more terms separated by operators
		static readonly string TermStr = "(((?'Open'{0})(?'Pre'[^{0}{1}]*?))+((?'Close-Open'{1})(?'Post'[^{0}{1}]*?))+)*?(?(Open)(?!))";

		// Match a group surrounded by parenthesis and one or more terms separated by operators, surrounded by not()
		//private static string not_term_str = string.Format("not\\s*(?'NotTerm'{0})", term_str);

		// Match a simple term or a group term or a not(group term)
		//private static string comb_term_str = string.Format ("(?'Term'{0}|{2}|{1})", simple_term_str, term_str, not_term_str);
		static readonly string CombTermStr =
			$"(?'Term'{LiteralStr}|{TermStr})|not\\s*\\((?'NotTerm'{LiteralStr})\\)|not\\s*(?'NotTerm'{TermStr})";

		// Match a single term or a set of terms separated by operators
		static readonly string RegexStr = $"^((?'Terms'{CombTermStr}){OpStr})*(?'Terms'{CombTermStr})$";

		static readonly Regex TermRegex = new Regex (
						  string.Format (RegexStr, "\\(", "\\)"),
						  RegexOptions.IgnoreCase | RegexOptions.Compiled);

		// Breaking the query the user typed into something useful involves running
		// it through the above regular expression recursively until it is broken down
		// into literals and operators that we can use to generate SQL queries.
		bool ConstructQuery (Term parent, int depth, string txt)
		{
			return ConstructQuery (parent, depth, txt, false);
		}

		bool ConstructQuery (Term parent, int depth, string txt, bool negated)
		{
			if (string.IsNullOrEmpty (txt))
				return true;

			string indent = string.Format ("{0," + depth * 2 + "}", " ");

			//Log.DebugFormat (indent + "Have text: {0}", txt);

			// Match the query the user typed against our regular expression
			Match match = TermRegex.Match (txt);

			if (!match.Success) {
				//Log.Debug (indent + "Failed to match.");
				return false;
			}

			bool op_valid = true;
			string op = string.Empty;

			// For the moment at least we don't support operator precedence, so we require
			// that only a single operator is used for any given term unless it is made unambiguous
			// by using parenthesis.
			foreach (Capture capture in match.Groups["Ops"].Captures) {
				if (string.IsNullOrEmpty (op))
					op = capture.Value;
				else if (op != capture.Value) {
					op_valid = false;
					break;
				}
			}

			if (!op_valid) {
				Log.Information (indent + "Ambiguous operator sequence.  Use parenthesis to explicitly define evaluation order.");
				return false;
			}

			if (match.Groups["Terms"].Captures.Count == 1 && match.Groups["NotTerm"].Captures.Count != 1) {
				//Log.DebugFormat (indent + "Unbreakable term: {0}", match.Groups ["Terms"].Captures [0]);
				string literal;
				bool isNegated = false;
				Tag tag = null;


				if (match.Groups["NotTag"].Captures.Count == 1) {
					literal = match.Groups["NotTag"].Captures[0].Value;
					isNegated = true;
				} else {
					literal = match.Groups["Terms"].Captures[0].Value;
				}

				isNegated = isNegated || negated;

				tag = App.Instance.Database.Tags.GetTagByName (literal);

				// New OR term so we can match against both tag and text search
				parent = new OrTerm (parent, null);

				// If the literal is the name of a tag, include it in the OR
				//AbstractLiteral term = null;
				if (tag != null) {
					new Literal (parent, tag, null);
				}

				// Always include the literal text in the search (path, comment, etc)
				new TextLiteral (parent, literal);

				// If the term was negated, negate the OR parent term
				if (isNegated) {
					parent = parent.Invert (true);
				}

				if (RootTerm == null)
					RootTerm = parent;

				return true;
			} else {
				Term us = null;
				if (!string.IsNullOrEmpty (op)) {
					us = Term.TermFromOperator (op, parent, null);
					if (RootTerm == null)
						RootTerm = us;
				}

				foreach (Capture capture in match.Groups["Term"].Captures) {
					string subterm = capture.Value.Trim ();

					if (string.IsNullOrEmpty (subterm))
						continue;

					// Strip leading/trailing parens
					if (subterm[0] == '(' && subterm[subterm.Length - 1] == ')') {
						subterm = subterm.Remove (subterm.Length - 1, 1);
						subterm = subterm.Remove (0, 1);
					}

					//Log.DebugFormat (indent + "Breaking subterm apart: {0}", subterm);

					if (!ConstructQuery (us, depth + 1, subterm, negated))
						return false;
				}

				foreach (Capture capture in match.Groups["NotTerm"].Captures) {
					string subterm = capture.Value.Trim ();

					if (string.IsNullOrEmpty (subterm))
						continue;

					// Strip leading/trailing parens
					if (subterm[0] == '(' && subterm[subterm.Length - 1] == ')') {
						subterm = subterm.Remove (subterm.Length - 1, 1);
						subterm = subterm.Remove (0, 1);
					}

					//Log.DebugFormat (indent + "Breaking not subterm apart: {0}", subterm);

					if (!ConstructQuery (us, depth + 1, subterm, true))
						return false;
				}

				if (negated && us != null) {
					if (us == RootTerm)
						RootTerm = us.Invert (false);
					else
						us.Invert (false);
				}

				return true;
			}
		}

		bool updating;
		uint update_timeout_id = 0;
		void QueueUpdate ()
		{
			if (updating || update_timeout_id != 0) {
				lock (lockObject) {
					// If there is a timer set and we are not yet handling its timeout, then remove the timer
					// so we delay its trigger for another 500ms.
					if (!updating && update_timeout_id != 0)
						GLib.Source.Remove (update_timeout_id);

					// Assuming we're not currently handling a timeout, add a new timer
					if (!updating)
						update_timeout_id = GLib.Timeout.Add (500, OnUpdateTimer);
				}
			} else {
				// If we are not updating and there isn't a timer already set, then there is
				// no risk of race condition with the  timeout handler.
				update_timeout_id = GLib.Timeout.Add (500, OnUpdateTimer);
			}
		}

		bool OnUpdateTimer ()
		{
			lock (lockObject) {
				updating = true;
			}

			Update ();

			lock (lockObject) {
				updating = false;
				update_timeout_id = 0;
			}

			return false;
		}

		void Update ()
		{
			// Clear the last root term
			RootTerm = null;

			if (ParensValid () && ConstructQuery (null, 0, Entry.Text)) {
				if (RootTerm != null) {
					//Log.DebugFormat("rootTerm = {0}", RootTerm);
					if (!(RootTerm is AndTerm)) {
						// A little hacky, here to make sure the root term is a AndTerm which will
						// ensure we handle the Hidden tag properly
						var root_parent = new AndTerm (null, null);
						RootTerm.Parent = root_parent;
						RootTerm = root_parent;
					}

					//Log.DebugFormat("rootTerm = {0}", RootTerm);
					if (!(RootTerm is AndTerm)) {
						// A little hacky, here to make sure the root term is a AndTerm which will
						// ensure we handle the Hidden tag properly
						var root_parent = new AndTerm (null, null);
						RootTerm.Parent = root_parent;
						RootTerm = root_parent;
					}
					//Log.DebugFormat ("condition = {0}", RootTerm.SqlCondition ());
					query.TagTerm = new ConditionWrapper (RootTerm.SqlCondition ());
				} else {
					query.TagTerm = null;
					//Log.Debug ("root term is null");
				}
			}
		}

		bool ParensValid ()
		{
			for (int i = 0; i < Entry.Text.Length; i++) {
				if (Entry.Text[i] == '(' || Entry.Text[i] == ')') {
					int pair_pos = ParenPairPosition (Entry.Text, i);

					if (pair_pos == -1)
						return false;
				}
			}

			return true;
		}

		/*
		 * Static Utility Methods
		 */
		static int ParenPairPosition (string txt, int pos)
		{
			char one = txt[pos];
			bool open = (one == '(');
			char two = (open) ? ')' : '(';

			//int level = 0;
			int num = (open) ? txt.Length - pos - 1 : pos;

			int sames = 0;
			for (int i = 0; i < num; i++) {
				if (open)
					pos++;
				else
					pos--;

				if (pos < 0 || pos > txt.Length - 1)
					return -1;

				if (txt[pos] == one)
					sames++;
				else if (txt[pos] == two) {
					if (sames == 0)
						return pos;
					sames--;
				}
			}

			return -1;
		}

		/*private static string ReverseString (string txt)
		{
		    char [] txt_a = txt.ToCharArray ();
		    System.Reverse (txt_a);
		    return new String (txt_a);
		}*/
	}
}
