using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Framework.Input.Handlers;
using osu.Framework.Input.Handlers.Keyboard;
using osu.Framework.Input.Handlers.Mouse;
using osu.Framework.Input.StateChanges;
using osu.Framework.Platform;
using osu.Framework.Utils;
using osu.XR.Components.Pointers;
using osu.XR.Inspector.Components;
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
	public class VirtualInputManager : CustomInputManager {
		internal VirtualMouseHandler mouseHandler;
		internal VirtualKeyboardHandler keyboardHandler;
		internal VirtualTouchHandler touchHandler;

		public Drawable GetFirstDrawableAt ( Vector2 position ) {
			return firstDrawableAt( this, position, ScreenSpaceDrawQuad );
		}

		Drawable firstDrawableAt ( Drawable parent, Vector2 position, Quad mask ) {
			if ( parent is CompositeDrawable composite ) {
				if ( parent is osu.XR.Inspector.IChildrenNotInspectable ) return null;

				foreach ( var i in HierarchyInspectorStep.getAliveInternalChildren( composite ).Reverse() ) {
					if ( !i.IsPresent || i is Component ) 
						continue;
					if ( i.AlwaysPresent && Precision.AlmostEquals( i.Alpha, 0f ) )
						continue;

					var drawable = firstDrawableAt( i, position, ( composite.Masking || composite is BufferedContainer<Drawable> ) ? composite.ScreenSpaceDrawQuad : mask );
					if ( drawable is not null ) return drawable;
				}

				return null;
			}
			else {
				if ( parent is not osu.XR.Inspector.ISelfNotInspectable && parent.ScreenSpaceDrawQuad.Contains( position ) && mask.Contains( position ) ) return parent;
				return null;
			}
		}

		new public bool HasFocus;
		public override bool HandleHoverEvents => HasFocus;

		protected override void LoadComplete () {
			base.LoadComplete();
			AddHandler( mouseHandler = new VirtualMouseHandler() );
			AddHandler( keyboardHandler = new VirtualKeyboardHandler() );
			AddHandler( touchHandler = new VirtualTouchHandler() );
		}

		protected override void Update () {
			base.Update();
			touchHandler.Update( Time.Current );
		}

		private bool isLeftPressed = false;
		public bool IsLeftPressed {
			get => isLeftPressed;
			set {
				if ( isLeftPressed == value ) return;
				isLeftPressed = value;
				if ( isLeftPressed )
					mouseHandler.EmulateMouseDown( MouseButton.Left );
				else
					mouseHandler.EmulateMouseUp( MouseButton.Left );
			}
		}
		private bool isRightPressed = false;
		public bool IsRightPressed {
			get => isRightPressed;
			set {
				if ( isRightPressed == value ) return;
				isRightPressed = value;
				if ( isRightPressed )
					mouseHandler.EmulateMouseDown( MouseButton.Right );
				else
					mouseHandler.EmulateMouseUp( MouseButton.Right );
			}
		}
		private Vector2 scroll;
		public Vector2 Scroll {
			get => scroll;
			set {
				mouseHandler.EmulateMouseWheel( value - scroll, false );
				scroll = value;
			}
		}

		public void TouchDown ( object source, Vector2 position ) {
			touchHandler.EmulateTouchDown( source, position, Time.Current );
		}
		public void TouchMove ( object source, Vector2 position ) {
			touchHandler.EmulateTouchMove( source, position, Time.Current );
		}
		public void TouchUp ( object source ) {
			touchHandler.EmulateTouchUp( source, Time.Current );
		}
		public void ReleaseAllTouch () {
			touchHandler.ReleaseAllSources( Time.Current );
		}

		public void PressKey ( TKKey key ) {
			keyboardHandler.EmulateKeyDown( key );
			keyboardHandler.EmulateKeyUp( key );
		}
		public void HoldKey ( TKKey key ) {
			keyboardHandler.EmulateKeyDown( key );
		}
		public void ReleaseKey ( TKKey key ) {
			keyboardHandler.EmulateKeyUp( key );
		}

		/// <summary>
		/// A copy of <see cref="MouseHandler"/> overriden to use <see cref="RaycastPointer"/> input.
		/// </summary>
		public class VirtualMouseHandler : InputHandler {
			public override bool IsActive => true;

			public override bool Initialize ( GameHost host ) => true;

			private void enqueueInput ( IInput input ) {
				PendingInputs.Enqueue( input );
			}

			public void EmulateMouseMove ( Vector2 position ) => enqueueInput( new MousePositionAbsoluteInput { Position = position } );
			public void EmulateMouseDown ( MouseButton button ) => enqueueInput( new MouseButtonInput( button, true ) );
			public void EmulateMouseUp ( MouseButton button ) => enqueueInput( new MouseButtonInput( button, false ) );
			public void EmulateMouseWheel ( Vector2 delta, bool precise ) => enqueueInput( new MouseScrollRelativeInput { Delta = delta, IsPrecise = precise } );
		}

		/// <summary>
		/// A copy of <see cref="KeyboardHandler"/> which can be disabled.
		/// </summary>
		public class VirtualKeyboardHandler : InputHandler {
			public override bool IsActive => true;

			public override bool Initialize ( GameHost host ) => true;

			private void enqueueInput ( IInput input ) {
				PendingInputs.Enqueue( input );
			}

			public void EmulateKeyDown ( TKKey key ) => enqueueInput( new KeyboardKeyInput( key, true ) );

			public void EmulateKeyUp ( TKKey key ) => enqueueInput( new KeyboardKeyInput( key, false ) );
		}

		public class VirtualTouchHandler : InputHandler {
			public override bool Initialize ( GameHost host ) => true;
			public override bool IsActive => true;

			public readonly BindableInt DeadzoneBindable = new( 20 );
			//double holdDuration = 500;

			[BackgroundDependencyLoader]
			private void load ( XrConfigManager config ) {
				config.BindWith( XrConfigSetting.Deadzone, DeadzoneBindable );
			}

			private void enqueueInput ( IInput input ) {
				PendingInputs.Enqueue( input );
			}

			Dictionary<object, TouchObject> sources = new();
			public void EmulateTouchDown ( object source, Vector2 position, double time ) {
				var touch = new TouchObject { LastUpdateTime = time, StartTime = time, Position = position, StartPosition = position, Index = Enum.GetValues<TouchSource>().Except( sources.Select( x => x.Value.Index ) ).First() };
				sources.Add( source, touch );
				enqueueInput( new TouchInput( touch.Touch, true ) );
			}

			public void EmulateTouchMove ( object source, Vector2 position, double time ) {
				if ( !sources.ContainsKey( source ) ) return;

				var touch = sources[ source ];
				touch.Position = position;
				touch.LastUpdateTime = time;
				if ( ( touch.StartPosition - position ).Length > DeadzoneBindable.Value )
					touch.InDeadzone = false;

				if ( !touch.InDeadzone )
					enqueueInput( new TouchInput( touch.Touch, true ) ); // drag
			}

			public void EmulateTouchUp ( object source, double time ) {
				if ( !sources.ContainsKey( source ) ) return;

				var touch = sources[ source ];
				sources.Remove( source );
				enqueueInput( new TouchInput( touch.Touch, false ) );
				//if ( !touch.RightClick )
				//	enqueueInput( new TouchInput( touch.Touch, false ) ); // tap if in deadzone
				//else {
				//	touch.Position += new Vector2( 50 );
				//	enqueueInput( new TouchInput( touch.Touch, true ) );
				//	touch.Position = touch.StartPosition;
				//	enqueueInput( new TouchInput( touch.Touch, true ) );
				//	enqueueInput( new TouchInput( touch.Touch, false ) );
				//
				//	PendingInputs.Enqueue( new MousePositionAbsoluteInput { Position = touch.StartPosition } );
				//	PendingInputs.Enqueue( new MouseButtonInput( MouseButton.Right, true ) );
				//	PendingInputs.Enqueue( new MouseButtonInput( MouseButton.Right, false ) );
				//}
			}

			public void ReleaseAllSources ( double time ) {
				foreach ( var i in sources.ToArray() ) {
					EmulateTouchUp( i.Key, time );
				}
			}

			public void Update ( double time ) {
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
				//public bool RightClick;

				public Touch Touch => new Touch( Index, Position );
			}
		}
	}
}
