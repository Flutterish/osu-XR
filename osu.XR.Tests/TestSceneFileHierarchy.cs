using osu.Framework.Graphics;
using osu.Game.Tests.Visual;
using osu.XR.Drawables;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Tests {
	public class TestSceneFileHierarchy : OsuTestScene {
		protected override void LoadComplete () {
			base.LoadComplete();

			Add( new FileHierarchyPanel() {
				Size = new Vector2( 400, 500 ),
				Anchor = Anchor.Centre,
				Origin = Anchor.Centre
			} );
		}
	}

	public class FileHierarchyPanel : ConfigurationContainer {
		public FileHierarchyPanel () {
			Title = "Files";
			Description = "select some files";

			AddSection( new FileHierarchyView(), name: "Files" );
		}
	}
}
