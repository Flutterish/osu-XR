using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osu.Game.Overlays.Settings;
using osu.XR.Drawables.Containers;
using osu.XR.Inspector;
using osuTK;
using System.Collections.Generic;

namespace osu.XR.Tests {
	public class TestSceneInspector : OsuTestScene3D {
		InspectorDrawable inspector;
		TestComponent component;

		protected override void LoadComplete () {
			base.LoadComplete();
			Add( inspector = new InspectorDrawable {
				Size = new Vector2( 400, 500 ),
				Anchor = Anchor.Centre,
				Origin = Anchor.Centre
			} );

			Scene.Add( component = new TestComponent() );
			AddStep( "Inspect element", () => inspector.InspectedElementBindable.Value = component );
		}
	}

	class TestComponent : Container3D, IConfigurableInspectable, IHasInspectorVisuals, IChildrenNotInspectable, IExperimental {
		public TestComponent () {
			Add( new Model { Mesh = Mesh.UnitCube } );
			X = -2;
		}

		public Drawable CreateInspectorSubsection () {
			return new NamedContainer {
				DisplayName = "Sample settings",
				Children = new Drawable[] {
					new SettingsSlider<double> {
						Current = new BindableDouble { MinValue = 0, MaxValue = 10 },
						LabelText = "Sample setting"
					}
				}
			};
		}

		public bool AreSettingsPersistent => true;
	}
}
