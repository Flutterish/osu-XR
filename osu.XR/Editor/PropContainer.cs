using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Components;
using osu.Framework.XR.Physics;
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
			var section = new SettingsSectionContainer {
				Title = "Imported Prop",
				Icon = FontAwesome.Solid.Cube,

				Children = new Drawable[] {
					new SettingsTextBox {
						LabelText = "Name",
						Current = NameBindable,
						Margin = new MarginPadding { Bottom = 5 }
					}
				}
			};

			if ( Prop is Collider co ) {
				Bindable<bool> colliderEnabled = new( false );
				section.Add( new SettingsCheckbox {
					LabelText = "Enable blocking touch pointers",
					TooltipText = "Touch pointers can easily get stuck inside props while gripping them.\nThis setting should be disable while doing so.",
					Current = colliderEnabled
				} );

				colliderEnabled.BindValueChanged( v => {
					co.PhysicsLayer = v.NewValue ? GamePhysicsLayer.All : GamePhysicsLayer.Prop;
				}, true );
			}

			if ( Prop is IGripable gr ) {
				section.AddRange( new Drawable[] {
					new SettingsCheckbox {
						LabelText = "Allow Gripping",
						Current = gr.CanBeGripped
					},
					new SettingsCheckbox {
						LabelText = "Allow Grip Movement",
						Current = gr.AllowsGripMovement
					},
					new SettingsCheckbox {
						LabelText = "Allow Grip Rotation",
						Current = gr.AllowsGripRotation
					},
					new SettingsCheckbox {
						LabelText = "Allow Grip Scaling",
						Current = gr.AllowsGripScaling
					},
				} );
			}

			section.Add( new DangerousSettingsButton {
				Text = "Delete",
				Action = () => {
					Destroy();
				},
				Margin = new MarginPadding { Top = 6 }
			} );

			return section;
		}

		public bool AreSettingsPersistent => false;
	}
}
