using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Input;
using osu.Framework.XR.VirtualReality;
using osu.XR.Configuration;
using osu.XR.Graphics.Player;
using osu.XR.Graphics.VirtualReality.Pointers;
using osu.XR.Osu;
using Controller = osu.Framework.XR.VirtualReality.Devices.Controller;
using Headset = osu.Framework.XR.VirtualReality.Devices.Headset;

namespace osu.XR.Graphics.VirtualReality;

public partial class VrController : BasicVrDevice, IControllerRelay {
	Controller source;
	public bool IsEnabled => source.IsEnabled.Value;
	public Hand Hand => source.Role == Valve.VR.ETrackedControllerRole.LeftHand ? Hand.Left : Hand.Right;
	public Colour4 AccentColour => Hand is Hand.Left ? Colour4.Cyan : Colour4.Orange;

	Scene scene;
	public VrController ( Controller source, Scene scene ) : base( source ) {
		this.scene = scene;
		this.source = source;

		AddInternal( teleportVisual );

		source.IsEnabled.BindValueChanged( v => {
			updatePointerType();

			if ( !v.NewValue ) {
				menuButton.Actuate( null );
				foreach ( var i in leftActuators.Concat( rightActuators ) )
					i.Actuate( null );
			}
		} );

		SuppressionSources.BindCollectionChanged( (_, _) => {
			updatePointerType();
		} );
	}

	Bindable<bool> settingsTeleportDisabled = new( false );
	Bindable<PlayerInfo?> currentPlayer = new();

	Bindable<bool> disableTeleport = new( false );
	TeleportVisual teleportVisual = new();

	IPointerBase? pointerSource;
	Pointer[]? pointers;

	Dictionary<IPointerBase, Pointer[]> pointersBySource = new();
	RayPointer? rayPointer;
	TouchPointer? touchPointer;
	HandSkeletonPointer? handSkeletonPointer;

	Pointer createPointer ( IPointer source ) {
		var pointer = new Pointer( source, InteractionSystem );
		bindPointer( pointer );
		return pointer;
	}

	void bindPointer ( Pointer pointer ) {
		pointer.TapStrum.BindTo( tapStrum );
		pointer.TouchDownStateChanged += v => {
			var multiplier = currentPlayer.Value?.CursorOpacityFromMods ?? 1f;
			if ( v )
				SendHapticVibration( 0.05, 20, 0.5 * multiplier );
			else
				SendHapticVibration( 0.05, 10, 0.3 * multiplier );
		};
	}

	void setPointerSource ( IPointerBase? pointerSource ) {
		if ( pointerSource == this.pointerSource )
			return;

		if ( pointers != null ) {
			foreach ( var i in pointers ) {
				i.Blur();
			}
			pointers = null;
		}

		if ( this.pointerSource is IPointerBase old )
			old.RemoveFromScene( scene );
		if ( pointerSource is IPointerBase @new ) {
			@new.AddToScene( scene );
			if ( !pointersBySource.TryGetValue( @new, out pointers ) ) {
				if ( pointerSource is IPointerSource source ) {
					pointersBySource.Add( @new, pointers = source.Pointers.Select( x => {
						bindPointer( x );
						return x;
					} ).ToArray() );
					source.SetTint( AccentColour );
				}
				else if ( pointerSource is IPointer pointer ) {
					pointersBySource.Add( @new, pointers = new[] { createPointer( pointer ) } );
					pointer.SetTint( AccentColour );
				}
			}
		}

		this.pointerSource = pointerSource;

		updateTouchSetting();
	}

	void updateTouchSetting () {
		if ( pointers == null ) {
			IsVisible = true;
			return;
		}

		foreach ( var i in pointers ) {
			i.ForceTouch = pointerTouch.Value 
				|| (i.HoveredCollider != null && activeControllers.SelectMany( x => x.pointers ?? Array.Empty<Pointer>() ).Count( x => x.HoveredCollider == i.HoveredCollider ) >= 2);
		}
		
		IsVisible = !pointers.Any( x => x.Source.IsTouchSource );
	}

	void updatePointerType () {
		setPointerSource( (IsEnabled && SuppressionSources.Count == 0) ? inputMode.Value switch {
			InputMode.TouchScreen => touchPointer ??= new(),
			InputMode.FingerTouchScreen => handSkeletonPointer ??= new( this ),
			InputMode.DoublePointer => rayPointer ??= new(),
			_ => ( activeControllers.Count == 1 || Hand == activeHand.Value ) ? (rayPointer ??= new()) : null
		} : null );
	}

	HapticAction haptic = null!;
	public void SendHapticVibration ( double duration, double frequency = 40, double amplitude = 1, double delay = 0 ) {
		haptic?.TriggerVibration( duration, frequency, amplitude, delay );
	}

	Bindable<Hand> activeHand = new( Hand.Right );
	Bindable<InputMode> inputMode = new( InputMode.DoublePointer );
	Bindable<bool> pointerTouch = new( false );
	Bindable<bool> tapStrum = new( false );
	BindableList<VrController> activeControllers = new();

