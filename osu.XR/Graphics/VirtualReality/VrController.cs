using OpenVR.NET.Devices;
using OpenVR.NET.Input;
using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Graphics.Rendering;
using osu.Framework.XR.Input;
using osu.Framework.XR.Physics;
using osu.Framework.XR.VirtualReality;
using osu.XR.VirtualReality;
using osuTK.Input;
using static osu.Framework.XR.Physics.Raycast;

namespace osu.XR.Graphics.VirtualReality;

public class VrController : BasicVrDevice {
	Controller source;
	RayPointer pointer;

	public IHasCollider? HoveredCollider { get; private set; }
	PoseAction? aim;

	public VrController ( Controller source, Scene scene ) : base( source ) {
		this.source = source;
		scene.Add( pointer = new( scene ) );
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

		pointer.ColliderHovered += hit => {
			HoveredCollider = hit.Collider;
			if ( hit.Collider is Panel panel ) {
				updateTouchSetting();
				currentPosition = panel.GlobalSpaceContentPositionAt( hit.TrisIndex, hit.Point );

				if ( isTouchDown )
					inputSource.TouchMove( currentPosition );
				else if ( !useTouch )
					panel.Content.MoveMouse( currentPosition );
			}
			ColliderHovered?.Invoke( hit );
		};
		pointer.NothingHovered += () => HoveredCollider = null;
		source.VR.BindActionsLoaded( () => {
			var left = source.GetAction<BooleanAction>( VrAction.LeftButton )!;
			var right = source.GetAction<BooleanAction>( VrAction.RightButton )!;
			aim = source.GetAction<PoseAction>( VrAction.ControllerTip )!;

			foreach ( var (button, action) in new[] { (left, VrAction.LeftButton), (right, VrAction.RightButton) } ) {
				button.ValueChanged += ( old, @new ) => {
					if ( @new ) {
						if ( HoveredCollider is Panel panel ) {
							inputSource.FocusedPanel = panel;
						}
						else return;

						if ( useTouch ) {
							isTouchDown = true;
							inputSource.TouchDown( currentPosition );
						}
						else {
							inputSource.MoveMouse( currentPosition );
							inputSource.Press( buttonFor( action ) );
						}
					}
					else {
						if ( isTouchDown ) {
							isTouchDown = false;
							inputSource.TouchUp();
						}
						else {
							inputSource.Release( buttonFor( action ) );
						}
					}
				};
			}
		} );
	}

	void updateTouchSetting () {
		useTouchBindable.Value = controllers.Count( x => x.HoveredCollider == HoveredCollider ) >= 2;
	}

	protected override void Update () {
		base.Update();
		updateTouchSetting();

		if ( aim?.FetchDataForNextFrame() is PoseInput pose ) {
			pointer.Position = pose.Position.ToOsuTk();
			pointer.Rotation = pose.Rotation.ToOsuTk();
		}
	}

	public delegate void ColliderHoveredHandler ( RaycastHit hit );
	public event ColliderHoveredHandler? ColliderHovered;
}
