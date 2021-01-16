using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Input;
using osu.Framework.Input.States;
using osu.Framework.IO.Stores;
using osu.Game;
using osu.Game.Graphics.Containers;
using osu.XR.Components;
using osu.XR.Maths;
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
    /// The full osu!lazer experience in VR.
    /// </summary>
	internal class OsuGameXr : OsuGame {
        [Cached]
        public readonly PhysicsSystem PhysicsSystem = new();
        internal InputManager InputManager;
        private InputManager inputManager => InputManager ??= GetContainingInputManager();
        [Cached]
        public readonly Camera Camera = new() { Position = new Vector3( 0, 0, 0 ) };
        private BufferedCapture content;
        XrScene scene;
        public Panel OsuPanel;
        [Cached]
        public readonly Pointer Pointer = new Pointer();
        private XrInputManager EmulatedInput;
        public OsuGameXr ( string[] args ) : base( args ) { } // TODO i want the game captured here so they dont have a chance to cache any top level drawables

        protected override void LoadComplete () {
            base.LoadComplete();
            Resources.AddStore( new DllResourceStore( typeof( OsuGameXr ).Assembly ) );
            float yScale = 2f;
            // size has to be less than the actual screen because clipping shenigans
            // TODO figure out how to render in any resolution without downgrading quality. might also just modify o!f to not clip.
            content = new BufferedCapture { RelativeSizeAxes = Axes.Both, Size = new Vector2( 1, 1/yScale ), FrameBufferScale = new Vector2( yScale ) };
            OsuPanel = new Panel( content );

            var contentWrapper = EmulatedInput = new XrInputManager( Pointer, OsuPanel ) { RelativeSizeAxes = Axes.Both };
            var internalChildren = InternalChildren.ToArray();
            //var children = Children.ToArray();
            ClearInternal( false );
            contentWrapper.AddRange( internalChildren );
            content.Add( contentWrapper );
            AddInternal( content );

            AddInternal( scene = new XrScene { Camera = Camera, RelativeSizeAxes = Axes.Both } );
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

            if ( lastKeys is null ) {
                lastKeys = inputManager.CurrentState.Keyboard.Keys.Clone();
                return;
            }
            var keys = inputManager.CurrentState.Keyboard.Keys;
            var diff = keys.EnumerateDifference( lastKeys );
            var mouse = inputManager.CurrentState.Mouse.Position;

            if ( diff.Pressed.Contains( Key.Q ) ) {
                isKeyboardDisabled = !isKeyboardDisabled;
                EmulatedInput.IsKeyboardActiveBindable.Value = isKeyboardDisabled;
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
    }
}
