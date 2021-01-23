using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.States;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Game;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Cursor;
using osu.Game.Overlays;
using osu.Game.Resources;
using osu.Game.Rulesets;
using osu.XR.Components;
using osu.XR.Drawables;
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
using System.Xml.Schema;
using Pointer = osu.XR.Components.Pointer;

namespace osu.XR {
	/// <summary>
	/// The full osu! experience in VR.
	/// </summary>
    [Cached]
	public class OsuGameXr : XrGame {
        [Cached]
        public readonly PhysicsSystem PhysicsSystem = new();
        internal InputManager _inputManager;
        private InputManager inputManager => _inputManager ??= GetContainingInputManager();
        [Cached]
        public readonly Camera Camera = new() { Position = new Vector3( 0, 0, 0 ) };
        public readonly CurvedPanel OsuPanel = new CurvedPanel { Y = 1.8f }; // TODO our own VR error panel
        public readonly XrConfigPanel ConfigPanel = new XrConfigPanel();
        [Cached(typeof(Framework.Game))]
        OsuGame OsuGame;

        public XrController MainController => controllers.Values.FirstOrDefault( x => x.Controller.IsEnabled && x.Controller.IsMainController ) ?? controllers.Values.FirstOrDefault( x => x.Controller.IsEnabled );
        public XrController SecondaryController {
            get {
                var main = MainController;
                return controllers.Values.FirstOrDefault( x => x != main && x.Controller.IsEnabled );
            }
        }

        DependencyContainer dependency;
		protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
            return dependency = new DependencyContainer( base.CreateChildDependencies(parent) );
        }

