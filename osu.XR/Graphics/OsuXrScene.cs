using OpenVR.NET;
using OpenVR.NET.Devices;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Input;
using osu.Framework.XR.VirtualReality;
using osu.XR.Configuration;
using FrameworkMaterialNames = osu.Framework.XR.Graphics.Materials.MaterialNames;
using MaterialNames = osu.XR.Graphics.Materials.MaterialNames;

namespace osu.XR.Graphics;

public partial class OsuXrScene : Scene {
	[Resolved( canBeNull: true )]
	VrCompositor compositor { get; set; } = null!;

	[Resolved( canBeNull: true )]
	BasicSceneMovementSystem movementSystem { get; set; } = null!;

	Bindable<CameraMode> cameraMode = new( CameraMode.ThirdPerson );
	[BackgroundDependencyLoader(permitNulls: true)]
	private void load ( OsuXrConfigManager? config ) {
		if ( config == null || movementSystem == null || compositor == null )
			return;

		config.BindWith( OsuXrSetting.CameraMode, cameraMode );

		if ( RenderToScreen ) { // HACK its only enabled when we use simulated vr
			cameraMode.Value = CameraMode.ThirdPerson;
		}

		cameraMode.BindValueChanged( v => {
			switch ( v.NewValue ) {
				case CameraMode.Disabled:
					RenderToScreen = false;
					break;

				case CameraMode.FirstPersonVR:
					break;

				case CameraMode.FirstPerson:
					RenderToScreen = true;
					movementSystem.ControlType = ControlType.Fly;
					// TODO panel interaction toggle - Im thinking some F-key shortcut would toggle an on screen panel with settings
					break;

				case CameraMode.ThirdPerson:
					RenderToScreen = true;
					movementSystem.ControlType = ControlType.Orbit;
					break;
			}
		}, true );
	}

	protected override void Update () {
		if ( cameraMode.Value == CameraMode.FirstPersonVR ) {
			if ( compositor.VR is VR vr && vr.Headset is Headset headset ) {
				RenderToScreen = true;
				Camera.Position = headset.Position.ToOsuTk(); // TODO use render position
				Camera.Rotation = headset.Rotation.ToOsuTk();
			}
			else {
				RenderToScreen = false;
			}
		}

		base.Update();
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