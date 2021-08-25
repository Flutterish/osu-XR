using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Components;
using osu.Game.Overlays.Settings;
using osu.XR.Inspector;
using osu.XR.Settings.Sections;

#nullable enable

namespace osu.XR.Editor {
	public class PropContainer : CompositeDrawable3D, IConfigurableInspectable {
		public readonly Drawable3D Prop;
		new public string Name {
			get => NameBindable.Value;
			set => NameBindable.Value = value;
		}
		public readonly Bindable<string> NameBindable;

		public PropContainer ( Drawable3D prop, string name = "New Prop" ) {
			Prop = prop;
			InternalChild = prop;
			AutoSizeAxes = Axes3D.All;
			NameBindable = new Bindable<string>( name );
		}

		public Drawable CreateInspectorSubsection () {
			return new SettingsSectionContainer {
				Title = "Imported Prop",
				Icon = FontAwesome.Solid.Cube,

				Children = new Drawable[] {
					new SettingsTextBox {
						LabelText = "Name",
						Current = NameBindable,
						Margin = new MarginPadding { Bottom = 5 }
					},
					new DangerousSettingsButton {
						Text = "Delete",
						Action = () => {
							Destroy();
						}
					}
				}
			};
		}

		public bool AreSettingsPersistent => false;
	}
}
