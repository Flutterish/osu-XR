using osu.Framework.Graphics.Rendering;
using osu.Framework.IO.Stores;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Rendering;
using MaterialNames = osu.XR.Graphics.Materials.MaterialNames;
using FrameworkMaterialNames = osu.Framework.XR.Graphics.Materials.MaterialNames;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;

namespace osu.XR.Graphics;

public partial class OsuXrScene : Scene {
	protected override ResourceStore<byte[]>? CreateMaterialStoreSource ( IReadOnlyDependencyContainer deps )
		=> new NamespacedResourceStore<byte[]>( new DllResourceStore( typeof( OsuXrGame ).Assembly ), "Resources/Shaders" );

	protected override ResourceStore<byte[]>? CreateMeshStoreSource ( IReadOnlyDependencyContainer deps )
		=> new NamespacedResourceStore<byte[]>( new DllResourceStore( typeof( OsuXrGame ).Assembly ), "Resources/Models" );

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		var deps = new DependencyContainer( parent );
		var r = base.CreateChildDependencies( deps );

		var textures = new TextureStore( parent.Get<IRenderer>() );
		textures.AddTextureSource( parent.Get<GameHost>().CreateTextureLoaderStore( new NamespacedResourceStore<byte[]>( new DllResourceStore( typeof(OsuXrGame).Assembly ), "Resources/Textures" ) ) );
		deps.CacheAs( textures );

		MaterialStore.AddDescriptor( MaterialNames.PanelTransparent, new MaterialDescriptor( MaterialStore.GetDescriptor( FrameworkMaterialNames.UnlitPanel ) )
			.AddOnBind( (m, s) => {
				m.Shader.SetUniform( "solidPass", s.GetGlobalProperty<bool>( "solidPass" ) );
			} )	
		);
		MaterialStore.AddDescriptor( MaterialNames.Transparent, new MaterialDescriptor( MaterialStore.GetDescriptor( FrameworkMaterialNames.Unlit ) )
			.AddOnBind( ( m, s ) => {
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
				foreach ( var i in drawables ) { // TODO sort them
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