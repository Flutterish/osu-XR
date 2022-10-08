﻿using OpenVR.NET.Devices;
using OpenVR.NET.Input;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Input;
using osu.Framework.XR.Physics;
using osu.Framework.XR.VirtualReality;
using osu.XR.VirtualReality;
using osuTK.Input;

namespace osu.XR.Graphics.VirtualReality;

public class VrController : BasicVrDevice {
	Controller source;
	RayPointer? rayPointer;
	TouchPointer? touchPointer;

	IPointer pointer;

	public IHasCollider? HoveredCollider { get; private set; }
	PoseAction? aim;

	Scene scene;
	public VrController ( Controller source, Scene scene ) : base( source ) {
		this.scene = scene;
		this.source = source;
		pointer = touchPointer ??= new();
		setPointer( pointer );
	}

	void setPointer ( IPointer pointer ) {
		Schedule( () => {
			scene.Remove( (Drawable3D)this.pointer, disposeImmediately: false );
			scene.Add( (Drawable3D)pointer );

			this.pointer.ColliderHovered -= onPointerHover;
			pointer.ColliderHovered += onPointerHover;
			this.pointer = pointer;

			updateTouchSetting();
		} );
	}

	[Resolved]
	IBindableList<VrController> controllers { get; set; } = null!;
	bool isTouchDown;
	BindableBool useTouchBindable = new();
	bool useTouch => useTouchBindable.Value;
	MouseButton buttonFor ( VrAction action ) => action is VrAction.LeftButton ? MouseButton.Left : MouseButton.Right;

	Vector2 currentPosition;
	PanelInteractionSystem.Source inputSource = null!;
	[BackgroundDependencyLoader]
	private void load ( PanelInteractionSystem system ) {
		inputSource = system.GetSource( this );

		source.VR.BindActionsLoaded( () => {
			var left = source.GetAction<BooleanAction>( VrAction.LeftButton )!;
			var right = source.GetAction<BooleanAction>( VrAction.RightButton )!;
			aim = source.GetAction<PoseAction>( VrAction.ControllerTip )!;

			foreach ( var (button, action) in new[] { (left, VrAction.LeftButton), (right, VrAction.RightButton) } ) {
				button.ValueChanged += ( old, @new ) => {
					if ( pointer.IsTouchSource )
						return;

					onButtonStateChanged( @new, action );
				};
			}
		} );
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

	void onPointerHover ( PointerHit? maybeHit ) {
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

	void updateTouchSetting () {
		useTouchBindable.Value = pointer.IsTouchSource
			|| controllers.Count( x => x.HoveredCollider == HoveredCollider ) >= 2;
	}

	protected override void Update () {
		base.Update();
		updateTouchSetting();

		if ( aim?.FetchDataForNextFrame() is PoseInput pose ) {
			pointer.SetTarget( pose.Position.ToOsuTk(), pose.Rotation.ToOsuTk() );
		}
	}
}
