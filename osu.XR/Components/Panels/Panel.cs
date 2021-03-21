using Microsoft.EntityFrameworkCore.Internal;
using OpenVR.NET;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.XR.Components;
using osu.Framework.XR.Maths;
using osu.Framework.XR.Physics;
using osu.Framework.XR.Rendering;
using osu.XR.Input;
using osu.XR.Settings;
using osuTK;
using System.Collections.Generic;
using System.Linq;
using static osu.Framework.XR.Components.Drawable3D.DrawNode3D;
using static osu.Framework.XR.Physics.Raycast;

namespace osu.XR.Components.Panels {
	/// <summary>
	/// A 3D panel that displays an image from a <see cref="BufferedCapture"/>.
	/// </summary>
	public abstract class Panel : Model, IHasCollider, IFocusable {
		public bool CanHaveGlobalFocus { get; set; } = true;
		public PanelInputMode RequestedInputMode { get; set; } = PanelInputMode.Regular;
		public readonly XrInputManager EmulatedInput = new XrInputManager { RelativeSizeAxes = Axes.Both };
		private PlatformActionContainer platformActions = new() { RelativeSizeAxes = Axes.Both };
		public Container Source => platformActions;
		/// <summary>
		/// Non-stretching scaling applied to the content
		/// </summary>
		public Bindable<Vector2> ContentScale = new( Vector2.One );
		public BufferedCapture SourceCapture { get; } = new BufferedCapture { RelativeSizeAxes = Axes.Both };
		protected bool IsMeshInvalidated = true;

		private bool hasFocus;
		new public bool HasFocus {
			get => hasFocus;
			set {
				if ( hasFocus == value ) return;
				hasFocus = value;
				if ( !hasFocus ) {
					EmulatedInput.IsLeftPressed = false;
					EmulatedInput.IsRightPressed = false;
					EmulatedInput.ReleaseAllTouch();
				}
				EmulatedInput.HasFocus = value;
			}
		}

		public Panel AutosizeX () {
			EmulatedInput.RelativeSizeAxes = Axes.Y;
			SourceCapture.RelativeSizeAxes = Axes.Y;
			platformActions.RelativeSizeAxes = Axes.Y;

			EmulatedInput.AutoSizeAxes = Axes.X;
			SourceCapture.AutoSizeAxes = Axes.X;
			platformActions.AutoSizeAxes = Axes.X;

			return this;
		}
		public Panel AutosizeY () {
			EmulatedInput.RelativeSizeAxes = Axes.X;
			SourceCapture.RelativeSizeAxes = Axes.X;
			platformActions.RelativeSizeAxes = Axes.X;

			EmulatedInput.AutoSizeAxes = Axes.Y;
			SourceCapture.AutoSizeAxes = Axes.Y;
			platformActions.AutoSizeAxes = Axes.Y;

			return this;
		}
		public Panel AutosizeBoth () {
			EmulatedInput.RelativeSizeAxes = Axes.None;
			SourceCapture.RelativeSizeAxes = Axes.None;
			platformActions.RelativeSizeAxes = Axes.None;

			EmulatedInput.AutoSizeAxes = Axes.Both;
			SourceCapture.AutoSizeAxes = Axes.Both;
			platformActions.AutoSizeAxes = Axes.Both;

			return this;
		}

		public Panel () {
			UseGammaCorrection = true;

			ContentScale.ValueChanged += v => {
				SourceCapture.Size = v.NewValue;
				IsMeshInvalidated = true;
			};

			SourceCapture.Add( EmulatedInput );
			EmulatedInput.Add( platformActions );
			AddDrawable( SourceCapture );
		}

		[BackgroundDependencyLoader]
		private void load ( XrConfigManager config ) {
			config.BindWith( XrConfigSetting.Deadzone, deadzoneBindable );
		}
		BindableInt deadzoneBindable = new( 20 );

		bool inDeadzone = false;
		Vector2 deadzoneCenter;
		Vector2 pointerPosition;

		void handleButton ( bool isLeft, bool isDown ) {
			if ( VR.EnabledControllerCount > 1 ) {
				if ( RequestedInputMode == PanelInputMode.Regular == isLeft ) EmulatedInput.IsLeftPressed = isDown;
				else if ( RequestedInputMode == PanelInputMode.Inverted == isLeft ) EmulatedInput.IsRightPressed = isDown;
			}
			else EmulatedInput.IsLeftPressed = isDown;

			if ( isDown ) {
				inDeadzone = true;
				deadzoneCenter = pointerPosition;
			}
			else inDeadzone = false;
		}

		protected abstract void RecalculateMesh ();

