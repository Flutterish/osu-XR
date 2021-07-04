using osu.Framework.Graphics;
using osu.XR.Drawables.Containers;
using osu.XR.Inspector;
using osuTK;

namespace osu.XR.Tests {
	public class TestSceneHierarchyInspectorV2 : OsuTestScene3D {
		HierarchyInspectorPanel inspector;
		TestComponent component;

		protected override void LoadComplete () {
			base.LoadComplete();

			Scene.Add( component = new TestComponent() );
			Add( inspector = new HierarchyInspectorPanel( component ) {
				Size = new Vector2( 400, 500 ),
				Anchor = Anchor.Centre,
				Origin = Anchor.Centre
			} );

			AddToggleStep( "IsMultiselect", v => inspector.preview.IsMultiselect = v );
			AddToggleStep( "SelectionNavigates", v => inspector.preview.SelectionNavigates = v );
		}
	}

	public class HierarchyInspectorPanel : ConfigurationContainer {
		public HierarchyInspector preview;
		public HierarchyInspectorPanel ( Drawable drawable ) {
			Title = "Hierarchy";
			Description = "browse drawables";

			AddSection( preview = new HierarchyInspector( drawable ), name: "Hierarchy" );
			preview.SearchTermsModified += () => {
				Content.PerformFilter();
			};
		}
	}
}
