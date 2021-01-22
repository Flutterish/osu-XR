using OpenVR.NET;
using OpenVR.NET.Manifests;
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

        public OsuGameXr ( string[] args ) { // BUG sometimes at startup osu throws an error. investigate.
            OsuGame = new OsuGame( args ) { RelativeSizeAxes = Axes.Both };
            Scene = new XrScene { RelativeSizeAxes = Axes.Both };

            VR.SetManifest( new Manifest<XrActionGroup, XrAction> {
                LaunchType = LaunchType.Binary,
                IsDashBoardOverlay = false,
                Name = "perigee.osuXR",
                Localizations = new() {
                    new( "en_us" ) {
                        Name = "osu!XR",
                        Description = "The full osu! experience in VR"
                    }
                },
                Groups = new() {
                    new() {
                        Type = ActionGroupType.LeftRight,
                        Name = XrActionGroup.Main,
                        Actions = new() {
							new() {
                                Name = XrAction.Press,
                                Type = ActionType.Boolean,
                                Requirement = Requirement.Mandatory,
                                Localizations = new() { ["en_us"] = "Press" }
							}
						},
                        Localizations = new() { ["en_us"] = "Main" },
					}
				},
                DefaultBindings = new() {
					new() {
                        ControllerType = "knuckles",
                        Path = "system_generated_osu_xr_exe_binding_knuckles.json"
                    }
                }
            } );
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
        bool areActionsBound;
        protected override void Update () {
            base.Update();
            // HACK hide cursor because it jitters
            ( ( ( typeof( OsuGame ).GetField( "MenuCursorContainer", BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( OsuGame ) ) as MenuCursorContainer ).Cursor as MenuCursor ).Hide();

            foreach ( var i in VR.Current.Controllers.Values.Except( controllers.Keys ).ToArray() ) {
                var controller = new XrController( i );
                controllers.Add( i, controller );
                Scene.Add( controller );

                Pointer.Source ??= controller;
            }

            if ( !areActionsBound && VR.AreComponentsLoaded ) {
                areActionsBound = true;
                var click = VR.GetControllerComponent<ControllerButton>( XrAction.Press );
                click.BindValueChanged( v => {
                    OsuPanel.EmulatedInput.IsPressed = v;
                }, true );
            }
        }
	}
}
