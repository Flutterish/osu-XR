using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Input;
using osu.Framework.XR.Physics;
using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;
using osu.XR.Configuration;
using osuTK.Input;

namespace osu.XR.Graphics.VirtualReality;

public partial class VrController : BasicVrDevice {
	Controller source;
	public bool IsEnabled => source.IsEnabled.Value;
	public Hand Hand => source.Role == Valve.VR.ETrackedControllerRole.LeftHand ? Hand.Left : Hand.Right;

	Scene scene;
	public IHasCollider? HoveredCollider { get; private set; }
	public VrController ( Controller source, Scene scene ) : base( source ) {
		this.scene = scene;
		this.source = source;

		source.IsEnabled.BindValueChanged( v => {
			updatePointerType();

			if ( !v.NewValue ) {
				leftButton.Actuate( null );
				rightButton.Actuate( null );
				menuButton.Actuate( null );
			}
		} );

		SuppressionSources.BindCollectionChanged( (_, _) => {
			updatePointerType();
		} );
	}

	IPointer? pointer;
	RayPointer rayPointer = new();
	TouchPointer touchPointer = new();
	void setPointer ( IPointer? pointer ) {
		if ( this.pointer is Drawable3D old )
			scene.Remove( old, disposeImmediately: false );
		if ( pointer is Drawable3D @new ) {
			scene.Add( @new );
			pointer.SetTint( Hand is Hand.Left ? Colour4.Cyan : Colour4.Orange );
		}

		this.pointer = pointer;
		if ( pointer is null )
			unfocus();

		updateTouchSetting();
	}

	void updateTouchSetting () {
		if ( pointer?.IsTouchSource == true ) {
			useTouchBindable.Value = !tapOnPress.Value;
		}
		else {
			useTouchBindable.Value = singlePointerTouch.Value
				|| activeControllers.Count( x => x.HoveredCollider == HoveredCollider ) >= 2;
		}
		
		IsVisible = pointer?.IsTouchSource != true;
	}

	void updatePointerType () {
		setPointer( (IsEnabled && SuppressionSources.Count == 0) ? inputMode.Value switch {
			InputMode.TouchScreen => touchPointer,
			InputMode.DoublePointer => rayPointer,
			_ => ( activeControllers.Count == 1 || Hand == dominantHand.Value ) ? rayPointer : null
		} : null );
	}

	void unfocus () {
		inputSource.FocusedPanel = null;
		HoveredCollider = null;
		isTouchDown = false;
	}

	HapticAction haptic = null!;
	public void SendHapticVibration ( double duration, double frequency = 40, double amplitude = 1, double delay = 0 ) {
		haptic?.TriggerVibration( duration, frequency, amplitude, delay );
	}

	Bindable<Hand> dominantHand = new( Hand.Right );
	Bindable<InputMode> inputMode = new( InputMode.DoublePointer );
	Bindable<bool> singlePointerTouch = new( false );
	Bindable<bool> tapOnPress = new( false );
	BindableList<VrController> activeControllers = new();

	BindableBool useTouchBindable = new();
	bool useTouch => useTouchBindable.Value;

	PanelInteractionSystem.Source inputSource = null!;
	[BackgroundDependencyLoader( true )]
	private void load ( PanelInteractionSystem system, OsuXrConfigManager? config, OsuXrGame game ) {
		inputSource = system.GetSource( this );

		var left = source.GetAction<BooleanAction>( VrAction.LeftButton );
		var right = source.GetAction<BooleanAction>( VrAction.RightButton );
		var menu = source.GetAction<BooleanAction>( VrAction.ToggleMenu );
		aim = source.GetAction<PoseAction>( VrAction.ControllerTip );

		foreach ( var (button, action) in new[] { (left, VrAction.LeftButton), (right, VrAction.RightButton), (menu, VrAction.ToggleMenu) } ) {
			button.ValueBindable.BindValueChanged( v => {
				onInputSourceValueChanged( action, v.NewValue );
			} );
		}

		haptic = source.GetAction<HapticAction>( VrAction.Feedback );

		if ( config != null ) {
			config.BindWith( OsuXrSetting.InputMode, inputMode );
			config.BindWith( OsuXrSetting.SinglePointerTouch, singlePointerTouch );
			config.BindWith( OsuXrSetting.TapOnPress, tapOnPress );
		}

		inputMode.BindValueChanged( _ => updatePointerType() );
		activeControllers.BindTo( game.ActiveVrControllers );
		activeControllers.BindCollectionChanged( ( _, _ ) => updatePointerType() );
		dominantHand.BindTo( game.DominantHand );
		dominantHand.BindValueChanged( _ => updatePointerType() );

		updatePointerType();
		leftButton.OnPressed += () => onButtonStateChanged( true, VrAction.LeftButton, isFromTouch: false );
		leftButton.OnReleased += () => onButtonStateChanged( false, VrAction.LeftButton, isFromTouch: false );
		rightButton.OnPressed += () => onButtonStateChanged( true, VrAction.RightButton, isFromTouch: false );
		rightButton.OnReleased += () => onButtonStateChanged( false, VrAction.RightButton, isFromTouch: false );
		menuButton.OnPressed += () => ToggleMenuPressed?.Invoke( this );
	}

