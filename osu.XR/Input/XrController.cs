using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.XR;
using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Physics;
using osu.XR.Components;
using osu.XR.Components.Panels;
using osu.XR.Input.Pointers;
using osu.XR.Settings;
using osuTK;
using osuTK.Graphics;
using System;
using System.Linq;
using Valve.VR;
using static osu.Framework.XR.Physics.Raycast;

namespace osu.XR.Input {
	public class XrController : CompositeDrawable3D, IFocusSource {
		public readonly Controller Source;
		public Hand Role => Source.Role switch {
			ETrackedControllerRole.RightHand => Hand.Right,
			ETrackedControllerRole.LeftHand => Hand.Left,
			_ => throw new InvalidOperationException()
		};

		Model ControllerMesh = new();
		RaycastPointer raycast = new() { IsVisible = false };
		TouchPointer touch = new() { IsVisible = false };
		TeleportVisual teleportVisual = new();
		private Pointer pointer { get => pointerBindable.Value; set => pointerBindable.Value = value; }
		private Bindable<Pointer> pointerBindable = new();
		public ControllerMode Mode {
			get => ModeBindable.Value;
			set => ModeBindable.Value = value;
		}
		public readonly Bindable<ControllerMode> ModeBindable = new( ControllerMode.Disabled );
		public readonly BindableBool IsMainControllerBindable = new BindableBool( false );

		public readonly BindableSet<object> HeldObjects = new();
		public bool IsHoldingAnything {
			get => HeldObjects.Any();
		}
		[Resolved]
		private Bindable<IFocusable> globalFocusBindable { get; set; }
		[Resolved]
		private OsuGameXr Game { get; set; }
		public readonly Bindable<bool> SinglePointerTouchBindable = new();
		public readonly Bindable<bool> TapTouchBindable = new();
		public readonly Bindable<bool> DisableTeleportBindable = new();

		public XrController ( Controller controller ) {
			AddInternal( ControllerMesh );
			AddInternal( teleportVisual );
			ControllerMesh.Tint = controller.IsMainController ? Color4.Orange : Color4.LightBlue;
			raycast.Tint = touch.Tint = controller.IsMainController ? Colour4.Orange : Colour4.LightBlue;
			touch.Alpha = 100f / 255f;
			raycast.Source = this;
			touch.Source = this;

			Source = controller;
			ControllerMesh.Mesh = new Mesh();
			_ = controller.Model.LoadAsync(
				begin: () => ControllerMesh.Mesh.IsReady = false,
				finish: () => ControllerMesh.Mesh.IsReady = true,
				addVertice: v => ControllerMesh.Mesh.Vertices.Add( new Vector3( v.X, v.Y, v.Z ) ),
				addTextureCoordinate: uv => ControllerMesh.Mesh.TextureCoordinates.Add( new Vector2( uv.X, uv.Y ) ),
				addTriangle: ( a, b, c ) => ControllerMesh.Mesh.Tris.Add( new IndexedFace( (uint)a, (uint)b, (uint)c ) )
			);

			controller.BindDisabled( () => {
				pointer = null;
			}, true );
			controller.BindEnabled( () => {
				updatePointer();
			}, true );

			ModeBindable.BindValueChanged( v => {
				updatePointer();
			}, true );
			pointerBindable.BindValueChanged( v => {
				if ( v.OldValue is not null ) v.OldValue.IsVisible = false;
				updateVisibility();
				onActivePointerChanged( v.OldValue, v.NewValue );
			} );

			VR.BindComponentsLoaded( () => {
				var scroll = VR.GetControllerComponent<Controller2DVector>( XrAction.Scroll );
				scroll.BindValueUpdatedDetailed( v => {
					if ( !acceptsInputFrom( v.Source ) ) return;

					ScrollBindable.Value += new Vector2( v.NewValue.X, v.NewValue.Y ) * (float)VR.DeltaTime * 30;
				} );

				var mouseLeft = VR.GetControllerComponent<ControllerButton>( XrAction.MouseLeft );
				mouseLeft.BindValueChangedDetailed( v => {
					if ( !acceptsInputFrom( v.Source ) ) return;

					if ( VR.EnabledControllerCount == 1 ) {
						leftButtonBindable.Value = v.NewValue;
					}
					else {
						if ( Source.Role == ETrackedControllerRole.LeftHand != IsMainControllerBindable.Value )
							leftButtonBindable.Value = v.NewValue;
						else
							rightButtonBindable.Value = v.NewValue;
					}
				} );

				var mouseRight = VR.GetControllerComponent<ControllerButton>( XrAction.MouseRight );
				mouseRight.BindValueChangedDetailed( v => {
					if ( !acceptsInputFrom( v.Source ) ) return;

					if ( VR.EnabledControllerCount == 1 ) {
						leftButtonBindable.Value = v.NewValue;
					}
					else {
						if ( Source.Role == ETrackedControllerRole.RightHand == IsMainControllerBindable.Value )
							rightButtonBindable.Value = v.NewValue;
						else
							leftButtonBindable.Value = v.NewValue;
					}
				} );

				var grip = VR.GetControllerComponent<ControllerButton>( XrAction.Grip, Source );
				grip.BindValueChanged( v => {
					if ( v ) {
						Game.GripManager.TryGrip( pointer?.CurrentHit as Drawable3D, this );
					}
					else {
						Game.GripManager.Release( this );
					}
				} );

				haptic = VR.GetControllerComponent<ControllerHaptic>( XrAction.Feedback, Source );
				var teleport = VR.GetControllerComponent<ControllerButton>( XrAction.Move, Source );
				teleport.BindValueChangedDetailed( v => {
					teleportVisual.IsActive.Value = v.NewValue && !DisableTeleportBindable.Value;
					if ( !v.NewValue && !DisableTeleportBindable.Value ) {
						teleportPlayer();
					}
				} );
			} );

			HeldObjects.BindCollectionChanged( () => {
				updateVisibility();
			}, true );

			rightButtonBindable.ValueChanged += v => {
				if ( !EmulatesTouch ) RightButtonBindable.Value = v.NewValue;
			};
			leftButtonBindable.ValueChanged += v => {
				if ( !EmulatesTouch ) LeftButtonBindable.Value = v.NewValue;
				if ( v.NewValue && inspector.Content.IsSelectingBindable.Value ) {
					inspector.Content.Inspect( inspector.Content.SelectedElementBindable.Value );
					inspector.Content.IsSelectingBindable.Value = false;
				}
			};

			// to prevent it being stuck at true after swapping
			IsMainControllerBindable.BindValueChanged( v => {
				leftButtonBindable.Value = false;
				rightButtonBindable.Value = false;
			} );

			LeftButtonBindable.BindValueChanged( v => { if ( v.NewValue ) onAnyInteraction(); } );
			RightButtonBindable.BindValueChanged( v => { if ( v.NewValue ) onAnyInteraction(); } );
			PointerDown += _ => onAnyInteraction();
		}

