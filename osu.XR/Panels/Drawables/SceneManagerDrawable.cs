﻿using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.XR.Components;
using osu.XR.Components.Skyboxes;
using osu.XR.Drawables.Containers;
using osu.XR.Editor;
using osu.XR.Inspector;

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
				//SceneContainer.Add( new BeatingScenery.GripableCollider { Mesh = Mesh.UnitCube, Scale = new osuTK.Vector3( 0.3f ), Y = 1 } );

				addSubsections( skybox );
				addSubsections( floorgrid );
				addSubsections( dust );
			} );
		}

		// TODO later on scenery components will be able to selectively appear besed on ruleset

		void addSubsections ( IConfigurableInspectable configurable ) {
			if ( configurable.CreateWarnings() is Drawable warning ) {
				AddSection( warning );
			}
			AddSection( configurable.CreateInspectorSubsection() );
		}
	}
}
