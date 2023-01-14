﻿using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Input;
using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;
using osu.XR.Configuration;
using osu.XR.Graphics.Player;
using osu.XR.Graphics.VirtualReality.Pointers;

namespace osu.XR.Graphics.VirtualReality;

public partial class VrController : BasicVrDevice {
	Controller source;
	public bool IsEnabled => source.IsEnabled.Value;
	public Hand Hand => source.Role == Valve.VR.ETrackedControllerRole.LeftHand ? Hand.Left : Hand.Right;

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

	Bindable<bool> disableTeleport = new( false );
	TeleportVisual teleportVisual = new();

	Drawable3D? pointerSource;
	Pointer[]? pointers;

	Dictionary<Drawable3D, Pointer[]> pointersBySource = new();
	RayPointer? rayPointer;
	TouchPointer? touchPointer;
	HandSkeletonPointer? handSkeletonPointer;

	Pointer createPointer ( IPointer source ) {
		var pointer = new Pointer( source, InteractionSystem );
		pointer.TapStrum.BindTo( tapStrum );
		pointer.TouchDownStateChanged += v => {
			if ( v )
				SendHapticVibration( 0.05, 20, 0.5 ); 
			else
				SendHapticVibration( 0.05, 10, 0.3 );
		};
		return pointer;
	}
	Pointer wrapPointer ( Pointer pointer ) {
		pointer.TapStrum.BindTo( tapStrum );
		pointer.TouchDownStateChanged += v => {
			if ( v )
				SendHapticVibration( 0.05, 20, 0.5 );
			else
				SendHapticVibration( 0.05, 10, 0.3 );
		};
		return pointer;
	}

	void setPointerSource ( Drawable3D? pointerSource ) {
		if ( pointerSource == this.pointerSource )
			return;

		if ( pointers != null ) {
			foreach ( var i in pointers ) {
				i.Blur();
			}
			pointers = null;
		}

		if ( this.pointerSource is Drawable3D old )
			scene.Remove( old, disposeImmediately: false );
		if ( pointerSource is Drawable3D @new ) {
			scene.Add( @new );
			if ( !pointersBySource.TryGetValue( @new, out pointers ) ) {
				var tint = Hand is Hand.Left ? Colour4.Cyan : Colour4.Orange;
				if ( pointerSource is IPointerSource source ) {
					pointersBySource.Add( @new, pointers = source.Pointers.Select( wrapPointer ).ToArray() );
					source.SetTint( tint );
				}
				else if ( pointerSource is IPointer pointer ) {
					pointersBySource.Add( @new, pointers = new[] { createPointer( pointer ) } );
					pointer.SetTint( tint );
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
			_ => ( activeControllers.Count == 1 || Hand == dominantHand.Value ) ? (rayPointer ??= new()) : null
		} : null );
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

	[Resolved]
	public PanelInteractionSystem InteractionSystem { get; set; } = null!;
	[BackgroundDependencyLoader( true )]
	private void load ( OsuXrConfigManager? config, OsuXrGame game ) {
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

		menuButton.OnPressed += () => ToggleMenuPressed?.Invoke( this );
		updatePointerType();
	}

	public bool IsFocusedOnAnything => pointers?.Any( x => x.HoveredCollider != null ) == true;
	public bool UsesTouch => pointers?.Any( x => x.UsesTouch ) == true;
	VrController relayController => inputMode.Value is InputMode.SinglePointer // in single pointer, the offhand should activate main hand buttons
		? activeControllers.OrderBy( x => x.Hand == dominantHand.Value ? 1 : 2 ).Append( this ).First()
		: (UsesTouch || pointers == null) && !IsFocusedOnAnything // in touch modes, unfocused pointers should activate focused hand buttons
		? activeControllers.Where( x => x.IsFocusedOnAnything ).Append( this ).First()
		: this;

	List<PointerButton> leftActuators = new();
	List<PointerButton> rightActuators = new();
	void onInputSourceValueChanged ( VrAction action, bool isDown ) {
		if ( action is VrAction.ToggleMenu ) {
			menuButton.Actuate( isDown ? menuButton : null );
			return;
		}

		var controller = relayController;
		var actuators = action is VrAction.LeftButton ? leftActuators : rightActuators;
		var pointerButtons = relayController.pointers?.DistinctBy( x => x.HoveredCollider ).Select( x => action is VrAction.LeftButton ? x.LeftButton : x.RightButton ).GetEnumerator();
		if ( pointerButtons is not IEnumerator<PointerButton> enumerator ) {
			foreach ( var i in actuators )
				i.Actuate( null );
			return;
		}

		int n = 0;
		if ( isDown ) {
			while ( pointerButtons.MoveNext() ) {
				if ( n >= actuators.Count )
					actuators.Add( new() );

				actuators[n++].Actuate( pointerButtons.Current );
			}
		}
		for ( ; n < actuators.Count; n++ ) {
			actuators[n].Actuate( null );
		}
	}

	Vector2Action scroll = null!;
	void onScroll ( Vector2 value ) {
		var pointers = relayController.pointers?.DistinctBy( x => x.HoveredCollider );
		if ( pointers is null )
			return;

		foreach ( var i in pointers ) {
			var panel = i.HoveredCollider as Panel ?? i.InputSource.FocusedPanel;
			if ( panel is null )
				continue;

			panel.Content.Scroll += value;
		}
	}

	/// <summary>
	/// Objects which prevent this pointer from working for various reasons such as getting too close, or holding something with this hand
	/// </summary>
	public readonly BindableList<object> SuppressionSources = new();

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

		if ( pointerSource is IPointerSource source ) {
			source.UpdatePointers( pos, rot ).Dispose();
		}
		else if ( pointers != null ) {
			foreach ( var i in pointers ) {
				i.Update( pos, rot );
			}
		}
	}

	PointerButton menuButton = new();
	public event Action<VrController>? ToggleMenuPressed;
}