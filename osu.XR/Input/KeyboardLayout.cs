using osu.Framework.Input.Events;
using osuTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using K = osu.XR.Input.KeyboardKey;
using KKey = osuTK.Input.Key;

namespace osu.XR.Input {
	public class KeyboardLayout {
		public List<KeyboardKey> Keys = new();

		public static KeyboardLayout Default => new() {
			Keys = new() {
				new( Key.Escape ), new( Key.F1 ), new( Key.F2 ), new( Key.F3 ), new( Key.F4 ), new( Key.F5 ), new( Key.F6 ), new( Key.F7 ), new( Key.F8 ), new( Key.F9 ), new( Key.F10 ), new( Key.F11 ), new( Key.F12 ), new( Key.PrintScreen ), new( Key.ScrollLock ), new( Key.Pause ),
				K.NoCaps( Key.Tilde, "`", "~" ), new( Key.Number1, "1", "!" ), new( Key.Number2, "2", "@" ), new( Key.Number3, "3", "#" ), new( Key.Number4, "4", "$" ), new( Key.Number5, "5", "%" ), new( Key.Number6, "6", "^" ), new( Key.Number7, "7", "&" ), new( Key.Number8, "8", "*" ), new( Key.Number9, "9", "(" ), new( Key.Number0, "0", ")" ), new( Key.Minus, "-", "_" ), new( Key.Plus, "+", "=" ), new( Key.BackSpace ), new( Key.Insert ), new( Key.Home ), new( Key.PageUp ), K.Toggle( Key.NumLock ), new( Key.KeypadDivide, "/" ), new( Key.KeypadMultiply, "*" ), new( Key.KeypadSubtract, "-" ),
				new( Key.Tab, "\t" ), new( Key.Q, "q", "Q" ), new( Key.W, "w", "W" ), new( Key.E, "e", "E" ), new( Key.R, "r", "R" ), new( Key.T, "t", "T" ), new( Key.Y, "y", "Y" ), new( Key.U, "u", "U" ), new( Key.I, "i", "I" ), new( Key.O, "o", "O" ), new( Key.P, "p", "P" ), new( Key.BracketLeft, "[", "{" ), new( Key.BracketRight, "]", "}" ), new( Key.Enter, "\n" ), new( Key.Delete ), new( Key.End ), new( Key.PageDown ), new( Key.Keypad7, "7" ), new( Key.Keypad8, "8" ), new( Key.Keypad9, "9" ), new( Key.KeypadAdd, "+" ),
				K.Toggle( Key.CapsLock ), new( Key.A, "a", "A" ), new( Key.S, "s", "S" ), new( Key.D, "d", "D" ), new( Key.F, "f", "F" ), new( Key.G, "g", "G" ), new( Key.H, "h", "H" ), new( Key.J, "j", "J" ), new( Key.K, "k", "K" ), new( Key.L, "l", "L" ), new( Key.Semicolon, ";", ":" ), new( Key.Quote, "'", "\"" ), new( Key.BackSlash, "\\", "|" ), new( Key.Keypad4, "4" ), new( Key.Keypad5, "5" ), new( Key.Keypad6, "6" ),
				K.Modifier( Key.ShiftLeft ), new( Key.Z, "z", "Z" ), new( Key.X, "x", "X" ), new( Key.C, "c", "C" ), new( Key.V, "v", "V" ), new( Key.B, "b", "B" ), new( Key.N, "n", "N" ), new( Key.M, "m", "M" ), new( Key.Comma, ",", "<" ), new( Key.Period, ".", ">" ), new( Key.Slash, "/", "?" ), K.Modifier( Key.ShiftRight ), new( Key.Up ), new( Key.Keypad1, "1" ), new( Key.Keypad2, "2" ), new( Key.Keypad3, "3" ), new( Key.KeypadEnter, "\n" ),
				K.Modifier( Key.ControlLeft ), new( Key.WinLeft ), K.Modifier( Key.AltLeft ), new( Key.Space, " " ), K.Modifier( Key.AltRight ), new( Key.WinRight ), new( Key.Menu ), K.Modifier( Key.ControlRight ), new( Key.Left ), new( Key.Down ), new( Key.Right ), new( Key.Keypad0, "0" ), new( Key.KeypadPeriod, "." )
			}
		};
	}

	public class KeyboardKey {
		public Key? Key;
		public bool IsModifier;
		public bool CanAutoRepeat;
		public bool IsToggle;
		public (Key[] modifiers, string value)[] ModifierCombos;

		public static KeyboardKey Modifier ( Key key ) {
			return new KeyboardKey() {
				Key = key,
				IsModifier = true,
				IsToggle = false
			};
		}

		public KeyboardKey () { }
		public KeyboardKey ( Key key ) {
			Key = key;
			CanAutoRepeat = true;
		}

