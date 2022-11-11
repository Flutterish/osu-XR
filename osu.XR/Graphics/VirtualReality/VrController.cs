using osu.Framework.XR;
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

public class VrController : BasicVrDevice {
	Controller source;
	RayPointer rayPointer = new();
	TouchPointer touchPointer = new();

	public bool IsEnabled => source.IsEnabled.Value;
	public Hand Hand => source.Role == Valve.VR.ETrackedControllerRole.LeftHand ? Hand.Left : Hand.Right;
	IPointer? pointer;

	public IHasCollider? HoveredCollider { get; private set; }
	PoseAction? aim;

	Scene scene;
	public VrController ( Controller source, Scene scene ) : base( source ) {
		this.scene = scene;
		this.source = source;

		source.IsEnabled.BindValueChanged( _ => {
			updatePointerType();
		} );
	}

	void setPointer ( IPointer? pointer ) {
		if ( this.pointer is Drawable3D old )
			scene.Remove( old, disposeImmediately: false );
		if ( pointer is Drawable3D @new )
			scene.Add( @new );

		this.pointer = pointer;
		if ( pointer is null ) {
			inputSource.FocusedPanel = null;
			HoveredCollider = null;
			isTouchDown = false;
		}

		updateTouchSetting();
	}

	Bindable<Hand> dominantHand = new( Hand.Right );
	Bindable<InputMode> inputMode = new( InputMode.DoublePointer );
	Bindable<bool> singlePointerTouch = new( false );
	BindableList<VrController> activeControllers = new();

	bool isTouchDown;
	BindableBool useTouchBindable = new();
	bool useTouch => useTouchBindable.Value;
	MouseButton buttonFor ( VrAction action ) => action is VrAction.LeftButton ? MouseButton.Left : MouseButton.Right;

	Vector2 currentPosition;
	PanelInteractionSystem.Source inputSource = null!;
	[BackgroundDependencyLoader(true)]
	private void load ( PanelInteractionSystem system, OsuXrConfigManager? config, OsuXrGame game ) {
		inputSource = system.GetSource( this );

		var left = source.GetAction<BooleanAction>( VrAction.LeftButton );
		var right = source.GetAction<BooleanAction>( VrAction.RightButton );
		var globalLeft = game.Compositor.Input.GetAction<BooleanAction>( VrAction.LeftButton );
		var globalRight = game.Compositor.Input.GetAction<BooleanAction>( VrAction.RightButton );
		aim = source.GetAction<PoseAction>( VrAction.ControllerTip );

		foreach ( var (button, globalButton, action) in new[] { (left, globalLeft, VrAction.LeftButton), (right, globalRight, VrAction.RightButton) } ) {
			(button.ValueBindable, globalButton.ValueBindable).BindValuesChanged( ( local, global ) => {
				if ( pointer?.IsTouchSource == true )
					return;

				onButtonStateChanged( ( inputMode.Value is InputMode.SinglePointer && pointer != null ) ? global : local, action );
			} );
		}

		source.GetAction<BooleanAction>( VrAction.ToggleMenu ).ValueBindable.BindValueChanged( v => {
			if ( v.NewValue )
				ToggleMenuPressed?.Invoke( this );
		} );

		if ( config != null ) {
			config.BindWith( OsuXrSetting.InputMode, inputMode );
			config.BindWith( OsuXrSetting.SinglePointerTouch, singlePointerTouch );
		}

		inputMode.BindValueChanged( _ => {
			updatePointerType();
		} );
		activeControllers.BindTo( game.ActiveVrControllers );
		activeControllers.BindCollectionChanged( (_, _) => updatePointerType() );
		dominantHand.BindTo( game.DominantHand );
		dominantHand.BindValueChanged( _ => updatePointerType() );

		updatePointerType();
	}

	public event Action<VrController>? ToggleMenuPressed;

	void updatePointerType () {
		setPointer( source.IsEnabled.Value ? inputMode.Value switch {
			InputMode.TouchScreen => touchPointer,
			InputMode.DoublePointer => rayPointer,
			_ => (activeControllers.Count == 1 || Hand == dominantHand.Value) ? rayPointer : null
		} : null );
	}

	void onButtonStateChanged ( bool value, VrAction action ) {
		if ( value ) {
			if ( HoveredCollider is Panel panel ) {
				inputSource.FocusedPanel = panel;
			}
			else return;

			if ( useTouch ) {
				if ( action is VrAction.LeftButton ) {
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
				if ( action is VrAction.LeftButton ) {
					isTouchDown = false;
					inputSource.TouchUp();
				}
			}
			else {
				inputSource.Release( buttonFor( action ) );
			}
		}
	}

	void updateTouchSetting () {
		useTouchBindable.Value = singlePointerTouch.Value 
			|| pointer?.IsTouchSource == true
			|| activeControllers.Count( x => x.HoveredCollider == HoveredCollider ) >= 2;

		IsVisible = pointer?.IsTouchSource != true;
	}

	protected override void Update () {
		base.Update();
		updateTouchSetting();

		if ( pointer is null )
			return;

		if ( aim?.FetchDataForNextFrame() is OpenVR.NET.Input.PoseInput pose ) {
			var maybeHit = pointer.UpdatePointer( pose.Position.ToOsuTk(), pose.Rotation.ToOsuTk() );

			if ( maybeHit is PointerHit hit ) {
				HoveredCollider = hit.Collider;
				if ( hit.Collider is Panel panel ) {
					updateTouchSetting();
					currentPosition = panel.GlobalSpaceContentPositionAt( hit.TrisIndex, hit.Point );

					if ( pointer.IsTouchSource && !isTouchDown )
						onButtonStateChanged( true, VrAction.LeftButton );

					if ( isTouchDown )
						inputSource.TouchMove( currentPosition );
					else if ( !useTouch )
						panel.Content.MoveMouse( currentPosition );
				}
			}
			else {
				if ( pointer.IsTouchSource && isTouchDown )
					onButtonStateChanged( false, VrAction.LeftButton );

				HoveredCollider = null;
			}
		}
	}
}
