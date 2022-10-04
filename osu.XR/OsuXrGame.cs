using OpenVR.NET;
using OpenVR.NET.Devices;
using osu.Framework.XR.Input;
using osu.Framework.XR.Physics;
using osu.Framework.XR.VirtualReality;
using osu.XR.Graphics;
using osu.XR.Graphics.Panels;
using osu.XR.Graphics.Scenes;
using osu.XR.Graphics.VirtualReality;
using osu.XR.VirtualReality;

namespace osu.XR;

public class OsuXrGame : OsuXrGameBase {
	[Cached]
	VR vr = new();

	[Cached]
	VrCompositor compositor = new();

	[Cached]
	VrResourceStore vrResourceStore = new();

	[Cached]
	PhysicsSystem physics = new();

	BasicSceneMovementSystem movementSystem;

	[Cached]
	PanelInteractionSystem panelInteraction = new();

	OsuXrScene scene;
	OsuGamePanel osuPanel;

	public OsuXrGame () {
		Add( compositor );
		scene = new() {
			RelativeSizeAxes = Axes.Both
		};
		scene.Camera.Z = -5;
		scene.Camera.Y = 1;
		scene.Add( osuPanel = new() {
			ContentSize = new( 1920, 1080 ),
			Y = 1.8f
		} );

		physics.AddSubtree( scene.Root );
		Add( movementSystem = new( scene ) { RelativeSizeAxes = Axes.Both } );
		Add( new BasicPanelInteractionSource( scene, physics, panelInteraction ) { RelativeSizeAxes = Axes.Both } );

		//scene.Add( new HandheldMenu() { Y = 1 } );
		scene.Add( new VrPlayer() );

		compositor.Initialized += vr => {
			vr.SetActionManifest( new OsuXrActionManifest() );

			foreach ( var i in vr.TrackedDevices )
				addVrDevice( i );
			vr.DeviceDetected += addVrDevice;
		};
	}

	[Cached(typeof(IBindableList<VrController>))]
	BindableList<VrController> vrControllers = new();

	void addVrDevice ( VrDevice device ) {
		if ( device is Headset )
			return;

		if ( device is Controller controller ) {
			VrController vrController;
			scene.Add( vrController = new VrController( controller, scene ) );
			vrControllers.Add( vrController );
			return;
		}

		scene.Add( new BasicVrDevice( device ) );
	}

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		var deps = new DependencyContainer( parent );

		deps.CacheAs( osuPanel.OsuDependencies );

		return base.CreateChildDependencies( deps );
	}

	protected override void LoadComplete () {
		base.LoadComplete();
		Add( scene );
		scene.Add( new GridScene() );
	}

	protected override void Dispose ( bool isDisposing ) {
		if ( !IsDisposed ) {
			Task.Run( async () => {
				await Task.Delay( 1000 );
				vr.Exit();
				vrResourceStore.Dispose();
			} );
		}

		base.Dispose( isDisposing );
	}
}
