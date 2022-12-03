using osu.Framework.Graphics.Rendering;
using osu.Framework.IO.Stores;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Rendering;
using MaterialNames = osu.XR.Graphics.Materials.MaterialNames;

namespace osu.XR.Graphics;

public partial class OsuXrScene : Scene {
	protected override ResourceStore<byte[]>? CreateMaterialStoreSource ( IReadOnlyDependencyContainer deps )
		=> new NamespacedResourceStore<byte[]>( new DllResourceStore( typeof( OsuXrGame ).Assembly ), "Resources/Shaders" );

	protected override ResourceStore<byte[]>? CreateMeshStoreSource ( IReadOnlyDependencyContainer deps )
		=> new NamespacedResourceStore<byte[]>( new DllResourceStore( typeof( OsuXrGame ).Assembly ), "Resources/Models" );

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		var r = base.CreateChildDependencies( parent );

		MaterialStore.AddDescriptor( MaterialNames.PanelTransparent, new MaterialDescriptor( MaterialStore.GetDescriptor( "unlit_panel" ) )
			.AddOnBind( (m, s) => {
				m.Shader.SetUniform( "solidPass", s.GetGlobalProperty<bool>( "solidPass" ) );
			} )	
		);

		return r;
	}

	protected override RenderPiepline CreateRenderPipeline () {
		return new Pipeline( this );
	}

	class Pipeline : BasicRenderPiepline {
		public Pipeline ( OsuXrScene source ) : base( source ) { }

		protected override void Draw ( IRenderer renderer, int subtreeIndex, Matrix4 projectionMatrix ) {
			if ( TryGetRenderStage( RenderingStage.Skybox, out var drawables ) ) {
				renderer.PushDepthInfo( new( depthTest: false, writeDepth: false ) );
				foreach ( var i in drawables ) {
					i.GetDrawNodeAtSubtree( subtreeIndex )?.Draw( renderer );
				}
				renderer.PopDepthInfo();
			}

			foreach ( var stage in RenderStages ) {
				if ( stage is RenderingStage )
					continue;

				foreach ( var i in GetRenderStage( stage ) ) {
					i.GetDrawNodeAtSubtree( subtreeIndex )?.Draw( renderer );
				}
			}

			if ( TryGetRenderStage( RenderingStage.Transparent, out drawables ) ) {
				MaterialStore.SetGlobalProperty( "solidPass", true );
				foreach ( var i in drawables ) {
					i.GetDrawNodeAtSubtree( subtreeIndex )?.Draw( renderer );
				}
				Material.Unbind(); // reset property

				MaterialStore.SetGlobalProperty( "solidPass", false );
				renderer.PushDepthInfo( new( depthTest: true, writeDepth: false ) );
				foreach ( var i in drawables ) {
					i.GetDrawNodeAtSubtree( subtreeIndex )?.Draw( renderer );
				}
				renderer.PopDepthInfo();
			}
		}
	}
}

public enum RenderingStage {
	Skybox,
	Transparent
}