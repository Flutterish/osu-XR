using OpenVR.NET;
using OpenVR.NET.Devices;
using osu.Framework.XR.Components;
using osu.Framework.XR.Physics;
using osu.Framework.XR.VirtualReality;
using osu.XR.Graphics;
using osu.XR.Graphics.Panels;
using osu.XR.Graphics.Panels.Menu;
using osu.XR.Graphics.Scenes;

namespace osu.XR;

public class OsuXrGame : OsuXrGameBase {
	[Cached]
	VR vr = new();

	[Cached]
	VrCompositor compositor = new();

	[Cached]
	VrResourceStore vrResourceStore = new();

	OsuXrScene scene;
	OsuGamePanel osuPanel;
	PhysicsSystem physics = new();
	SceneMovementSystem movementSystem;
	PanelInteractionSystem panelInteraction;

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
		Add( panelInteraction = new( scene, physics ) { RelativeSizeAxes = Axes.Both } );

		scene.Add( new HandheldMenu() { Y = 1 } );
		scene.Add( new VrPlayer() );

		compositor.Initialized += vr => {
			foreach ( var i in vr.TrackedDevices )
				addVrDevice( i );
			vr.DeviceDetected += addVrDevice;
		};
	}

	void addVrDevice ( VrDevice device ) {
		if ( device is Headset )
			return;

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
