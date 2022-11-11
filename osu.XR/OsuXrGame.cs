using OpenVR.NET;
using osu.Framework.Graphics.Shapes;
using osu.Framework.XR.Input;
using osu.Framework.XR.Physics;
using osu.Framework.XR.Testing.VirtualReality;
using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;
using osu.XR.Configuration;
using osu.XR.Graphics;
using osu.XR.Graphics.Panels;
using osu.XR.Graphics.Panels.Menu;
using osu.XR.Graphics.Scenes;
using osu.XR.Graphics.VirtualReality;
using osu.XR.VirtualReality;

namespace osu.XR;

[Cached]
public class OsuXrGame : OsuXrGameBase {
	[Cached]
	VR vr = new();

	[Cached(typeof(VrCompositor))]
	public readonly VrCompositor Compositor;

	[Cached]
	VrResourceStore vrResourceStore = new();

	[Cached]
	PhysicsSystem physics = new();

	BasicSceneMovementSystem movementSystem;

	[Cached]
	PanelInteractionSystem panelInteraction = new();

	OsuXrScene scene;
	OsuGamePanel osuPanel;

	public OsuXrGame ( bool useSimulatedVR = false ) {
		scene = new() {
			RelativeSizeAxes = Axes.Both
		};

		Compositor = useSimulatedVR ? setupVrRig() : new VrCompositor();

		Add( Compositor );
		
		scene.Camera.Z = -5;
		scene.Camera.Y = 1;
		scene.Add( osuPanel = new() );

		physics.AddSubtree( scene.Root );
		Add( movementSystem = new( scene ) { RelativeSizeAxes = Axes.Both } );
		Add( new BasicPanelInteractionSource( scene, physics, panelInteraction ) { RelativeSizeAxes = Axes.Both } );

		scene.Add( new HandheldMenu() { Y = 1 } );
		scene.Add( new VrPlayer() );

		Compositor.Input.SetActionManifest( new OsuXrActionManifest() );
		Compositor.BindDeviceDetected( addVrDevice );

		Compositor.Input.DominantHandBindable.BindValueChanged( v => {
			if ( dominantHandSetting.Value is HandSetting.Auto )
				DominantHand.Value = v.NewValue;
		} );

		dominantHandSetting.BindValueChanged( v => {
			if ( v.NewValue is HandSetting.Auto ) {
				DominantHand.Value = Compositor.Input.DominantHandBindable.Value;
			}
			else {
				DominantHand.Value = v.NewValue == HandSetting.Right ? Hand.Right : Hand.Left;
			}
		} );
	}

	VrCompositor setupVrRig () {
		var comp = new TestingVrCompositor();
		TestingRig rig = new( scene ) { Depth = -1 };
		Add( rig );

		var left = new TestingController( comp, Valve.VR.ETrackedControllerRole.LeftHand );
		left.IsEnabled.Value = true;
		var right = new TestingController( comp, Valve.VR.ETrackedControllerRole.RightHand );
		right.IsEnabled.Value = true;
		var head = new TestingHeadset( comp );

		left.PositionBindable.BindTo( rig.LeftTarget.PositionBindable );
		left.RotationBindable.BindTo( rig.LeftTarget.RotationBindable );
		right.PositionBindable.BindTo( rig.RightTarget.PositionBindable );
		right.RotationBindable.BindTo( rig.RightTarget.RotationBindable );
		head.PositionBindable.BindTo( rig.Head.PositionBindable );
		head.RotationBindable.BindTo( rig.Head.RotationBindable );

		comp.Input.LeftHandPosition.BindTo( rig.LeftTarget.PositionBindable );
		comp.Input.LeftHandRotation.BindTo( rig.LeftTarget.RotationBindable );
		comp.Input.RightHandPosition.BindTo( rig.RightTarget.PositionBindable );
		comp.Input.RightHandRotation.BindTo( rig.RightTarget.RotationBindable );

		Schedule( () => {
			comp.AddDevice( left );
			comp.AddDevice( right );
			comp.AddDevice( head );
		} );

		var controls = comp.Input.CreateControlsDrawable();
		controls.AutoSizeAxes = Axes.Y;
		controls.RelativeSizeAxes = Axes.X;
		Add( new Container {
			Depth = -1,
			RelativeSizeAxes = Axes.Both,
			Size = new( 0.4f, 0.5f ),
			Origin = Anchor.BottomRight,
			Anchor = Anchor.BottomRight,
			Children = new Drawable[] {
				new Box { Colour = FrameworkColour.GreenDark, RelativeSizeAxes = Axes.Both },
				new BasicScrollContainer {
					RelativeSizeAxes = Axes.Both,
					Padding = new MarginPadding( 16 ),
					ScrollbarVisible = false,
					Child = controls
				}
			}
		} );

		return comp;
	}

	public readonly BindableList<VrController> VrControllers = new();
	public readonly BindableList<VrController> ActiveVrControllers = new();

	public readonly Bindable<Hand> DominantHand = new( Hand.Right );

	void addVrDevice ( VrDevice device ) {
		if ( device is Headset )
			return;

		if ( device is Controller controller ) {
			VrController vrController;
			scene.Add( vrController = new VrController( controller, scene ) );
			VrControllers.Add( vrController );

			controller.IsEnabled.BindValueChanged( v => {
				if ( v.NewValue )
					ActiveVrControllers.Add( vrController );
				else
					ActiveVrControllers.Remove( vrController );
			}, true );
			return;
		}

		scene.Add( new BasicVrDevice( device ) );
	}

	Bindable<HandSetting> dominantHandSetting = new( HandSetting.Right );
	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		var deps = new DependencyContainer( parent );

		deps.CacheAs( osuPanel.OsuDependencies );

		return base.CreateChildDependencies( deps );
	}

	protected override void LoadComplete () {
		base.LoadComplete();
		Add( scene );
		scene.Add( new GridScene() );

		var config = Dependencies.Get<OsuXrConfigManager>();
		config.BindWith( OsuXrSetting.DominantHand, dominantHandSetting );
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