		private void teleportPlayer () {
			if ( teleportVisual.HasHitGround && !DisableTeleportBindable.Value ) {
				var offset = teleportVisual.HitPosition - ( Game.Player.Position - Game.Player.PositionOffset );
				Game.Player.PositionOffset = new Vector3( offset.X, 0, offset.Z );
			}
		}

		ControllerHaptic haptic;
		public void SendHapticVibration ( double duration, double frequency = 40, double amplitude = 1, double delay = 0 ) {
			haptic?.TriggerVibration( duration, frequency, amplitude, delay );
		}

		public bool IsLoneController => Game.FreeControllers.Count() == 1;
		private bool acceptsInputFrom ( Controller controller )
			=> controller == Source || IsLoneController;

		void updatePointer () {
			if ( Mode == ControllerMode.Disabled ) {
				pointer = null;
			}
			else if ( Mode == ControllerMode.Pointer ) {
				pointer = raycast;
			}
			else if ( Mode == ControllerMode.Touch ) {
				touch.Position = Position;
				pointer = touch;
			}
		}

		void onActivePointerChanged ( Pointer previous, Pointer current ) {
			if ( previous is not null ) {
				previous.FocusChanged -= onPointerFocusChanged;
				previous.NewHit -= onPointerHit;
				previous.NoHit -= onPointerNoHit;
			}
			if ( current is not null ) {
				current.FocusChanged += onPointerFocusChanged;
				current.NewHit += onPointerHit;
				current.NoHit += onPointerNoHit;
			}
			onPointerFocusChanged( new ValueChangedEvent<IHasCollider>( myFocus, current?.CurrentFocus ) );
		}

		private bool isTouchPointerDown;
		public bool EmulatesTouch
			=> Mode == ControllerMode.Touch
			|| Mode == ControllerMode.Pointer && (
				!IsLoneController
				|| SinglePointerTouchBindable.Value
			);

		private bool anyButtonDown => leftButtonBindable.Value || rightButtonBindable.Value;
		private bool canTouch =>
			 Mode == ControllerMode.Pointer && anyButtonDown
			|| Mode == ControllerMode.Touch && ( !TapTouchBindable.Value || anyButtonDown );

		[Resolved]
		private InspectorPanel inspector { get; set; }

