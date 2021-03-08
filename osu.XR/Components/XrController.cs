using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.XR.Graphics;
using osu.Game.Beatmaps;
using osu.XR.Components.Panels;
using osu.XR.Components.Pointers;
using osu.XR.Drawables;
using osu.XR.Physics;
using osu.XR.Settings;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static osu.XR.Physics.Raycast;

namespace osu.XR.Components {
	public class XrController : MeshedXrObject {
		public readonly Controller Source;

		RaycastPointer raycast = new() { IsVisible = false };
		TouchPointer touch = new() { IsVisible = false };
		private Pointer pointer { get => pointerBindable.Value; set => pointerBindable.Value = value; }
		private Bindable<Pointer> pointerBindable = new();
		public ControllerMode Mode { get => ModeBindable.Value; set => ModeBindable.Value = value; }
		public readonly Bindable<ControllerMode> ModeBindable = new( ControllerMode.Disabled );
		public bool IsHoldingAnything {
			get => IsHoldingBindable.Value;
			set => IsHoldingBindable.Value = value;
		}
		public readonly BindableBool IsHoldingBindable = new();
		[Resolved( name: "FocusedPanel" )]
		private Bindable<Panel> focusedPanel { get; set; }

		public XrController ( Controller controller ) {
			MainTexture = Textures.Pixel( controller.IsMainController ? Color4.Orange : Color4.LightBlue ).TextureGL;
			touch.MainTexture = raycast.MainTexture = Textures.Pixel( (controller.IsMainController ? Colour4.Orange : Colour4.LightBlue ).MultiplyAlpha( 100f / 255f ) ).TextureGL;
			raycast.Source = this;
			touch.Source = this;

			Source = controller;
			Mesh = new Mesh();
			_ = controller.LoadModelAsync(
				begin: () => Mesh.IsReady = false,
				finish: () => Mesh.IsReady = true,
				addVertice: v => Mesh.Vertices.Add( new osuTK.Vector3( v.X, v.Y, v.Z ) ),
				addTextureCoordinate: uv => Mesh.TextureCoordinates.Add( new osuTK.Vector2( uv.X, uv.Y ) ),
				addTriangle: (a,b,c) => Mesh.Tris.Add( new IndexedFace( (uint)a, (uint)b, (uint)c ) )
			);

			controller.BindDisabled( () => {
				pointer = null;
			}, true );

			controller.BindEnabled( () => {
				setPointer();
			}, true );

			ModeBindable.BindValueChanged( v => {
				setPointer();
			}, true );
			IsHoldingBindable.BindValueChanged( _ => updateVisibility() );
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

					if ( EmulatesTouch ) forceTouch = v.NewValue;
					else LeftButtonBindable.Value = v.NewValue;
				} );

				var mouseRight = VR.GetControllerComponent<ControllerButton>( XrAction.MouseRight );
				mouseRight.BindValueChangedDetailed( v => {
					if ( !acceptsInputFrom( v.Source ) ) return;

					if ( EmulatesTouch ) forceTouch = v.NewValue;
					else RightButtonBindable.Value = v.NewValue;
				} );
			} );
		}
		private bool acceptsInputFrom ( Controller controller )
			=> controller == Source || ( inputModeBindable.Value == InputMode.SinglePointer && Mode == ControllerMode.Pointer );

		void setPointer () {
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

		public bool EmulatesTouch
			=> Mode == ControllerMode.Touch
			|| inputModeBindable.Value == InputMode.DoublePointer
			|| ( inputModeBindable.Value == InputMode.SinglePointer && singlePointerTouchBindable.Value );

		private bool isTouchPointerDown;
		private bool forceTouch;
		private bool canTouch => ( Mode == ControllerMode.Touch && !tapTouchBindable.Value ) || forceTouch;
		private void onPointerNoHit () {
			if ( isTouchPointerDown ) {
				isTouchPointerDown = false;
				PointerUp?.Invoke();

				onPointerFocusChanged( new ValueChangedEvent<IHasCollider>( myFocus, null ) );
			}
		}

		private void onPointerHit ( RaycastHit hit ) {
			if ( !isTouchPointerDown && EmulatesTouch && canTouch ) {
				onPointerFocusChanged( new ValueChangedEvent<IHasCollider>( myFocus, hit.Collider ) );

				isTouchPointerDown = true;
				PointerDown?.Invoke( hit );
			}
			if ( !EmulatesTouch || isTouchPointerDown ) PointerMove?.Invoke( hit );
		}

		IHasCollider myFocus;
		void onPointerFocusChanged ( ValueChangedEvent<IHasCollider> v ) {
			if ( myFocus is IReactsToController old ) old.OnControllerFocusLost( this );
			myFocus = v.NewValue;
			if ( myFocus is IReactsToController @new ) @new.OnControllerFocusGained( this );
			if ( myFocus is Panel panel && panel.CanHaveGlobalFocus ) focusedPanel.Value = panel;
		}

		void updateVisibility () {
			if ( Source.IsEnabled ) {
				if ( pointer is not null )
					pointer.IsVisible = Mode != ControllerMode.Disabled && !IsHoldingAnything;
				if ( Mode == ControllerMode.Disabled ) {
					IsVisible = true;
				}
				else if ( Mode == ControllerMode.Pointer ) {
					IsVisible = true;
				}
				else if ( Mode == ControllerMode.Touch ) {
					IsVisible = false;
				}
			}
			else {
				if ( pointer is not null ) pointer.IsVisible = false;
				IsVisible = false;
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

		Bindable<InputMode> inputModeBindable = new();
		Bindable<bool> singlePointerTouchBindable = new();
		Bindable<bool> tapTouchBindable = new();
		[BackgroundDependencyLoader]
		private void load ( XrConfigManager config ) {
			config.BindWith( XrConfigSetting.InputMode, inputModeBindable );
			config.BindWith( XrConfigSetting.SinglePointerTouch, singlePointerTouchBindable );
			config.BindWith( XrConfigSetting.TapOnPress, tapTouchBindable );
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

	public interface IReactsToController {
		void OnControllerFocusGained ( XrController controller );
		void OnControllerFocusLost ( XrController controller );
	}

	public enum ControllerMode {
		Disabled,
		Pointer,
		Touch
	}
}