		/// <summary>
		/// The texture position from top left.
		/// </summary>
		public Vector2 TexturePositionAt ( int trisIndex, Vector3 position ) {
			var face = Faces[ trisIndex ];
			var barycentric = Triangles.Barycentric( face, position );
			var tris = Mesh.Tris[ trisIndex ];
			var textureCoord =
				  Mesh.TextureCoordinates[ (int)tris.A ] * barycentric.X
				+ Mesh.TextureCoordinates[ (int)tris.B ] * barycentric.Y
				+ Mesh.TextureCoordinates[ (int)tris.C ] * barycentric.Z;
			return new Vector2( MainTexture.Width * textureCoord.X, MainTexture.Height * ( 1 - textureCoord.Y ) );
		}

		private Vector2 lastTextureSize;
		public override void BeforeDraw ( DrawSettings settings ) {
			if ( SourceCapture is null ) return;
			if ( SourceCapture.Capture is null ) return;
			MainTexture = SourceCapture.Capture;
			if ( MainTexture.Size != lastTextureSize ) {
				IsMeshInvalidated = true;
				lastTextureSize = MainTexture.Size;
			}
		}

		protected override void Update () {
			base.Update();
			if ( IsMeshInvalidated ) {
				RecalculateMesh();
			}
		}

		public virtual bool IsColliderEnabled => Source.Any( x => x.IsPresent );
		public override void Show () {
			foreach ( var i in Source ) {
				i.Show();
			}
		}
		public override void Hide () {
			foreach ( var i in Source ) {
				i.Hide();
			}
		}

		List<XrController> focusedControllers = new();
		IEnumerable<Controller> focusedControllerSources => focusedControllers.Select( x => x.Source );
		Dictionary<XrController, System.Action> eventUnsubs = new();
		public void OnControllerFocusGained ( IFocusSource focus ) {
			if ( focus is not XrController controller ) return;

			System.Action<ValueChangedEvent<Vector2>> onScroll = v => { EmulatedInput.Scroll += v.NewValue - v.OldValue; };
			System.Action<ValueChangedEvent<bool>> onLeft = v => { handleButton( isLeft: true, isDown: v.NewValue ); };
			System.Action<ValueChangedEvent<bool>> onRight = v => { handleButton( isLeft: false, isDown: v.NewValue ); };
			System.Action<RaycastHit> onMove = hit => { onPointerMove( controller, hit ); };
			System.Action<RaycastHit> onDown = hit => { onTouchDown( controller, hit ); };
			System.Action onUp = () => { onTouchUp( controller ); };
			controller.PointerMove += onMove;
			controller.PointerDown += onDown;
			controller.PointerUp += onUp;
			controller.ScrollBindable.ValueChanged += onScroll;
			controller.LeftButtonBindable.ValueChanged += onLeft;
			controller.RightButtonBindable.ValueChanged += onRight;
			eventUnsubs.Add( controller, () => {
				controller.PointerMove -= onMove;
				controller.PointerDown -= onDown;
				controller.PointerUp -= onUp;
				controller.ScrollBindable.ValueChanged -= onScroll;
				controller.LeftButtonBindable.ValueChanged -= onLeft;
				controller.RightButtonBindable.ValueChanged -= onRight;
			} );
			focusedControllers.Add( controller );

			updateFocus();
		}
		public void OnControllerFocusLost ( IFocusSource focus ) {
			if ( focus is not XrController controller ) return;

			eventUnsubs[ controller ].Invoke();
			eventUnsubs.Remove( controller );
			focusedControllers.Remove( controller );

			updateFocus();
		}
		void updateFocus () {
			HasFocus = focusedControllers.Any();
		}

		private void onPointerMove ( XrController controller, RaycastHit hit ) {
			if ( hit.Collider != this ) return;

			var position = TexturePositionAt( hit.TrisIndex, hit.Point );
			if ( controller.EmulatesTouch ) {
				EmulatedInput.TouchMove( controller, position );
			}
			else {
				pointerPosition = position;
				if ( ( pointerPosition - deadzoneCenter ).Length > deadzoneBindable.Value ) inDeadzone = false;
				if ( !inDeadzone ) EmulatedInput.mouseHandler.EmulateMouseMove( position );
			}
		}

		private void onTouchDown ( XrController controller, RaycastHit hit ) {
			if ( hit.Collider != this ) return;

			var position = TexturePositionAt( hit.TrisIndex, hit.Point );
			EmulatedInput.TouchDown( controller, position );
		}

		private void onTouchUp ( XrController controller ) {
			EmulatedInput.TouchUp( controller );
		}
	}

	public enum PanelInputMode {
		Regular,
		Inverted
	}
}
