using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Input;
using osu.Framework.Input.Handlers;
using osu.Framework.Input.Handlers.Keyboard;
using osu.Framework.Input.Handlers.Mouse;
using osu.Framework.Input.StateChanges;
using osu.Framework.Platform;
using osu.XR.Components.Pointers;
using osu.XR.Settings;
using osuTK;
using osuTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using TKKey = osuTK.Input.Key;

namespace osu.XR.Input {
	/// <summary>
	/// XR input is passed to 2D drawables though this manger.
	/// </summary>
	public class XrInputManager : CustomInputManager {
		internal XrMouseHandler mouseHandler;
		internal XrKeyboardHandler keyboardHandler;
		internal XrTouchHandler touchHandler;

		new public bool HasFocus;
		protected override bool HandleHoverEvents => HasFocus;

		/// <summary>
		/// Whether to pass keyboard input to the children.
		/// </summary>
		public Bindable<bool> IsKeyboardActiveBindable => keyboardHandler.IsActiveBindable;

		protected override void LoadComplete () {
			base.LoadComplete();
			AddHandler( mouseHandler = new XrMouseHandler() );
			AddHandler( keyboardHandler = new XrKeyboardHandler() );
			AddHandler( touchHandler = new XrTouchHandler() );
		}

		protected override void Update () {
			base.Update();
			touchHandler.update( Time.Current );
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

		public void TouchDown ( object source, Vector2 position ) {
			touchHandler.touchDown( source, position, Time.Current );
		}
		public void TouchMove ( object source, Vector2 position ) {
			touchHandler.move( source, position, Time.Current );
		}
		public void TouchUp ( object source ) {
			touchHandler.touchUp( source, Time.Current );
		}
		public void ReleaseAllTouch () {
			touchHandler.releaseAll( Time.Current );
		}

		public void PressKey ( TKKey key ) {
			keyboardHandler.HandleKeyDown( key );
			keyboardHandler.HandleKeyUp( key );
		}
		public void HoldKey ( TKKey key ) {
			keyboardHandler.HandleKeyDown( key );
		}
		public void ReleaseKey ( TKKey key ) {
			keyboardHandler.HandleKeyUp( key );
		}

		/// <summary>
		/// A copy of <see cref="MouseHandler"/> overriden to use <see cref="RaycastPointer"/> input.
		/// </summary>
		internal class XrMouseHandler : InputHandler {
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
		internal class XrKeyboardHandler : InputHandler {
			public Bindable<bool> IsActiveBindable = new( false );
			public override bool IsActive => true;
			public override int Priority => 0;

			public override bool Initialize ( GameHost host ) {
				if ( !( host.Window is SDL2DesktopWindow window ) )
					return false;

				Enabled.BindValueChanged( e => {
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
				if ( IsActiveBindable.Value )
					PendingInputs.Enqueue( input );
			}

			private void EnqueueInput ( IInput input ) {
				PendingInputs.Enqueue( input );
			}

			public void HandleKeyDown ( TKKey key ) => EnqueueInput( new KeyboardKeyInput( key, true ) );

			public void HandleKeyUp ( TKKey key ) => EnqueueInput( new KeyboardKeyInput( key, false ) );
			private void handleKeyDown ( TKKey key ) => enqueueInput( new KeyboardKeyInput( key, true ) );

			private void handleKeyUp ( TKKey key ) => enqueueInput( new KeyboardKeyInput( key, false ) );
		}

		internal class XrTouchHandler : InputHandler {
			public override bool Initialize ( GameHost host ) => true;
			public override bool IsActive => true;
			public override int Priority => 0;

			public readonly BindableInt DeadzoneBindable = new( 20 );
			double holdDuration = 500;

			[BackgroundDependencyLoader]
			private void load ( XrConfigManager config ) {
				config.BindWith( XrConfigSetting.Deadzone, DeadzoneBindable );
			}

			private void enqueueInput ( IInput input ) {
				PendingInputs.Enqueue( input );
			}

			Dictionary<object, TouchObject> sources = new();
			internal void touchDown ( object source, Vector2 position, double time ) {
				var touch = new TouchObject { LastUpdateTime = time, StartTime = time, Position = position, StartPosition = position, Index = Enum.GetValues<TouchSource>().Except( sources.Select( x => x.Value.Index ) ).First() };
				sources.Add( source, touch );
				enqueueInput( new TouchInput( touch.Touch, true ) );
			}

			internal void move ( object source, Vector2 position, double time ) {
				if ( !sources.ContainsKey( source ) ) return;

				var touch = sources[ source ];
				touch.Position = position;
				touch.LastUpdateTime = time;
				if ( ( touch.StartPosition - position ).Length > DeadzoneBindable.Value )
					touch.InDeadzone = false;

				if ( !touch.InDeadzone )
					enqueueInput( new TouchInput( touch.Touch, true ) ); // drag
			}

			internal void touchUp ( object source, double time ) {
				if ( !sources.ContainsKey( source ) ) return;

				var touch = sources[ source ];
				sources.Remove( source );
				if ( !touch.RightClick )
					enqueueInput( new TouchInput( touch.Touch, false ) ); // tap if in deadzone
				else {
					touch.Position += new Vector2( 50 );
					enqueueInput( new TouchInput( touch.Touch, true ) );
					touch.Position = touch.StartPosition;
					enqueueInput( new TouchInput( touch.Touch, true ) );
					enqueueInput( new TouchInput( touch.Touch, false ) );

					PendingInputs.Enqueue( new MousePositionAbsoluteInput { Position = touch.StartPosition } );
					PendingInputs.Enqueue( new MouseButtonInput( MouseButton.Right, true ) );
					PendingInputs.Enqueue( new MouseButtonInput( MouseButton.Right, false ) );
				}
			}
			internal void releaseAll ( double time ) {
				foreach ( var i in sources.ToArray() ) {
					touchUp( i.Key, time );
				}
			}

			internal void update ( double time ) {
				//foreach ( var i in sources ) {
				//	var touch = i.Value;
				//	if ( touch.InDeadzone ) {
				//		if ( time - touch.StartTime >= holdDuration ) {
				//			// hold
				//			touch.RightClick = true;
				//		}
				//	}
				//}
				// TODO right click only in menu
			}

			private class TouchObject {
				public Vector2 StartPosition;
				public Vector2 Position;
				public double StartTime;
				public double LastUpdateTime;
				public bool InDeadzone = true;
				public TouchSource Index;
				public bool RightClick;

				public Touch Touch => new Touch( Index, Position );
			}
		}
	}
}
