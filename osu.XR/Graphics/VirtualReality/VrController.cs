using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Input;
using osu.Framework.XR.Physics;
using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;
using osu.XR.Configuration;
using osu.XR.Graphics.Player;
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

		AddInternal( teleportVisual );

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

	Bindable<bool> disableTeleport = new( false );
	TeleportVisual teleportVisual = new();

	IPointer? pointer;
	RayPointer rayPointer = new();
	TouchPointer touchPointer = new();
	void setPointer ( IPointer? pointer ) {
		if ( pointer == this.pointer )
			return;

		if ( this.pointer is Drawable3D old )
			scene.Remove( old, disposeImmediately: false );
		if ( pointer is Drawable3D @new ) {
			scene.Add( @new );
			pointer.SetTint( Hand is Hand.Left ? Colour4.Cyan : Colour4.Orange );
		}

		this.pointer = pointer;
		unfocus();

		updateTouchSetting();
	}

	void updateTouchSetting () {
		useTouchBindable.Value = pointerTouch.Value
			|| pointer?.IsTouchSource == true
			|| (HoveredCollider != null && activeControllers.Count( x => x.HoveredCollider == HoveredCollider ) >= 2);
		
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
	Bindable<bool> pointerTouch = new( false );
	Bindable<bool> tapStrum = new( false );
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

		var teleport = source.GetAction<BooleanAction>( VrAction.Teleport );
		teleport.ValueBindable.BindValueChanged( v => {
			teleportVisual.IsActive.Value = v.NewValue && !disableTeleport.Value;
			if ( !v.NewValue ) {
				teleportPlayer();
			}
		} );

		void teleportPlayer () {
			if ( teleportVisual.HasHitGround && !disableTeleport.Value ) {
				var offset = teleportVisual.HitPosition - ( game.Player.Position - game.Player.PositionOffset );
				game.Player.PositionOffset = new Vector3( offset.X, 0, offset.Z );
			}
		}

		scroll = source.GetAction<Vector2Action>( VrAction.Scroll );
		haptic = source.GetAction<HapticAction>( VrAction.Feedback );

		if ( config != null ) {
			config.BindWith( OsuXrSetting.InputMode, inputMode );
			config.BindWith( OsuXrSetting.TouchPointers, pointerTouch );
			config.BindWith( OsuXrSetting.TapStrum, tapStrum );
			config.BindWith( OsuXrSetting.DisableTeleport, disableTeleport );
		}

		inputMode.BindValueChanged( _ => updatePointerType() );
		activeControllers.BindTo( game.ActiveVrControllers );
		activeControllers.BindCollectionChanged( ( _, _ ) => updatePointerType() );
		dominantHand.BindTo( game.DominantHand );
		dominantHand.BindValueChanged( _ => updatePointerType() );

		updatePointerType();
		leftButton.OnPressed += () => onButtonStateChanged( true, VrAction.LeftButton );
		leftButton.OnRepeated += () => onButtonStateChanged( true, VrAction.LeftButton, isRepeated: true );
		leftButton.OnReleased += () => onButtonStateChanged( false, VrAction.LeftButton );
		rightButton.OnPressed += () => onButtonStateChanged( true, VrAction.RightButton );
		rightButton.OnRepeated += () => onButtonStateChanged( true, VrAction.RightButton, isRepeated: true );
		rightButton.OnReleased += () => onButtonStateChanged( false, VrAction.RightButton );
		menuButton.OnPressed += () => ToggleMenuPressed?.Invoke( this );
	}

	VrController relayController => inputMode.Value is InputMode.SinglePointer // in single pointer, the offhand should activate main hand buttons
		? activeControllers.OrderBy( x => x.Hand == dominantHand.Value ? 1 : 2 ).Append( this ).First()
		: (useTouch || pointer is null) && HoveredCollider is null // in touch modes, unfocused pointers should activate focused hand buttons
		? activeControllers.Where( x => x.HoveredCollider != null ).Append( this ).First()
		: this;

	void onInputSourceValueChanged ( VrAction action, bool isDown ) {
		var ourButton = action switch {
			VrAction.LeftButton => leftButton,
			VrAction.RightButton => rightButton,
			_ => menuButton
		};
		if ( isDown ) {
			var target = action is VrAction.ToggleMenu ? this : relayController;
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

	Vector2Action scroll = null!;
	void onScroll ( Vector2 value ) {
		var panel = HoveredCollider as Panel ?? inputSource.FocusedPanel;
		if ( panel is null ) {
			var target = relayController;
			if ( target != this )
				target?.onScroll( value );
			return;
		}

		panel.Content.Scroll += value;
	}

	/// <summary>
	/// Objects which prevent this pointer from working for various reasons such as getting too close, or holding something with this hand
	/// </summary>
	public readonly BindableList<object> SuppressionSources = new();

	bool isTouchDown;
	Vector2 currentPosition;
	MouseButton buttonFor ( VrAction action ) => action is VrAction.LeftButton ? MouseButton.Left : MouseButton.Right;
	void onButtonStateChanged ( bool value, VrAction action, bool isFromTouch = false, bool isRepeated = false ) {
		if ( HoveredCollider is Panel panel ) {
			inputSource.FocusedPanel = panel;
		}
		else if ( inputSource.FocusedPanel is null || value ) return;

		if ( useTouch )
			handleTouch( value, action, isFromTouch );
		else
			handleMouse( value, action, isRepeated );
	}

	void handleTouch ( bool value, VrAction action, bool isFromTouch ) {
		if ( action is not VrAction.LeftButton )
			return;
		
		if ( isTouchDown ) {
			if ( value ) {
				inputSource.TouchUp();
				inputSource.TouchDown( currentPosition );
			}
			else if ( pointer!.IsTouchSource == false || isFromTouch ) {
				isTouchDown = false;
				SendHapticVibration( 0.05, 10, 0.3 );
				inputSource.TouchUp();
			}
			else if ( tapStrum.Value ) {
				inputSource.TouchUp();
				inputSource.TouchDown( currentPosition );
			}
		}
		else if ( value ) {
			isTouchDown = true;
			SendHapticVibration( 0.05, 20, 0.5 );
			inputSource.TouchDown( currentPosition );
		}
	}

	void handleMouse ( bool value, VrAction action, bool isRepeated ) {
		if ( value ) {
			if ( isRepeated )
				inputSource.Release( buttonFor( action ) );

			inputSource.Press( buttonFor( action ) );
		}
		else {
			inputSource.Release( buttonFor( action ) );
		}
	}

	PoseAction? aim;
	protected override void Update () {
		base.Update();
		updateTouchSetting();
		onScroll( scroll.Value * (float)Time.Elapsed / 1000 * 30 );

		Vector3 pos = Position;
		Quaternion rot = Rotation;
		if ( aim?.FetchDataForNextFrame() is PoseInput pose ) {
			pos = pose.Position;
			rot = pose.Rotation;
		}
		// TODO report if theres no aim data

		teleportVisual.OriginBindable.Value = pos;
		teleportVisual.DirectionBindable.Value = rot.Apply( Vector3.UnitZ ) * 5;

		if ( pointer is null )
			return;

		var maybeHit = pointer.UpdatePointer( pos, rot );

		if ( maybeHit is PointerHit hit ) {
			HoveredCollider = hit.Collider;
			if ( hit.Collider is Panel panel ) {
				updateTouchSetting();
				currentPosition = panel.GlobalSpaceContentPositionAt( hit.TrisIndex, hit.Point );

				if ( pointer.IsTouchSource && !isTouchDown ) {
					onButtonStateChanged( true, VrAction.LeftButton, isFromTouch: true );
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
			}

			HoveredCollider = null;
		}
	}

	class Button {
		Button? actuated;
		int inputSourceCount;
		public bool IsDown { get; private set; }

		public void Actuate ( Button? who ) {
			if ( actuated == who )
				return;

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
				else if ( wasTargetDown == true ) {
					who.OnRepeated?.Invoke();
				}
			}
		}

		public event Action? OnPressed;
		public event Action? OnRepeated;
		public event Action? OnReleased;
	}

	Button leftButton = new();
	Button rightButton = new();
	Button menuButton = new();
	public event Action<VrController>? ToggleMenuPressed;
}