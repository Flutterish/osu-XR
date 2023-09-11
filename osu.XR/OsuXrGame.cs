using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Framework.XR.Input;
using osu.Framework.XR.Physics;
using osu.Framework.XR.Testing.VirtualReality;
using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;
using osu.Game.Overlays.Notifications;
using osu.XR.Graphics;
using osu.XR.Graphics.Panels;
using osu.XR.Graphics.Panels.Menu;
using osu.XR.Graphics.Player;
using osu.XR.Graphics.Sceneries;
using osu.XR.Graphics.VirtualReality;
using osu.XR.IO;

namespace osu.XR;

[Cached]
public partial class OsuXrGame : OsuXrGameBase {
	[Cached]
	PhysicsSystem physics = new();

	[Cached]
	BasicSceneMovementSystem movementSystem;

	[Cached]
	PanelInteractionSystem panelInteraction = new();

	[Cached]
	SceneryContainer scenery = new();
	public readonly OsuXrScene Scene;
	OsuGamePanel osuPanel;

	[Cached]
	[Cached(typeof(VrPlayer))]
	public readonly OsuXrPlayer Player;

	HandheldMenu menu;
	UserTrackingDrawable3D menuContainer;

	[Cached]
	public readonly VrKeyboard Keyboard;
	[Cached]
	public readonly VrKeyboardInputSource KeyboardInput;
	PanelInteractionSystem.Source keyboardInteractionSource;
	bool isKeyboardActive;

	public OsuXrGame ( bool useSimulatedVR = false ) : base( useSimulatedVR ) { // TODO I dont really like the 'useSimulatedVR' - can't we extract it up?
		Scene = new() {
			RelativeSizeAxes = Axes.Both
		};

		if ( useSimulatedVR )
			setupVrRig();

		Scene.Camera.Z = -5;
		Scene.Camera.Y = 1;
		Scene.Add( osuPanel = new() );

		physics.AddSubtree( Scene.Root );
		Add( movementSystem = new( Scene ) { RelativeSizeAxes = Axes.Both } );
		Add( new BasicPanelInteractionSource( Scene, physics, panelInteraction ) { RelativeSizeAxes = Axes.Both } );

		Scene.Add( menuContainer = new UserTrackingDrawable3D { Child = menu = new HandheldMenu(), Y = 1 } );
		menu.Notifications.Post( new SimpleNotification { Text = @"Welcome to OXR! This is the menu panel, which you can open and close with you VR controller (probably the A or B button). Check out other sections, configure your game, and close this menu when you are ready." } );
		Scene.Add( Player = new OsuXrPlayer() );

		Keyboard = new() {
			Scale = new( 0.02f ),
			Y = 1,
			Z = 0.5f,
			Rotation = Quaternion.FromAxisAngle( Vector3.UnitY, MathF.PI ) * Quaternion.FromAxisAngle( Vector3.UnitX, -MathF.PI * 1 / 4 )
		};
		KeyboardInput = new( Keyboard );
		keyboardInteractionSource = panelInteraction.GetSource( Keyboard );
		panelInteraction.PanelGainedFocusSource += (panel, source) => { // TODO show when there is input focus OR a toggle is pressed
			if ( !Keyboard.IsKeyboardPanel( panel ) ) {
				keyboardInteractionSource.FocusedPanel = panel;
			}
		};
		Keyboard.KeyDown = key => {
			keyboardInteractionSource.Press( key );
		};
		Keyboard.KeyUp = key => {
			keyboardInteractionSource.Release( key );
		};
		KeyboardInput.Activated = () => {
			Schedule( () => {
				isKeyboardActive = true;
				Scene.Add( Keyboard );
			} );
		};
		KeyboardInput.Deactivated = () => {
			Schedule( ()  => {
				isKeyboardActive = false;
				keyboardInteractionSource.ReleaseAllInput();
				Scene.Remove( Keyboard, disposeImmediately: false );
			} );
		};

		osuPanel.OsuDependencies.ExceptionInvoked += ( msg, ex ) => {
			menu.Notifications.Post( new SimpleErrorNotification { Text = msg } );
			Logger.Error( ex, msg.ToString(), recursive: true );
		};

		Compositor.BindDeviceDetected( addVrDevice );
	}

	protected override void OnBindingsLoadMessage ( BindingsSaveContext.Message message ) {
		base.OnBindingsLoadMessage( message );

		if ( message.Severity is Severity.Warning or Severity.Error ) {
			menu.Notifications.Post( new SimpleErrorNotification {
				Icon = message.Severity == Severity.Error ? FontAwesome.Solid.Bomb : FontAwesome.Solid.ExclamationTriangle,
				Text = message.Ruleset == null ? message.Text : message.Ruleset.AvailableVariants.Count() > 1
				? $@"{message.Text} in {message.Ruleset.RulesetInfo.Name} ({message.Ruleset.GetVariantName(message.Variant!.Value)})"
				: $@"{message.Text} in {message.Ruleset.RulesetInfo.Name}"
			} );

			menu.Panels.FocusPanel( menu.Notifications );
			menuContainer.IsOpen = true;
		}
	}

	void setupVrRig () {
		var comp = (TestingVrCompositor)Compositor;
		TestingRig rig = new( Scene ) { Depth = -1 };
		Add( rig );

		Schedule( () => comp.AddRig( rig ) );

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
	}

	public readonly BindableList<VrController> VrControllers = new();
	public readonly BindableList<VrController> ActiveVrControllers = new();

	void addVrDevice ( VrDevice device ) {
		if ( device is Controller controller ) {
			VrController vrController;
			Scene.Add( vrController = new VrController( controller, Scene ) );
			VrControllers.Add( vrController );

			controller.IsEnabled.BindValueChanged( v => {
				if ( v.NewValue )
					ActiveVrControllers.Add( vrController );
				else
					ActiveVrControllers.Remove( vrController );
			}, true );
			return;
		}

		var model = new BasicVrDevice( device );
		if ( device is Headset ) {
			model.RenderLayer = RenderLayer.HMD;
		}

		Scene.Add( model );
	}

	public VrController? PrimaryController
		=> VrControllers.FirstOrDefault( x => x.Hand == DominantHand.Value );
	public VrController? SecondaryController
		=> VrControllers.FirstOrDefault( x => x.Hand != DominantHand.Value );

	public VrController? PrimaryActiveController
		=> ActiveVrControllers.OrderBy( x => x.Hand == DominantHand.Value ? 1 : 2 ).FirstOrDefault();
	public VrController? SecondaryActiveController
		=> ActiveVrControllers.OrderBy( x => x.Hand == DominantHand.Value ? 1 : 2 ).Skip( 1 ).FirstOrDefault();

	public VrController ControllerFor ( Controller source )
		=> VrControllers.Single( x => x.Source == source );

	public Headset? Headset => Compositor.TrackedDevices.OfType<Headset>().FirstOrDefault();

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		var deps = new DependencyContainer( parent );

		deps.CacheAs( osuPanel.OsuDependencies );

		return base.CreateChildDependencies( deps );
	}

	protected override void LoadComplete () {
		base.LoadComplete();
		Add( BeatSync );
		Add( Scene );
		Scene.Add( scenery ); 
		Add( new ScreenOverlay() );
	}
}
