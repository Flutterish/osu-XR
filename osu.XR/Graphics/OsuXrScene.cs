using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Parsing.Wavefront;
using FrameworkMaterialNames = osu.Framework.XR.Graphics.Materials.MaterialNames;
using MaterialNames = osu.XR.Graphics.Materials.MaterialNames;

namespace osu.XR.Graphics;

public partial class OsuXrScene : Scene {
	protected override MaterialStore CreateMaterialStore ( IReadOnlyDependencyContainer dependencies ) {
		return new MaterialStore( new ResourceStore<byte[]>( new[] { // TODO why does game not have our namespace?
			new NamespacedResourceStore<byte[]>( new DllResourceStore( typeof(OsuXrGameBase).Assembly ), "Resources/Shaders" ),
			new NamespacedResourceStore<byte[]>( new DllResourceStore( typeof(Scene).Assembly ), "Resources/Shaders" )
		} ) );
	}
	protected override void CreateMeshStoreSources ( MeshStore meshes, IReadOnlyDependencyContainer dependencies ) {
		var resources = new NamespacedResourceStore<byte[]>( new DllResourceStore( typeof( OsuXrGameBase ).Assembly ), "Resources/Meshes" );
		meshes.AddStore( new SingleObjMeshStore( resources ) );
		meshes.AddStore( new ObjMeshCollectionStore( resources ) );
	}
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