	[Resolved]
	VrCompositor compositor { get; set; } = null!;

	[Resolved]
	public PanelInteractionSystem InteractionSystem { get; private set; } = null!;
	[BackgroundDependencyLoader( true )]
	private void load ( OsuXrConfigManager? config, OsuXrGame game, OsuDependencies osu ) {
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
			config.BindWith( OsuXrSetting.DisableTeleport, settingsTeleportDisabled );
		}
		currentPlayer.BindTo( osu.Player );

		inputMode.BindValueChanged( _ => updatePointerType() );
		activeControllers.BindTo( game.ActiveVrControllers );
		activeControllers.BindCollectionChanged( ( _, _ ) => updatePointerType() );
		activeHand.BindTo( game.ActiveHand );
		activeHand.BindValueChanged( _ => updatePointerType() );

		menuButton.OnPressed += () => {
			ToggleMenuPressed?.Invoke( this );
		};
		updatePointerType();
	}

	void updateTeleportCapability () {
		disableTeleport.Value = settingsTeleportDisabled.Value || (currentPlayer.Value != null && !currentPlayer.Value.IsPaused);
	}

	public bool IsPointingToAnything => pointers?.Any( x => x.HoveredCollider is Panel ) == true;
	public bool IsFocusedOnAnything => pointers?.Any( x => x.InputSource.FocusedPanel != null || (x.UsesTouch && x.HoveredCollider is Panel) ) == true;
	public bool UsesTouch => pointers?.Any( x => x.UsesTouch ) == true;

	public IControllerRelay? CustomRelay;
	IControllerRelay getRelay ( bool permitIndirect ) { // TODO tab controls
		foreach ( var controller in activeControllers.OrderBy( x => x == this ? 1 : 2 ) ) {
			if ( controller.IsPointingToAnything )
				return controller;

			if ( permitIndirect && controller.IsFocusedOnAnything )
				return controller;

			if ( controller.CustomRelay != null )
				return controller.CustomRelay;
		}

		return this;
	}

	List<RelayButton> leftActuators = new();
	List<RelayButton> rightActuators = new();
	void onInputSourceValueChanged ( VrAction action, bool isDown ) {
		if ( action is VrAction.ToggleMenu ) {
			menuButton.Actuate( isDown ? menuButton : null );
			return;
		}

		var actuators = action is VrAction.LeftButton ? leftActuators : rightActuators;
		var pointerButtons = getRelay( permitIndirect: false ).GetButtonsFor( this, action );

		int n = 0;
		if ( isDown ) {
			foreach ( var button in pointerButtons ) {
				if ( n >= actuators.Count )
					actuators.Add( new() );

				actuators[n++].Actuate( button );
			}
		}
		for ( ; n < actuators.Count; n++ ) {
			actuators[n].Actuate( null );
		}
	}
	public IEnumerable<RelayButton> GetButtonsFor ( VrController source, VrAction action ) {
		if ( inputMode.Value == InputMode.SinglePointer && currentPlayer.Value?.IsPaused is null or true )
			activeHand.Value = Hand;

		if ( pointers is null )
			return Array.Empty<RelayButton>();

		return pointers.DistinctBy( x => x.HoveredCollider ).Select( x => action is VrAction.LeftButton ? x.LeftButton : x.RightButton );
	}

	Vector2Action scroll = null!;
	void onScroll ( Vector2 value ) {
		getRelay( permitIndirect: true ).ScrollBy( this, value );
	}
	public void ScrollBy ( VrController source, Vector2 amount ) {
		if ( currentPlayer.Value != null )
			return;

		var pointers = this.pointers?.DistinctBy( x => x.HoveredCollider );
		if ( pointers is null )
			return;

		foreach ( var i in pointers ) {
			var panel = i.HoveredCollider as Panel ?? i.InputSource.FocusedPanel;
			if ( panel is null )
				continue;

			panel.Content.Scroll += amount;
		}
	}

	/// <summary>
	/// Objects which prevent this pointer from working for various reasons such as getting too close, or holding something with this hand
	/// </summary>
	public readonly BindableList<object> SuppressionSources = new(); // joy cursors might want to add themselves here?

	PoseAction? aim;
	protected override void Update () {
		base.Update();
		pointerSource?.SetTint( AccentColour.MultiplyAlpha( currentPlayer.Value?.CursorOpacityFromMods ?? 1f ) );
		updateTeleportCapability();
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

		Vector3 player = compositor.TrackedDevices.OfType<Headset>().SingleOrDefault() is Headset headset ? headset.GlobalPosition : Vector3.Zero;
		if ( pointerSource is IPointerSource source ) {
			source.UpdatePointers( player, pos, rot ).Dispose();
		}
		else if ( pointers != null ) {
			foreach ( var i in pointers ) {
				i.Update( player, pos, rot );
			}
		}
	}

	RelayButton menuButton = new();
	public event Action<VrController>? ToggleMenuPressed;
}