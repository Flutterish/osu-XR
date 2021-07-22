using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.XR.Components;
using osu.Game.Rulesets;
using osu.XR.Components;
using osu.XR.Drawables.Containers;
using osu.XR.Editor;
using osu.XR.Inspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Panels.Drawables {
	public class SceneManagerDrawable : ConfigurationContainer {
		public readonly Bindable<SceneContainer> SceneContainerBindable = new();
		public SceneContainer SceneContainer {
			get => SceneContainerBindable.Value;
			set => SceneContainerBindable.Value = value;
		}
		public SceneManagerDrawable () { // TODO save the scenery
			Title = "Scene Manager";
			Description = "change up the scenery";

			SceneContainerBindable.BindValueChanged( v => {
				SceneContainer.Clear();

				var skybox = new SkyBox();
				var floorgrid = new FloorGrid();
				var dust = new DustEmitter();

				SceneContainer.Add( skybox );
				SceneContainer.Add( floorgrid );
				SceneContainer.Add( dust );

				addSubsections( skybox );
				addSubsections( floorgrid );
				addSubsections( dust );
			} );
		}

		// TODO later on scenery components will be able to selectively appear besed on ruleset

		void addSubsections ( IConfigurableInspectable configurable ) {
			foreach ( var i in configurable.CreateInspectorSubsectionsWithWarning() ) {
				AddSection( i );
			}
		}
	}
}
