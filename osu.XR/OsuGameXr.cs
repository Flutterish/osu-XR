using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
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
using osu.Game.Overlays.Notifications;
using osu.Game.Resources;
using osu.Game.Rulesets;
using osu.XR.Components;
using osu.XR.Components.Groups;
using osu.XR.Components.Panels;
using osu.XR.Drawables;
using osu.XR.Graphics;
using osu.XR.Maths;
using osu.XR.Physics;
using osu.XR.Projection;
using osu.XR.Rendering;
using osu.XR.Settings;
using osuTK;
using osuTK.Input;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Schema;
using Pointer = osu.XR.Components.Pointers.RaycastPointer;

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
        public readonly CurvedPanel OsuPanel = new CurvedPanel { Y = 1.8f };
        [Cached]
        public readonly XrConfigManager Config = new();
        [Cached(typeof(Framework.Game))]
        OsuGame OsuGame;
        [Cached]
        public readonly BeatProvider BeatProvider = new();
        [Cached]
        public readonly XrNotificationPanel Notifications = new XrNotificationPanel();

        public XrController MainController => controllers.Values.FirstOrDefault( x => x.Source.IsEnabled && x.Source.IsMainController ) ?? controllers.Values.FirstOrDefault( x => x.Source.IsEnabled );
        public XrController SecondaryController {
            get {
                var main = MainController;
                return controllers.Values.FirstOrDefault( x => x != main && x.Source.IsEnabled );
            }
        }
        public XrController GetControllerFor ( Controller controller ) => controller is null ? null : ( controllers.TryGetValue( controller, out var c ) ? c : null );

        DependencyContainer dependency;
		protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
            return dependency = new DependencyContainer( base.CreateChildDependencies(parent) );
        }

        object updatelock = new { };
		public OsuGameXr ( string[] args ) { // BUG sometimes at startup osu throws an error. investigate.
            OpenVR.NET.Events.OnMessage += msg => {
                Notifications.Post( new SimpleNotification() { Text = msg } );
            };
            OpenVR.NET.Events.OnError += msg => {
                Notifications.Post( new SimpleNotification() { Text = msg, Icon = FontAwesome.Solid.Bomb } );
            };
            OpenVR.NET.Events.OnException += (msg,e) => {
                Notifications.Post( new SimpleNotification() { Text = msg, Icon = FontAwesome.Solid.Bomb } );
            };
            OsuGame = new OsuGame( args ) { RelativeSizeAxes = Axes.None, Size = new Vector2( 1920 * 2, 1080 ) };
            Scene = new XrScene { RelativeSizeAxes = Axes.Both, Camera = Camera };

            VR.BindNewControllerAdded( c => {
                var controller = new XrController( c );
                controllers.Add( c, controller );
                lock ( updatelock ) {
                    onUpdateThread += () => {
                        Scene.Add( controller );

                        c.BindEnabled( () => {
                            onControllerInputModeChanged();
                        }, true );
                        c.BindDisabled( () => {
                            onControllerInputModeChanged();
                        } );
                    };
                }
            }, true );

            VR.BindComponentsLoaded( () => {
                var haptic = VR.GetControllerComponent<ControllerHaptic>( XrAction.Feedback );
                haptic.TriggerVibration( 0.5 ); // NOTE haptics dont work yet
            } );

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
                        Type = ActionGroupType.LeftRight,
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

            Config.BindWith( XrConfigSetting.InputMode, inputModeBindable );
            inputModeBindable.BindValueChanged( v => {
                onControllerInputModeChanged();
            }, true );
            Config.BindWith( XrConfigSetting.ScreenHeight, screenHeightBindable );
            screenHeightBindable.BindValueChanged( v => OsuPanel.Y = v.NewValue, true );

            screenResX.BindValueChanged( v => OsuGame.Width = v.NewValue, true );
            screenResY.BindValueChanged( v => OsuGame.Height = v.NewValue, true );
        }
        Bindable<InputMode> inputModeBindable = new();
        Bindable<float> screenHeightBindable = new( 1.8f );

        Bindable<int> screenResX = new( 1920 * 2 );
        Bindable<int> screenResY = new( 1080 );

        void onControllerInputModeChanged () {
            var main = MainController;
            if ( inputModeBindable.Value == InputMode.SinglePointer ) {
                foreach ( var controller in controllers.Values ) {
                    controller.Mode = controller == main ? ControllerMode.Pointer : ControllerMode.Disabled;
                }
			}
            else if ( inputModeBindable.Value == InputMode.DoublePointer ) {
                foreach ( var controller in controllers.Values ) {
                    controller.Mode = ControllerMode.Pointer;
                }
            }
            else if ( inputModeBindable.Value == InputMode.TouchScreen ) {
                foreach ( var controller in controllers.Values ) {
                    controller.Mode = ControllerMode.Touch;
                }
            }
		}

        protected override void LoadComplete () {
            base.LoadComplete();

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
            OsuPanel.AutosizeBoth();

            OsuGame.OnLoadComplete += v => {
                dependency.CacheAs( OsuGame.Dependencies.Get<PreviewTrackManager>() );
                dependency.CacheAs( OsuGame.Dependencies.Get<OsuColour>() ); 
                dependency.CacheAs( OsuGame.Dependencies.Get<RulesetStore>() );
                dependency.CacheAs( OsuGame.Dependencies.Get<SessionStatics>() );
                dependency.CacheAs( OsuGame.Dependencies.Get<IBindable<WorkingBeatmap>>() );
                dependency.CacheAs<OsuGameBase>( OsuGame );

                Scene.Add( new HandheldMenu().With( s => s.Panels.AddRange( new FlatPanel[] { new XrConfigPanel(), Notifications } ) ) );
                AddInternal( BeatProvider );
            };

            // TODO transparency that either doesnt depend on order or is transparent-shader agnostic
            // for now we are just sorting objects here
            AddInternal( Scene );
            Scene.Root.Add( new SkyBox() );
            Scene.Root.Add( new FloorGrid() );
            Scene.Root.Add( new BeatingScenery() );
            Scene.Root.Add( Camera );
            Scene.Root.Add( OsuPanel );
            PhysicsSystem.Root = Scene.Root;

            Config.BindWith( XrConfigSetting.ScreenRadius, OsuPanel.RadiusBindable );
            Config.BindWith( XrConfigSetting.ScreenArc, OsuPanel.ArcBindable );

            Config.BindWith( XrConfigSetting.ScreenResolutionX, screenResX );
            Config.BindWith( XrConfigSetting.ScreenResolutionY, screenResY );
        }

        Dictionary<Controller, XrController> controllers = new();
        private event System.Action onUpdateThread;
        protected override void Update () {
            base.Update();

            lock ( updatelock ) {
                onUpdateThread?.Invoke();
                onUpdateThread = null;
            }

            // HACK hide cursor because it jitters
            ( ( ( typeof( OsuGame ).GetField( "MenuCursorContainer", BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( OsuGame ) ) as MenuCursorContainer ).Cursor as MenuCursor ).Hide();
        }
	}
}