		private void onPointerNoHit () {
			if ( isTouchPointerDown ) {
				isTouchPointerDown = false;
				PointerUp?.Invoke();

				onPointerFocusChanged( new ValueChangedEvent<IHasCollider>( myFocus, null ) );
				SendHapticVibration( 0.05, 20 );
			}

			if ( inspector.Content.IsSelectingBindable.Value ) {
				if ( IsMainControllerBindable.Value ) {
					inspector.Content.SelectedElementBindable.Value = null;
				}
			}
		}

		private void onPointerHit ( RaycastHit hit ) {
			if ( inspector.Content.IsSelectingBindable.Value ) {
				if ( IsMainControllerBindable.Value ) {
					if ( inspector.Content.Targets2DDrawables.Value ) {
						var panel = hit.Collider as Panel;
						if ( panel is null ) {
							inspector.Content.Select( null );
						}
						else {
							inspector.Content.Select( panel.EmulatedInput.GetFirstDrawableAt( panel.TexturePositionAt( hit.TrisIndex, hit.Point ) ) );
						}
					}
					else {
						inspector.Content.Select( hit.Collider as Drawable3D );
					}
				}

				return;
			}

			if ( EmulatesTouch && !isTouchPointerDown && canTouch ) {
				onPointerFocusChanged( new ValueChangedEvent<IHasCollider>( myFocus, hit.Collider ) );

				isTouchPointerDown = true;
				PointerDown?.Invoke( hit );
				SendHapticVibration( 0.05, 40 );
			}
			if ( !EmulatesTouch || isTouchPointerDown ) PointerMove?.Invoke( hit );
		}

		IHasCollider myFocus;
		void onPointerFocusChanged ( ValueChangedEvent<IHasCollider> v ) {
			if ( inspector.Content.IsSelectingBindable.Value && v.NewValue is not null ) return;

			if ( myFocus is IFocusable old ) old.OnControllerFocusLost( this );
			myFocus = v.NewValue;
			if ( myFocus is IFocusable @new ) {
				@new.OnControllerFocusGained( this );
			}
		}

		void updateVisibility () {
			if ( Source.IsEnabled ) {
				if ( pointer is not null )
					pointer.IsVisible = Mode != ControllerMode.Disabled && !IsHoldingAnything;
				if ( Mode == ControllerMode.Disabled ) {
					ControllerMesh.IsVisible = true;
				}
				else if ( Mode == ControllerMode.Pointer ) {
					ControllerMesh.IsVisible = true;
				}
				else if ( Mode == ControllerMode.Touch ) {
					ControllerMesh.IsVisible = false;
				}
			}
			else {
				if ( pointer is not null ) pointer.IsVisible = false;
				ControllerMesh.IsVisible = false;
			}
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			Root.Add( touch );
			Root.Add( raycast );

			inspector.Content.IsSelectingBindable.BindValueChanged( v => {
				if ( v.NewValue ) {
					onPointerNoHit();
					if ( myFocus != null ) onPointerFocusChanged( new ValueChangedEvent<IHasCollider>( myFocus, null ) );
				}
				else {
					onPointerFocusChanged( new ValueChangedEvent<IHasCollider>( myFocus, pointer?.CurrentFocus ) );
				}
			}, true );
		}

		protected override void Update () {
			base.Update();
			Position = Game.Player.PositionOffset + new Vector3( Source.Position.X, Source.Position.Y, Source.Position.Z );
			Rotation = new Quaternion( Source.Rotation.X, Source.Rotation.Y, Source.Rotation.Z, Source.Rotation.W );

			if ( isTouchPointerDown && ( !EmulatesTouch || !canTouch ) ) {
				isTouchPointerDown = false;
				PointerUp?.Invoke();
			}

			if ( EmulatesTouch ) {
				LeftButtonBindable.Value = false;
				RightButtonBindable.Value = false;
			}

			teleportVisual.OriginBindable.Value = Position;
			teleportVisual.DirectionBindable.Value = Forward * 5;
		}

		void onAnyInteraction () {
			if ( myFocus is IFocusable focusable ) {
				if ( focusable.CanHaveGlobalFocus ) globalFocusBindable.Value = focusable;
			}
		}
		/// <summary>
		/// When the pointer moves.
		/// </summary>
		public event System.Action<RaycastHit> PointerMove;
		/// <summary>
		/// When a touch pointer touches.
		/// </summary>
		public event System.Action<RaycastHit> PointerDown;
		/// <summary>
		/// When a touch pointer releases.
		/// </summary>
		public event System.Action PointerUp;
		public readonly Bindable<Vector2> ScrollBindable = new();
		private readonly BindableBool leftButtonBindable = new();
		private readonly BindableBool rightButtonBindable = new();
		public readonly BindableBool LeftButtonBindable = new();
		public readonly BindableBool RightButtonBindable = new();
	}

	public enum ControllerMode {
		Disabled,
		Pointer,
		Touch
	}
}
