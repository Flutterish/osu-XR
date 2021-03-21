using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.XR;
using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Physics;
using osu.XR.Components.Panels;
using osu.XR.Components.Pointers;
using osu.XR.Settings;
using osuTK;
using osuTK.Graphics;
using System.Linq;
using static osu.Framework.XR.Physics.Raycast;

namespace osu.XR.Components {
	public class XrController : CompositeDrawable3D, IFocusSource {
		public readonly Controller Source;

		Model ControllerMesh = new();
		RaycastPointer raycast = new() { IsVisible = false };
		TouchPointer touch = new() { IsVisible = false };
		private Pointer pointer { get => pointerBindable.Value; set => pointerBindable.Value = value; }
		private Bindable<Pointer> pointerBindable = new();
		public ControllerMode Mode {
			get {
				return ModeOverrideBindable.Value == ControllerMode.Disabled
					? ModeBindable.Value
					: ModeOverrideBindable.Value;
			}
			set {
				ModeBindable.Value = value;
			}
		}
		public readonly Bindable<ControllerMode> ModeBindable = new( ControllerMode.Disabled );
		public readonly Bindable<ControllerMode> ModeOverrideBindable = new( ControllerMode.Disabled );

		public readonly BindableSet<object> HeldObjects = new();
		public bool IsHoldingAnything {
			get => HeldObjects.Any();
		}
		[Resolved]
		private Bindable<IFocusable> globalFocusBindable { get; set; }
		public readonly Bindable<bool> SinglePointerTouchBindable = new();
		public readonly Bindable<bool> TapTouchBindable = new();

		public XrController ( Controller controller ) {
			Add( ControllerMesh );
			ControllerMesh.MainTexture = Textures.Pixel( controller.IsMainController ? Color4.Orange : Color4.LightBlue ).TextureGL;
			touch.MainTexture = raycast.MainTexture = Textures.Pixel( (controller.IsMainController ? Colour4.Orange : Colour4.LightBlue ).MultiplyAlpha( 100f / 255f ) ).TextureGL;
			raycast.Source = this;
			touch.Source = this;

			Source = controller;
			ControllerMesh.Mesh = new Mesh();
			_ = controller.Model.LoadAsync(
				begin: () => ControllerMesh.Mesh.IsReady = false,
				finish: () => ControllerMesh.Mesh.IsReady = true,
				addVertice: v => ControllerMesh.Mesh.Vertices.Add( new osuTK.Vector3( v.X, v.Y, v.Z ) ),
				addTextureCoordinate: uv => ControllerMesh.Mesh.TextureCoordinates.Add( new osuTK.Vector2( uv.X, uv.Y ) ),
				addTriangle: (a,b,c) => ControllerMesh.Mesh.Tris.Add( new IndexedFace( (uint)a, (uint)b, (uint)c ) )
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
			ModeOverrideBindable.BindValueChanged( v => {
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

					ScrollBindable.Value += new Vector2( v.NewValue.X, v.NewValue.Y ) * (float)VR.DeltaTime * 80;
				} );

				var mouseLeft = VR.GetControllerComponent<ControllerButton>( XrAction.MouseLeft );
				mouseLeft.BindValueChangedDetailed( v => {
					if ( !acceptsInputFrom( v.Source ) ) return;

					else LeftButtonBindable.Value = v.NewValue;
				} );

				var mouseRight = VR.GetControllerComponent<ControllerButton>( XrAction.MouseRight );
				mouseRight.BindValueChangedDetailed( v => {
					if ( !acceptsInputFrom( v.Source ) ) return;

					else RightButtonBindable.Value = v.NewValue;
				} );

				haptic = VR.GetControllerComponent<ControllerHaptic>( XrAction.Feedback, Source );
			} );

			HeldObjects.BindCollectionChanged( () => {
				updateVisibility();
			}, true );
		}
		ControllerHaptic haptic;
		public void SendHapticVibration ( double duration, double frequency = 40, double amplitude = 1, double delay = 0 ) {
			haptic?.TriggerVibration( duration, frequency, amplitude, delay );
		}

		public bool IsSoloMode;
		public bool IsLoneController => IsSoloMode || VR.EnabledControllerCount == 1;
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
			onPointerFocusChanged( new ValueChangedEvent<IHasCollider>(myFocus, current?.CurrentFocus) );
		}

		private bool isTouchPointerDown;
		public bool EmulatesTouch
			=> Mode == ControllerMode.Touch
			|| ( Mode == ControllerMode.Pointer && (
				!IsLoneController
				|| SinglePointerTouchBindable.Value
			) );

		private bool anyButtonDown => LeftButtonBindable.Value || RightButtonBindable.Value;
		private bool canTouch => !TapTouchBindable.Value || anyButtonDown;
		private void onPointerNoHit () {
			if ( isTouchPointerDown ) {
				isTouchPointerDown = false;
				PointerUp?.Invoke();

				onPointerFocusChanged( new ValueChangedEvent<IHasCollider>( myFocus, null ) );
				SendHapticVibration( 0.05, 20 );
			}
		}

		private void onPointerHit ( RaycastHit hit ) {
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
			if ( myFocus is IFocusable old ) old.OnControllerFocusLost( this );
			myFocus = v.NewValue;
			if ( myFocus is IFocusable @new ) {
				@new.OnControllerFocusGained( this );
				if ( @new.CanHaveGlobalFocus ) globalFocusBindable.Value = @new;
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
		}

		protected override void Update () {
			base.Update();
			Position = new osuTK.Vector3( Source.Position.X, Source.Position.Y, Source.Position.Z );
			Rotation = new osuTK.Quaternion( Source.Rotation.X, Source.Rotation.Y, Source.Rotation.Z, Source.Rotation.W );

			if ( isTouchPointerDown && ( !EmulatesTouch || !canTouch ) ) {
				isTouchPointerDown = false;
				PointerUp?.Invoke();
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
		public readonly BindableBool LeftButtonBindable = new();
		public readonly BindableBool RightButtonBindable = new();
	}

	public enum ControllerMode {
		Disabled,
		Pointer,
		Touch
	}
}
