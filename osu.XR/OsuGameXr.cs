using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.States;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Game;
using osu.Game.Graphics.Cursor;
using osu.XR.Components;
using osu.XR.Maths;
using osu.XR.Physics;
using osu.XR.Projection;
using osu.XR.Rendering;
using osu.XR.VR;
using osuTK;
using osuTK.Input;
using System;
using System.Collections.Generic;
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
        public readonly Panel OsuPanel = new Panel { Y = 1.8f };
        [Cached]
        public readonly Pointer Pointer = new Pointer();
        [Cached(typeof(Framework.Game))]
        OsuGame OsuGame;

        public OsuGameXr ( string[] args ) { 
            OsuGame = new OsuGame( args ) { RelativeSizeAxes = Axes.Both };
            Scene = new XrScene { RelativeSizeAxes = Axes.Both };
        }

        protected override void LoadComplete () {
            base.LoadComplete();
            Resources.AddStore( new DllResourceStore( typeof( OsuGameXr ).Assembly ) );
            OsuGame.SetHost( Host );

            OsuPanel.Source.Add( OsuGame );
            OsuPanel.EmulatedInput.Pointer = Pointer;
            OsuPanel.ContentScale.Value = new Vector2( 2, 1 );

            AddInternal( Scene );
            Scene.Camera = Camera;
            Scene.Root.Add( new SkyBox() );
            Scene.Root.Add( new FloorGrid() );
            Scene.Root.Add( Camera );
            Scene.Root.Add( OsuPanel );
            Scene.Root.Add( Pointer );
            PhysicsSystem.Root = Scene.Root;
        }

        Dictionary<Controller, XrController> controllers = new();
        protected override void Update () {
            base.Update();
            // HACK hide cursor because it jitters
            ( ( ( typeof( OsuGame ).GetField( "MenuCursorContainer", BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( OsuGame ) ) as MenuCursorContainer ).Cursor as MenuCursor ).Hide();

            foreach ( var i in VrManager.Current.Controllers.Values.Except( controllers.Keys ).ToArray() ) {
                controllers.Add( i, new XrController( i ) );
                Scene.Root.Add( controllers[ i ] );

                Pointer.Source ??= controllers[ i ].Transform;
            }
        }
	}
}