	void onInputSourceValueChanged ( VrAction action, bool isDown ) {
		var ourButton = action switch {
			VrAction.LeftButton => leftButton,
			VrAction.RightButton => rightButton,
			_ => menuButton
		};
		if ( isDown ) {
			var target = inputMode.Value is InputMode.SinglePointer ? activeControllers.OrderBy( x => x.Hand == dominantHand.Value ? 1 : 2 ).First() : this;
			var theirButton = action switch {
				VrAction.LeftButton => target.leftButton,
				VrAction.RightButton => target.rightButton,
				_ => target.menuButton
			};

			ourButton.Actuate( theirButton );
		}
		else {
			ourButton.Actuate( null );
		}
	}

	/// <summary>
	/// Objects which prevent this pointer from working for various reasons such as getting too close, or holding something with this hand
	/// </summary>
	public readonly BindableList<object> SuppressionSources = new();

	bool isTouchDown;
	Vector2 currentPosition;
	MouseButton buttonFor ( VrAction action ) => action is VrAction.LeftButton ? MouseButton.Left : MouseButton.Right;
	void onButtonStateChanged ( bool value, VrAction action, bool isFromTouch ) {
		if ( useTouch && !isFromTouch && !isTouchDown )
			return;

		if ( value ) {
			if ( HoveredCollider is Panel panel ) {
				inputSource.FocusedPanel = panel;
			}
			else return;

			if ( useTouch ) {
				if ( action is VrAction.LeftButton ) {
					if ( !isFromTouch )
						inputSource.TouchUp();

					isTouchDown = true;
					inputSource.TouchDown( currentPosition );
				}
			}
			else {
				inputSource.MoveMouse( currentPosition );
				inputSource.Press( buttonFor( action ) );
			}
		}
		else {
			if ( isTouchDown ) {
				if ( action is VrAction.LeftButton && isFromTouch ) {
					isTouchDown = false;
					inputSource.TouchUp();
				}
			}
			else {
				inputSource.Release( buttonFor( action ) );
			}
		}
	}

	PoseAction? aim;
	protected override void Update () {
		base.Update();
		updateTouchSetting();

		if ( pointer is null )
			return;

		Vector3 pos = Position;
		Quaternion rot = Rotation;
		if ( aim?.FetchDataForNextFrame() is OpenVR.NET.Input.PoseInput pose ) {
			pos = pose.Position.ToOsuTk();
			rot = pose.Rotation.ToOsuTk();
		}
		// TODO report if theres no aim data

		var maybeHit = pointer.UpdatePointer( pos, rot );

		if ( maybeHit is PointerHit hit ) {
			HoveredCollider = hit.Collider;
			if ( hit.Collider is Panel panel ) {
				updateTouchSetting();
				currentPosition = panel.GlobalSpaceContentPositionAt( hit.TrisIndex, hit.Point );

				if ( useTouch && !isTouchDown ) {
					onButtonStateChanged( true, VrAction.LeftButton, isFromTouch: true );
					SendHapticVibration( 0.05, 20, 0.5 );
				}

				if ( isTouchDown )
					inputSource.TouchMove( currentPosition );
				else if ( !useTouch )
					panel.Content.MoveMouse( currentPosition );
			}
		}
		else {
			if ( useTouch && isTouchDown ) {
				onButtonStateChanged( false, VrAction.LeftButton, isFromTouch: true );
				SendHapticVibration( 0.05, 10, 0.3 );
			}

			HoveredCollider = null;
		}
	}

	class Button {
		Button? actuated;
		int inputSourceCount;
		public bool IsDown { get; private set; }

		public void Actuate ( Button? who ) {
			var wasOldDown = actuated?.IsDown;
			var wasTargetDown = who?.IsDown;

			var old = actuated;
			if ( old != null )
				old.inputSourceCount--;
			if ( who != null )
				who.inputSourceCount++;
			actuated = who;

			if ( old != null ) {
				old.IsDown = old.inputSourceCount != 0;
				if ( wasOldDown == true && !old.IsDown )
					old.OnReleased?.Invoke();
			}
			if ( who != null ) {
				who.IsDown = who.inputSourceCount != 0;
				if ( wasTargetDown == false && who.IsDown )
					who.OnPressed?.Invoke();
			}
		}

		public event Action? OnPressed;
		public event Action? OnReleased;
	}

	Button leftButton = new();
	Button rightButton = new();
	Button menuButton = new();
	public event Action<VrController>? ToggleMenuPressed;
}