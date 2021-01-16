using Humanizer;
using osu.Framework.Bindables;
using osu.Framework.Input;
using osu.Framework.Input.Handlers;
using osu.Framework.Input.Handlers.Keyboard;
using osu.Framework.Input.Handlers.Mouse;
using osu.Framework.Input.StateChanges;
using osu.Framework.Input.StateChanges.Events;
using osu.Framework.Input.States;
using osu.Framework.Platform;
using osu.XR.Components;
using osu.XR.Maths;
using osuTK;
using osuTK.Input;
using System;
using System.Collections.Generic;
using System.Text;
using static osu.XR.Physics.Raycast;
using TKKey = osuTK.Input.Key;

namespace osu.XR {
	/// <summary>
	/// XR input is passed to 2D drawables though this manger.
	/// </summary>
	public class XrInputManager : CustomInputManager {
		private Pointer pointer;
		XrMouseHandler mouseHandler;
		XrKeyboardHandler keyboardHandler;
		/// <summary>
		/// Whether to pass keyboard input to the children.
		/// </summary>
		public Bindable<bool> IsKeyboardActiveBindable => keyboardHandler.IsActiveBindable;
		/// <param name="pointer">The pointer to guide the cursor with.</param>
		public XrInputManager ( Pointer pointer ) {
			this.pointer = pointer;
			pointer.OnUpdate += pointerUpdate;
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			AddHandler( mouseHandler = new XrMouseHandler() );
			AddHandler( keyboardHandler = new XrKeyboardHandler() );
		}

		Vector2 mousePos;
		private void pointerUpdate ( osuTK.Vector3 position, MeshedXrObject mesh, RaycastHit hit ) {
			var face = mesh.Faces[ hit.TrisIndex ];
			var barycentric = Triangles.Barycentric( face, position );
			var tris = mesh.Mesh.Tris[ hit.TrisIndex ];
			var textureCoord = 
				  mesh.Mesh.TextureCoordinates[ (int)tris.A ] * barycentric.X
				+ mesh.Mesh.TextureCoordinates[ (int)tris.B ] * barycentric.Y
				+ mesh.Mesh.TextureCoordinates[ (int)tris.C ] * barycentric.Z;
			mousePos = new Vector2( DrawWidth * textureCoord.X, DrawHeight * ( 1 - textureCoord.Y ) );
			mouseHandler.handleMouseMove( mousePos );
		}

		/// <summary>
		/// A copy of <see cref="MouseHandler"/> overriden to use <see cref="Pointer"/> input.
		/// </summary>
		private class XrMouseHandler : InputHandler {
			public override bool IsActive => true;
			public override int Priority => 0;

			public override bool Initialize ( GameHost host ) {
				if ( !( host.Window is SDL2DesktopWindow window ) )
					return false;

				Enabled.BindValueChanged( e =>
				{
					if ( e.NewValue ) {
						window.MouseDown += handleMouseDown;
						window.MouseUp += handleMouseUp;
						window.MouseWheel += handleMouseWheel;
					}
					else {
						window.MouseDown -= handleMouseDown;
						window.MouseUp -= handleMouseUp;
						window.MouseWheel -= handleMouseWheel;
					}
				}, true );

				return true;
			}

			private void enqueueInput ( IInput input ) {
				PendingInputs.Enqueue( input );
			}

			internal void handleMouseMove ( Vector2 position ) => enqueueInput( new MousePositionAbsoluteInput { Position = position } );
			private void handleMouseDown ( MouseButton button ) => enqueueInput( new MouseButtonInput( button, true ) );
			private void handleMouseUp ( MouseButton button ) => enqueueInput( new MouseButtonInput( button, false ) );
			private void handleMouseWheel ( Vector2 delta, bool precise ) => enqueueInput( new MouseScrollRelativeInput { Delta = delta, IsPrecise = precise } );
		}

		/// <summary>
		/// A copy of <see cref="KeyboardHandler"/> which can be disabled.
		/// </summary>
		private class XrKeyboardHandler : InputHandler {
			public Bindable<bool> IsActiveBindable = new( false );
			public override bool IsActive => IsActiveBindable.Value;
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
				if ( IsActiveBindable.Value ) 
					PendingInputs.Enqueue( input );
			}

			private void handleKeyDown ( TKKey key ) => enqueueInput( new KeyboardKeyInput( key, true ) );

			private void handleKeyUp ( TKKey key ) => enqueueInput( new KeyboardKeyInput( key, false ) );
		}
	}
}
