using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.States;
using osu.Framework.IO.Stores;
using osu.Game;
using osu.Game.Graphics.Cursor;
using osu.XR.Components;
using osu.XR.Physics;
using osu.XR.Projection;
using osu.XR.Rendering;
using osuTK;
using osuTK.Input;
using System;
using System.Linq;
using System.Reflection;
using Pointer = osu.XR.Components.Pointer;

namespace osu.XR {
	/// <summary>
	/// The full osu! experience in VR.
	/// </summary>
	internal class OsuGameXr : XrGame {
        [Cached]
        public readonly PhysicsSystem PhysicsSystem = new();
        internal InputManager _inputManager;
        private InputManager inputManager => _inputManager ??= GetContainingInputManager();
        [Cached]
        public readonly Camera Camera = new() { Position = new Vector3( 0, 0, 0 ) };
        XrScene scene = new XrScene { RelativeSizeAxes = Axes.Both };
        public Panel OsuPanel;
        [Cached]
        public readonly Pointer Pointer = new Pointer();
        [Cached(typeof(Framework.Game))]
        OsuGame OsuGame;

        public OsuGameXr ( string[] args ) { OsuGame = new OsuGame( args ) { RelativeSizeAxes = Axes.Both }; }

        protected override void LoadComplete () {
            base.LoadComplete();
            Resources.AddStore( new DllResourceStore( typeof( OsuGameXr ).Assembly ) );
            OsuGame.SetHost( Host );

            OsuPanel = new Panel();
            OsuPanel.Source.Add( OsuGame );

            OsuPanel.EmulatedInput.Pointer = Pointer;
            OsuPanel.ContentScale.Value = new Vector2( 2, 1 );

            AddInternal( scene );
            scene.Camera = Camera;
            scene.Root.Add( new SkyBox() );
            scene.Root.Add( new FloorGrid() );
            scene.Root.Add( Camera );
            scene.Root.Add( OsuPanel );
            scene.Root.Add( Pointer );
            PhysicsSystem.Root = scene.Root;
        }

		private ButtonStates<Key> lastKeys;
        private bool isKeyboardDisabled;
        protected override void Update () {
            base.Update();
            // HACK hide cursor because it jitters
            ( ( ( typeof( OsuGame ).GetField( "MenuCursorContainer", BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( OsuGame ) ) as MenuCursorContainer ).Cursor as MenuCursor ).Hide();

            if ( lastKeys is null ) {
                lastKeys = inputManager.CurrentState.Keyboard.Keys.Clone();
                return;
            }
            var keys = inputManager.CurrentState.Keyboard.Keys;
            var diff = keys.EnumerateDifference( lastKeys );
            var mouse = inputManager.CurrentState.Mouse.Position;

            if ( diff.Pressed.Contains( Key.Q ) ) {
                isKeyboardDisabled = !isKeyboardDisabled;
                OsuPanel.EmulatedInput.IsKeyboardActiveBindable.Value = isKeyboardDisabled;
            }

            if ( !isKeyboardDisabled ) {
                Vector2 direction = Vector2.Zero;
                if ( keys.IsPressed( Key.W ) ) direction += ( Camera.Rotation * new Vector4( 0, 0, 1, 1 ) ).Xz.Normalized();
                if ( keys.IsPressed( Key.S ) ) direction += ( Camera.Rotation * new Vector4( 0, 0, -1, 1 ) ).Xz.Normalized();
                if ( keys.IsPressed( Key.A ) ) direction += ( Camera.Rotation * new Vector4( -1, 0, 0, 1 ) ).Xz.Normalized();
                if ( keys.IsPressed( Key.D ) ) direction += ( Camera.Rotation * new Vector4( 1, 0, 0, 1 ) ).Xz.Normalized();

                Camera.Position += new Vector3( direction.X, 0, direction.Y ) * (float)( Time.Elapsed / 1000 );
            }
            Camera.Rotation = Quaternion.FromEulerAngles( 0, ( mouse.X - DrawSize.X / 2 ) / 200, 0 ) * Quaternion.FromEulerAngles( Math.Clamp( ( mouse.Y - DrawSize.Y / 2 ) / 200, -MathF.PI * 2 / 5, MathF.PI * 2 / 5 ), 0, 0 );
            lastKeys = keys.Clone();
        }

		public override XrScene CreateScene () {
            return scene;
		}
	}
}
