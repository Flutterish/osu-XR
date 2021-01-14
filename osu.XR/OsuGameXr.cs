using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Input;
using osu.Framework.Input.States;
using osu.Framework.IO.Stores;
using osu.Game;
using osu.XR.Components;
using osu.XR.Maths;
using osu.XR.Projection;
using osu.XR.Rendering;
using osuTK;
using osuTK.Input;
using System;
using System.Linq;

namespace osu.XR {
	internal class OsuGameXr : OsuGame {
        [Cached]
        public readonly Camera Camera = new() { Position = new Vector3( 0, 0, 0 ) };
        private BufferedCapture content;
        XrScene scene;
        internal InputManager InputManager;
        private InputManager inputManager => InputManager ??= GetContainingInputManager();
        public OsuGameXr ( string[] args ) : base( args ) { }

        protected override void LoadComplete () {
            base.LoadComplete();
            Resources.AddStore( new DllResourceStore( typeof( OsuGameXr ).Assembly ) );
            content = new BufferedCapture { RelativeSizeAxes = Axes.Both };
            foreach ( var i in InternalChildren.ToArray() ) {
                RemoveInternal( i );
                content.Add( i );
            }
            base.AddInternal( content );

            base.AddInternal( scene = new XrScene { Camera = Camera, RelativeSizeAxes = Axes.Both } );
            scene.Root.Add( new SkyBox() );
            scene.Root.Add( new FloorGrid() );
            scene.Root.Add( new Panel( content ) );
        }

        private ButtonStates<Key> lastKeys;
        private bool rotationLocked;
        protected override void Update () {
            base.Update();

            if ( lastKeys is null ) {
                lastKeys = inputManager.CurrentState.Keyboard.Keys.Clone();
                return;
            }
            var keys = inputManager.CurrentState.Keyboard.Keys;
            var diff = keys.EnumerateDifference( lastKeys );
            var mouse = inputManager.CurrentState.Mouse.Position;

            if ( diff.Pressed.Contains( Key.Q ) ) rotationLocked = !rotationLocked;
            if ( !rotationLocked ) Camera.Rotation = Quaternion.FromEulerAngles( 0, ( mouse.X - DrawSize.X / 2 ) / 200, 0 ) * Quaternion.FromEulerAngles( Math.Clamp( ( mouse.Y - DrawSize.Y / 2 ) / 200, -MathF.PI * 2 / 5, MathF.PI * 2 / 5 ), 0, 0 );

            Vector2 direction = Vector2.Zero;
            if ( keys.IsPressed( Key.I ) ) direction += ( Camera.Rotation * new Vector4( 0, 0, 1, 1 ) ).Xz.Normalized();
            if ( keys.IsPressed( Key.K ) ) direction += ( Camera.Rotation * new Vector4( 0, 0, -1, 1 ) ).Xz.Normalized();
            if ( keys.IsPressed( Key.J ) ) direction += ( Camera.Rotation * new Vector4( -1, 0, 0, 1 ) ).Xz.Normalized();
            if ( keys.IsPressed( Key.L ) ) direction += ( Camera.Rotation * new Vector4( 1, 0, 0, 1 ) ).Xz.Normalized();

            Camera.Position += new Vector3( direction.X, 0, direction.Y ) * (float)( Time.Elapsed / 1000 );
            lastKeys = keys.Clone();
        }

        protected override void AddInternal ( Drawable drawable ) {
            if ( content is null || drawable == content )
                base.AddInternal( drawable );
            else
                content.Add( drawable );
        }
    }
}