		public KeyboardKey ( string lowerCase, string upperCase ) {
			CanAutoRepeat = true;
			ModifierCombos = new[] {
				(new Key[]{ }, lowerCase),
				(new Key[]{ osuTK.Input.Key.ShiftLeft }, upperCase),
				(new Key[]{ osuTK.Input.Key.ShiftRight }, upperCase),
				(new Key[]{ osuTK.Input.Key.CapsLock }, upperCase)
			};
		}

		public KeyboardKey ( string lowerCase ) {
			CanAutoRepeat = true;
			ModifierCombos = new[] {
				(new Key[]{ }, lowerCase)
			};
		}

		public KeyboardKey ( Key key, string lowerCase ) {
			Key = key;
			CanAutoRepeat = true;
			ModifierCombos = new[] {
				(new Key[]{ }, lowerCase)
			};
		}

		public KeyboardKey ( Key key, string lowerCase, string upperCase ) {
			Key = key;
			CanAutoRepeat = true;
			ModifierCombos = new[] {
				(new Key[]{ }, lowerCase),
				(new Key[]{ osuTK.Input.Key.ShiftLeft }, upperCase),
				(new Key[]{ osuTK.Input.Key.ShiftRight }, upperCase),
				(new Key[]{ osuTK.Input.Key.CapsLock }, upperCase)
			};
		}

		public static KeyboardKey NoCaps ( Key key, string lowerCase, string upperCase ) {
			return new() {
				Key = key,
				CanAutoRepeat = true,
				ModifierCombos = new[] {
					(new Key[]{ }, lowerCase),
					(new Key[]{ osuTK.Input.Key.ShiftLeft }, upperCase),
					(new Key[]{ osuTK.Input.Key.ShiftRight }, upperCase)
				}
			};
		}

		public static KeyboardKey Toggle ( Key key ) {
			return new() {
				Key = key,
				IsModifier = true,
				IsToggle = true
			};
		}

		public string GetComboFor ( params Key[] modifiers ) {
			if ( ModifierCombos is not null ) {
				if ( modifiers is null ) modifiers = Array.Empty<Key>();
				var matches = this.ModifierCombos.Where( x => x.modifiers.Count() == modifiers.Length && !x.modifiers.Except( modifiers ).Any() );
				if ( matches.Any() ) {
					return matches.First().value;
				}

				matches = this.ModifierCombos.Where( x => x.modifiers.Count() < modifiers.Length && !x.modifiers.Except( modifiers ).Any() );
				if ( matches.Any() ) {
					return matches.OrderByDescending( x => x.modifiers.Count() ).First().value;
				}

				if ( ModifierCombos.Any( x => x.modifiers.Length == 0 ) ) 
					return ModifierCombos.First( x => x.modifiers.Length == 0 ).value;
			}

			return "";
		}

		public string GetDisplayFor ( params Key[] modifiers ) {
			static string translate (string str) {
				return str switch {
					"\t" => "Tab",
					"\n" => "Enter",
					" " => "␣", // Space
					_ => str
				};
			}

			if ( ModifierCombos is not null ) {
				if ( modifiers is null ) modifiers = Array.Empty<Key>();
				var matches = this.ModifierCombos.Where( x => x.modifiers.Count() == modifiers.Length && !x.modifiers.Except( modifiers ).Any() );
				if ( matches.Any() ) {
					return translate( matches.First().value );
				}

				matches = this.ModifierCombos.Where( x => x.modifiers.Count() < modifiers.Length && !x.modifiers.Except( modifiers ).Any() );
				if ( matches.Any() ) {
					return translate( matches.OrderByDescending( x => x.modifiers.Count() ).First().value );
				}

				if ( ModifierCombos.Any( x => x.modifiers.Length == 0 ) ) {
					return translate( ModifierCombos.First( x => x.modifiers.Length == 0 ).value );
				}
			}

			return Key switch {
				KKey.Escape => "Esc",
				KKey.AltLeft or KKey.AltRight => "Alt",
				KKey.ControlLeft or KKey.ControlRight => "Ctrl",
				KKey.WinLeft or KKey.WinRight => "Host",
				KKey.ShiftLeft or KKey.ShiftRight => "Shift",
				KKey.PrintScreen => "Print\nScrn",
				KKey.PageDown => "Page\nDown",
				KKey.PageUp => "Page\nUp",
				KKey.Insert => "Ins",
				KKey.ScrollLock => "Scroll\nLock",
				KKey.Pause => "Pause\nBreak",
				KKey.Delete => "Del",
				KKey.BackSpace => "BackSpc",
				KKey.NumLock => "Num\nLock",

				_ => Key?.ToString() ?? "NULL"
			};
		}
	}
}
