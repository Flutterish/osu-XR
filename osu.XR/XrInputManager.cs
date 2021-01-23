using osu.Framework.Bindables;
using osu.Framework.Input;
using osu.Framework.Input.Handlers;
using osu.Framework.Input.Handlers.Keyboard;
using osu.Framework.Input.Handlers.Mouse;
using osu.Framework.Input.StateChanges;
using osu.Framework.Platform;
using osu.XR.Components;
using osu.XR.Maths;
using osuTK;
using osuTK.Input;
using System.Security.Cryptography.Pkcs;
using static osu.XR.Physics.Raycast;
using TKKey = osuTK.Input.Key;

namespace osu.XR {
	/// <summary>
	/// XR input is passed to 2D drawables though this manger.
	/// </summary>
	public class XrInputManager : CustomInputManager {
		Pointer pointer;
		public Pointer Pointer {
			get => pointer;
			set {
				if ( pointer is not null ) {
					pointer.NewHit -= onPointerUpdate;
				}
				pointer = value;
				if ( pointer is not null ) {
					pointer.NewHit += onPointerUpdate;
				}
			}
		}
		public Panel InputPanel;
		XrMouseHandler mouseHandler;
		XrKeyboardHandler keyboardHandler;
		/// <summary>
		/// Whether to pass keyboard input to the children.
		/// </summary>
		public Bindable<bool> IsKeyboardActiveBindable => keyboardHandler.IsActiveBindable;
		/// <param name="pointer">The pointer to guide the cursor with.</param>

		protected override void LoadComplete () {
			base.LoadComplete();
			AddHandler( mouseHandler = new XrMouseHandler() );
			AddHandler( keyboardHandler = new XrKeyboardHandler() );
		}

		private void onPointerUpdate ( RaycastHit hit ) {
			var mesh = hit.Collider;
			if ( mesh != InputPanel ) return;

			// BUG this makes the cursor jitter for a frame each time
			mouseHandler.handleMouseMove( InputPanel.TexturePositionAt( hit.TrisIndex, hit.Point ).ScaledBy( new Vector2( DrawWidth / InputPanel.MainTexture.Width, DrawHeight / InputPanel.MainTexture.Height ) ) );
		}

		private bool isLeftPressed = false;
		public bool IsLeftPressed {
			get => isLeftPressed;
			set {
				if ( isLeftPressed == value ) return;
				isLeftPressed = value;
				if ( isLeftPressed )
					mouseHandler.handleMouseDown( MouseButton.Left );
				else
					mouseHandler.handleMouseUp( MouseButton.Left );
			}
		}
		private bool isRightPressed = false;
		public bool IsRightPressed {
			get => isRightPressed;
			set {
				if ( isRightPressed == value ) return;
				isRightPressed = value;
				if ( isRightPressed )
					mouseHandler.handleMouseDown( MouseButton.Right );
				else
					mouseHandler.handleMouseUp( MouseButton.Right );
			}
		}
		private Vector2 scroll;
		public Vector2 Scroll {
			get => scroll;
			set {
				mouseHandler.handleMouseWheel( value - scroll, false );
				scroll = value;
			}
		}

		/// <summary>
		/// A copy of <see cref="MouseHandler"/> overriden to use <see cref="Pointer"/> input.
		/// </summary>
		private class XrMouseHandler : InputHandler {
			public override bool IsActive => true;
			public override int Priority => 0;

			public override bool Initialize ( GameHost host ) => true;

			private void enqueueInput ( IInput input ) {
				PendingInputs.Enqueue( input );
			}

			internal void handleMouseMove ( Vector2 position ) => enqueueInput( new MousePositionAbsoluteInput { Position = position } );
			internal void handleMouseDown ( MouseButton button ) => enqueueInput( new MouseButtonInput( button, true ) );
			internal void handleMouseUp ( MouseButton button ) => enqueueInput( new MouseButtonInput( button, false ) );
			internal void handleMouseWheel ( Vector2 delta, bool precise ) => enqueueInput( new MouseScrollRelativeInput { Delta = delta, IsPrecise = precise } );
		}

		/// <summary>
		/// A copy of <see cref="KeyboardHandler"/> which can be disabled.
		/// </summary>
		private class XrKeyboardHandler : InputHandler {
			public Bindable<bool> IsActiveBindable = new( false );
			public override bool IsActive => true;
			public override int Priority => 0;

			public override bool Initialize ( GameHost host ) {
				if ( !( host.Window is SDL2DesktopWindow window ) )
					return false;

				Enabled.BindValueChanged( e =>
				{
					if ( e.NewValue ) {
						window.KeyDown += handleKeyDown;
						window.KeyUp += handleKeyUp;
					}
					else {
						window.KeyDown -= handleKeyDown;
						window.KeyUp -= handleKeyUp;
					}
				}, true );

				return true;
			}

			private void enqueueInput ( IInput input ) {
				if ( IsActiveBindable.Value ) // ISSUE for whatever reason, even though this is disabled, textfields still get inputs while focused. Probably the game hosts proper text input. however, a better solution is to take away focus.
					PendingInputs.Enqueue( input );
			}

			private void handleKeyDown ( TKKey key ) => enqueueInput( new KeyboardKeyInput( key, true ) );

			private void handleKeyUp ( TKKey key ) => enqueueInput( new KeyboardKeyInput( key, false ) );
		}
	}
}
