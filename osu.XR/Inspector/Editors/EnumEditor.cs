using osu.Framework.Bindables;
using osu.Game.Overlays.Settings;
using System;
using System.Collections.Generic;

namespace osu.XR.Inspector.Editors {
	public class EnumEditor<T> : ValueEditor<T> where T : struct, Enum {
		public EnumEditor ( T defaultValue ) : base( defaultValue ) {
			if ( typeof( T ).IsDefined( typeof( FlagsAttribute ), false ) ) {
				makeFlags();
			}
			else {
				makeChoice();
			}
		}

		void makeFlags () {
			List<(T flag, Bindable<bool> isActive)> flags = new();
			foreach ( T i in Enum.GetValues<T>() ) {
				dynamic value = i;
				if ( value != 0 && ( value & value - 1 ) == 0 ) {
					SettingsCheckbox cb;
					Add( cb = new SettingsCheckbox {
						LabelText = i.ToString(),
						Current = new BindableBool( Current.Value.HasFlag( i ) )
					} );
					flags.Add( (i, cb.Current) );
					cb.Current.ValueChanged += _ => flagChanged( flags );
				}
				else {
					// TODO multiflags
				}
			}
		}

		private void flagChanged ( List<(T flag, Bindable<bool> isActive)> flags ) {
			dynamic value = default( T );
			foreach ( var (flag, isActive) in flags ) {
				if ( isActive.Value )
					value |= flag;
			}
			Current.Value = value;
		}

		void makeChoice () {
			Add( new SettingsEnumDropdown<T> {
				Current = Current
			} );
		}
	}
}
