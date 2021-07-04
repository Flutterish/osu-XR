using OpenVR.NET;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.XR.Settings;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static osu.Framework.XR.Physics.Raycast;

namespace osu.XR.Components.Panels {
	public abstract class InteractivePanel : Panel, IFocusable {
		public bool CanHaveGlobalFocus { get; init; } = true;
		public PanelInputMode RequestedInputMode { get; set; } = PanelInputMode.Regular;

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
