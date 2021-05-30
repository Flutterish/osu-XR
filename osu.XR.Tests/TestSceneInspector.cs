using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.IO.Stores;
using osu.Framework.Utils;
using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osu.Game;
using osu.Game.Tests.Visual;
using osu.XR.Components.Panels;
using osu.XR.Inspector;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Tests {
	public class TestSceneInspector : OsuTestScene3D {
		InspectorPanel inspector;
		TestComponent component;

		protected override void LoadComplete () {
			base.LoadComplete();
			Add( inspector = new InspectorPanel {
				Size = new Vector2( 400, 500 ),
				Anchor = Anchor.Centre,
				Origin = Anchor.Centre
			} );

			Scene.Add( component = new TestComponent() );
			AddStep( "Inspect element", () => inspector.InspectedElementBindable.Value = component );
		}
	}

	class TestComponent : Container3D {
		public TestComponent () {
			Add( new Model { Mesh = Mesh.UnitCube } );
		}
	}
}