		public OsuGameXr ( string[] args ) { // BUG sometimes at startup osu throws an error. investigate.
            OsuGame = new OsuGame( args ) { RelativeSizeAxes = Axes.Both };
            Scene = new XrScene { RelativeSizeAxes = Axes.Both };

            VR.BindNewControllerAdded( c => {
                var controller = new XrController( c );
                controllers.Add( c, controller );
                onUpdateThread += () => {
                    Scene.Add( controller );

                    controller.Pointer.HitChanged += _ => reassignFocus();

                    c.BindEnabled( () => {
                        onControllerInputModeChanged();
                    }, true );
                    c.BindDisabled( () => {
                        onControllerInputModeChanged();
                    } );
                };
            }, true );

            VR.BindComponentsLoaded( () => {
                var mouseLeft = VR.GetControllerComponent<ControllerButton>( XrAction.MouseLeft );
                mouseLeft.BindValueChangedDetailed( v => {
                    if ( v.Source is null || !controllers.ContainsKey( v.Source ) ) return;
                    var Pointer = controllers[ v.Source ].Pointer;
                    if ( Pointer.CurrentFocus is Panel panel ) {
                        if ( VR.EnabledControllerCount > 1 ) {
                            if ( panel.RequestedInputMode == PanelInputMode.Regular ) panel.EmulatedInput.IsLeftPressed = v.NewValue;
                            else if ( panel.RequestedInputMode == PanelInputMode.Inverted ) panel.EmulatedInput.IsRightPressed = v.NewValue;
                        }
                        else panel.EmulatedInput.IsLeftPressed = v.NewValue;
                    }
                }, true );

                var mouseRight = VR.GetControllerComponent<ControllerButton>( XrAction.MouseRight );
                mouseRight.BindValueChangedDetailed( v => {
                    if ( v.Source is null || !controllers.ContainsKey( v.Source ) ) return;
                    var Pointer = controllers[ v.Source ].Pointer;
                    if ( Pointer.CurrentFocus is Panel panel ) {
                        if ( VR.EnabledControllerCount > 1 ) {
                            if ( panel.RequestedInputMode == PanelInputMode.Regular ) panel.EmulatedInput.IsRightPressed = v.NewValue;
                            else if ( panel.RequestedInputMode == PanelInputMode.Inverted ) panel.EmulatedInput.IsLeftPressed = v.NewValue;
                        }
                        else panel.EmulatedInput.IsLeftPressed = v.NewValue;
                    }
                }, true );

                var scroll = VR.GetControllerComponent<Controller2DVector>( XrAction.Scroll );
                scroll.BindValueUpdatedDetailed( v => {
                    if ( v.Source is null || !controllers.ContainsKey( v.Source ) ) return;
                    var Pointer = controllers[ v.Source ].Pointer;
                    if ( Pointer.CurrentFocus is Panel panel ) panel.EmulatedInput.Scroll += new Vector2( v.NewValue.X, v.NewValue.Y ) * (float)VR.DeltaTime * 80;
                } );

                var haptic = VR.GetControllerComponent<ControllerHaptic>( XrAction.Feedback );
                haptic.TriggerVibration( 0.5 ); // NOTE haptics dont work yet
            } );

            ConfigPanel.IsVisibleBindable.ValueChanged += v => {
                onControllerInputModeChanged();
            };

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
                        Name = XrActionGroup.Pointer,
                        Actions = new() {
                            new() {
                                Name = XrAction.MouseLeft,
                                Type = ActionType.Boolean,
                                Requirement = Requirement.Mandatory,
                                Localizations = new() { [ "en_us" ] = "Left Click" }
                            },
                            new() {
                                Name = XrAction.MouseRight,
                                Type = ActionType.Boolean,
                                Requirement = Requirement.Mandatory,
                                Localizations = new() { [ "en_us" ] = "Right Click" }
                            },
                            new() {
                                Name = XrAction.Scroll,
                                Type = ActionType.Vector2,
                                Requirement = Requirement.Suggested,
                                Localizations = new() { [ "en_us" ] = "Scroll" }
                            }
                        },
                        Localizations = new() { [ "en_us" ] = "Pointer" },
					},
                    new() {
                        Type = ActionGroupType.LeftRight,
                        Name = XrActionGroup.Configuration,
                        Actions = new() {
							new() {
                                Name = XrAction.ToggleMenu,
                                Type = ActionType.Boolean,
                                Requirement = Requirement.Suggested,
                                Localizations = new() { [ "en_us" ] = "Toggle configuration panel" }
                            }
						},
                        Localizations = new() { [ "en_us" ] = "Configuration" }
					},
                    new() {
                        Type = ActionGroupType.Hidden,
                        Name = XrActionGroup.Haptics,
                        Actions = new() {
                            new() {
                                Name = XrAction.Feedback,
                                Type = ActionType.Vibration,
                                Requirement = Requirement.Suggested,
                                Localizations = new() { [ "en_us" ] = "Feedback" }
                            }
                        },
                        Localizations = new() { [ "en_us" ] = "Haptics" }
                    },
				},
                DefaultBindings = new() {
					new() {
                        ControllerType = "knuckles",
                        Path = "system_generated_osu_xr_exe_binding_knuckles.json"
                    }
                }
            } );
        }

        void onControllerInputModeChanged () {

		}

        void reassignFocus () {
            foreach ( var panel in panels ) {
                var controller = 
                    controllers.Values.FirstOrDefault( x => x.Pointer.CurrentHit == panel && x.Controller.IsMainController ) ?? 
                    controllers.Values.FirstOrDefault( x => x.Pointer.CurrentHit == panel ) ?? 
                    controllers.Values.FirstOrDefault( x => x.Pointer.CurrentFocus == panel && x.Controller.IsMainController ) ??
                    controllers.Values.FirstOrDefault( x => x.Pointer.CurrentFocus == panel );
                panel.HasFocus = controller is not null;
                if ( panel.HasFocus ) {
                    panel.EmulatedInput.Pointer = controller.Pointer;
				}
			}
		}

        List<Panel> panels = new();

        protected override void LoadComplete () {
            base.LoadComplete();
            Scene.Root.BindHierarchyChange( (parent,added) => {
                if ( added is Panel panel ) {
                    panels.Add( panel );
                    reassignFocus();
                }
            },
            (parent,removed) => {
                if ( removed is Panel panel ) {
                    panels.Remove( panel );
                    reassignFocus();
                }
            }, true );

            Resources.AddStore( new DllResourceStore( typeof( OsuGameXr ).Assembly ) );
            Resources.AddStore( new DllResourceStore( typeof( OsuGame ).Assembly ) );
            Resources.AddStore( new DllResourceStore( OsuResources.ResourceAssembly ) );
            AddFont( Resources, @"Fonts/osuFont" );

            AddFont( Resources, @"Fonts/Torus-Regular" );
            AddFont( Resources, @"Fonts/Torus-Light" );
            AddFont( Resources, @"Fonts/Torus-SemiBold" );
            AddFont( Resources, @"Fonts/Torus-Bold" );

            AddFont( Resources, @"Fonts/Noto-Basic" );
            AddFont( Resources, @"Fonts/Noto-Hangul" );
            AddFont( Resources, @"Fonts/Noto-CJK-Basic" );
            AddFont( Resources, @"Fonts/Noto-CJK-Compatibility" );
            AddFont( Resources, @"Fonts/Noto-Thai" );

            AddFont( Resources, @"Fonts/Venera-Light" );
            AddFont( Resources, @"Fonts/Venera-Bold" );
            AddFont( Resources, @"Fonts/Venera-Black" );
            // TODO somehow just cache everything osugame caches ( either set our dep container to osu's + ours or somehow retreive all of its cache )
            OsuGame.SetHost( Host );

            OsuPanel.Source.Add( OsuGame );
            OsuPanel.ContentScale.Value = new Vector2( 2, 1 );

            OsuGame.OnLoadComplete += v => {
                dependency.CacheAs( OsuGame.Dependencies.Get<PreviewTrackManager>() );
                dependency.CacheAs( OsuGame.Dependencies.Get<OsuColour>() ); 
                dependency.CacheAs( OsuGame.Dependencies.Get<RulesetStore>() );
                dependency.CacheAs( OsuGame.Dependencies.Get<SessionStatics>() );
                dependency.CacheAs( OsuGame.Dependencies.Get<IBindable<WorkingBeatmap>>() );
                dependency.CacheAs<OsuGameBase>( OsuGame );

                onUpdateThread += () => {
                    Scene.Add( ConfigPanel );
                };
            };

            AddInternal( Scene );
            Scene.Camera = Camera;
            Scene.Root.Add( new SkyBox() );
            Scene.Root.Add( new FloorGrid() );
            Scene.Root.Add( Camera );
            Scene.Root.Add( OsuPanel );
            PhysicsSystem.Root = Scene.Root;
        }

		Dictionary<Controller, XrController> controllers = new();
        private event System.Action onUpdateThread;
        protected override void Update () {
            base.Update();

            onUpdateThread?.Invoke();
            onUpdateThread = null;

            // HACK hide cursor because it jitters
            ( ( ( typeof( OsuGame ).GetField( "MenuCursorContainer", BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( OsuGame ) ) as MenuCursorContainer ).Cursor as MenuCursor ).Hide();
        }
	}
}
