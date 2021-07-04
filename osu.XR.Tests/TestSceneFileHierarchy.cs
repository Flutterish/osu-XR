using osu.Framework.Graphics;
using osu.Game.Tests.Visual;
using osu.XR.Drawables;
using osu.XR.Drawables.Containers;
using osuTK;

namespace osu.XR.Tests {
	public class TestSceneFileHierarchy : OsuTestScene {
		FileHierarchyPanel hierarchy;

		protected override void LoadComplete () {
			base.LoadComplete();

			Add( hierarchy = new FileHierarchyPanel() {
				Size = new Vector2( 400, 500 ),
				Anchor = Anchor.Centre,
				Origin = Anchor.Centre
			} );

			AddToggleStep( "IsMultiselect", v => hierarchy.preview.IsMultiselect = v );
			AddToggleStep( "SelectionNavigates", v => hierarchy.preview.SelectionNavigates = v );
		}
	}

	public class FileHierarchyPanel : ConfigurationContainer {
		public FileHierarchyViewWithPreview preview;
		public FileHierarchyPanel () {
			Title = "Files";
			Description = "select some files";

			AddSection( preview = new FileHierarchyViewWithPreview { IsMultiselect = true, SelectionNavigates = false }, name: "Files" );
			preview.SearchTermsModified += () => {
				Content.PerformFilter();
			};
		}
	}
}
