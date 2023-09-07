using osu.Framework.Extensions;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;

namespace osu.XR.Graphics.Settings;

public partial class SettingsFlagEnumCheckbox<T> : SettingsItem<T> where T : struct, Enum, IConvertible {
	protected override Drawable CreateControl () {
		return new ControlDrawable();
	}

	partial class ControlDrawable : FillFlowContainer, IHasCurrentValue<T> {
		BindableWithCurrent<T> current = new();
		public Bindable<T> Current {
			get => current;
			set => current.Current = value;
		}

		List<OsuCheckbox> checkboxes = new();
		public ControlDrawable () {
			var values = Enum.GetValues<T>();
			var flags = values.Where( x => ulong.PopCount( Convert.ToUInt64( x ) ) == 1 ).ToArray();

			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;

			foreach ( var flag in flags ) {
				var control = new OsuCheckbox {
					LabelText = flag.GetLocalisableDescription()
				};

				control.Current.BindValueChanged( v => {
					if ( v.NewValue ) {
						Current.Value = (T)Enum.ToObject( typeof(T), current.Value.ToUInt64(null) | flag.ToUInt64(null));
					}
					else {
						Current.Value = (T)Enum.ToObject( typeof(T), current.Value.ToUInt64( null ) & ~flag.ToUInt64( null ));
					}
				} );

				Add( control );
				checkboxes.Add( control );
			}

			Current.BindValueChanged( v => {
				for ( int i = 0; i < flags.Length; i++ ) {
					checkboxes[i].Current.Value = v.NewValue.HasFlag( flags[i] );
				}
			}, true );
		}
	}
}
