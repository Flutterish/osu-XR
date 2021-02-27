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
using osu.XR.Input;
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
    // TODO skybox settings:
    // Rave!
    // Storyboard

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
        OsuGame OsuGame;
        [Cached]
        public readonly BeatProvider BeatProvider = new();
        [Cached]
        public readonly XrNotificationPanel Notifications = new XrNotificationPanel();
        [Cached( name: "FocusedPanel" )]
        public readonly Bindable<Panel> FocusedPanel = new();
        [Cached]
        public readonly XrKeyboard Keyboard = new() { Scale = new Vector3( 0.04f ) };

        public XrController MainController => controllers.Values.FirstOrDefault( x => x.Source.IsEnabled && x.Source.IsMainController ) ?? controllers.Values.FirstOrDefault( x => x.Source.IsEnabled );
        public XrController SecondaryController {
            get {
                var main = MainController;
                return controllers.Values.FirstOrDefault( x => x != main && x.Source.IsEnabled );
            }
        }

        Dictionary<Controller, XrController> controllers = new();
        public XrController GetControllerFor ( Controller controller ) => controller is null ? null : ( controllers.TryGetValue( controller, out var c ) ? c : null );

        DependencyContainer dependency;
		protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
            return dependency = new DependencyContainer( base.CreateChildDependencies(parent) );
        }

        private string[] args;
		public OsuGameXr ( string[] args ) {
            this.args = args.ToArray();

            OpenVR.NET.Events.OnMessage += msg => {
                Notifications.Post( new SimpleNotification() { Text = msg } );
            };
            OpenVR.NET.Events.OnError += msg => {
                Notifications.Post( new SimpleNotification() { Text = msg, Icon = FontAwesome.Solid.Bomb } );
            };
            OpenVR.NET.Events.OnException += (msg,e) => {
                Notifications.Post( new SimpleNotification() { Text = msg, Icon = FontAwesome.Solid.Bomb } );
            };
            Scene = new XrScene { RelativeSizeAxes = Axes.Both, Camera = Camera };
            PhysicsSystem.Root = Scene.Root;

            setManifest();
        }

        private void setManifest () {
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

            OsuGame = new OsuGame( args ) { RelativeSizeAxes = Axes.None, Size = new Vector2( 1920 * 2, 1080 ) };
            OsuGame.SetHost( Host );
            AddInternal( OsuGame );

            OsuGame.OnLoadComplete += _ => {
                RemoveInternal( OsuGame );
                osuLoaded();
            };
        }

        void osuLoaded () {
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
            // another option is to add dependent items to osugame and create a proxy

            OsuPanel.Source.Add( OsuGame );
            OsuPanel.AutosizeBoth();

            dependency.CacheAs( OsuGame.Dependencies.Get<PreviewTrackManager>() );
            dependency.CacheAs( OsuGame.Dependencies.Get<OsuColour>() );
            dependency.CacheAs( OsuGame.Dependencies.Get<RulesetStore>() );
            dependency.CacheAs( OsuGame.Dependencies.Get<SessionStatics>() );
            dependency.CacheAs( OsuGame.Dependencies.Get<IBindable<WorkingBeatmap>>() );
            dependency.CacheAs<OsuGameBase>( OsuGame );
            dependency.CacheAs<Framework.Game>( OsuGame );

            // TODO transparency that either doesnt depend on order or is transparent-shader agnostic
            // for now we are just sorting objects here
            AddInternal( BeatProvider );
            AddInternal( Scene );
            Scene.Add( new SkyBox() );
            Scene.Add( new FloorGrid() );
            Scene.Add( new BeatingScenery() );
            Scene.Add( Camera );
            Scene.Add( OsuPanel );
            Scene.Add( new HandheldMenu().With( s => s.Panels.AddRange( new FlatPanel[] { new XrConfigPanel(), Notifications } ) ) );
            //Scene.Add( Keyboard );

            Config.BindWith( XrConfigSetting.InputMode, inputModeBindable );
            inputModeBindable.BindValueChanged( v => {
                onControllerInputModeChanged();
            }, true );

            Config.BindWith( XrConfigSetting.ScreenHeight, screenHeightBindable );
            screenHeightBindable.BindValueChanged( v => OsuPanel.Y = v.NewValue, true );

            screenResX.BindValueChanged( v => OsuGame.Width = v.NewValue, true );
            screenResY.BindValueChanged( v => OsuGame.Height = v.NewValue, true );

            Config.BindWith( XrConfigSetting.ScreenRadius, OsuPanel.RadiusBindable );
            Config.BindWith( XrConfigSetting.ScreenArc, OsuPanel.ArcBindable );

            Config.BindWith( XrConfigSetting.ScreenResolutionX, screenResX );
            Config.BindWith( XrConfigSetting.ScreenResolutionY, screenResY );

            Keyboard.LoadModel( @".\Resources\keyboard.obj" );

            VR.BindNewControllerAdded( c => {
                this.ScheduleAfterChildren( () => {
                    var controller = new XrController( c );
                    controllers.Add( c, controller );
                    Scene.Add( controller );

                    c.BindEnabled( () => {
                        onControllerInputModeChanged();
                    }, true );
                    c.BindDisabled( () => {
                        onControllerInputModeChanged();
                    }, true );
                } );
            }, true );

            VR.BindComponentsLoaded( () => {
                var haptic = VR.GetControllerComponent<ControllerHaptic>( XrAction.Feedback );
                haptic.TriggerVibration( 0.5 ); // NOTE haptics dont work yet
            } );
        }
	}
}
