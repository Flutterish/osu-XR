using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.XR;
using osu.Framework.XR.Components;
using osu.Framework.XR.Physics;
using osu.Framework.XR.Projection;
using osu.Game;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Overlays.Notifications;
using osu.Game.Resources;
using osu.Game.Rulesets;
using osu.XR.Components;
using osu.XR.Components.Groups;
using osu.XR.Components.Panels;
using osu.XR.Drawables;
using osu.XR.Input;
using osu.XR.Settings;
using osuTK;
using System.Collections.Generic;
using System.Linq;
using Valve.VR;

namespace osu.XR {
	// TODO separate out osu.Framework.XR

	// TODO skybox settings:
	// Rave!
	// Color squares
	// Tiled B/W sphere
	// Storyboard

	// TODO clap bindings and general user ruleset action bindings

	// TODO rigs
	// TODO 3rd person

	/// <summary>
	/// The full osu! experience in VR.
	/// </summary>
	[Cached]
	public class OsuGameXr : XrGame {
		[Cached]
		public readonly PhysicsSystem PhysicsSystem = new();
		[Cached]
		public readonly Camera Camera = new() { Position = new Vector3( 0, 0, 0 ) };
		public readonly CurvedPanel OsuPanel = new CurvedPanel { Y = 1.8f };
		public XrConfigManager Config { get; private set; }
		OsuGame OsuGame;
		[Cached]
		public readonly BeatProvider BeatProvider = new();
		[Cached]
		public readonly XrNotificationPanel Notifications = new XrNotificationPanel();
		[Cached]
		public readonly Bindable<IFocusable> GlobalFocusBindable = new(); // TODO this will be moved to Scene
		[Cached]
		public readonly XrKeyboard Keyboard = new() { Scale = new Vector3( 0.04f ) };
		[Cached]
		public readonly XrInspectorPanel Inspector = new();

		ETrackedControllerRole dominantHandRole => dominantHandBindable.Value switch {
			Hand.Right => ETrackedControllerRole.RightHand,
			Hand.Left => ETrackedControllerRole.LeftHand,
			Hand.Auto or _ => VR.DominantHand
		};
		public XrController MainController => controllers.Values.FirstOrDefault( x => x.Source.IsEnabled && x.Source.Role == dominantHandRole ) ?? controllers.Values.FirstOrDefault( x => x.Source.IsEnabled );
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
			Scene = new SceneWithMirrorWarning { RelativeSizeAxes = Axes.Both, Camera = Camera };
			PhysicsSystem.Root = Scene.Root;

			VR.BindVrStateChanged( v => {
				if ( v == VrState.OK ) {
					OpenVR.NET.Events.Message( $"Headset model: {VR.Current.Headset.Model.Name}" );
				}
			}, true );
		}

		public override Manifest XrManifest => new Manifest<XrActionGroup, XrAction> {
			LaunchType = LaunchType.Binary,
			IsDashBoardOverlay = false,
			Name = "system.generated.osu.xr.exe", // TODO this is the only name steamVR accepts for now
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
					Path = "DefaultBindings/knuckles.json"
				},
				new() {
					ControllerType = "vive_controller",
					Path = "DefaultBindings/vive_controller.json"
				},
				new() {
					ControllerType = "oculus_touch",
					Path = "DefaultBindings/oculus_touch.json"
				}
			}
		};

		Bindable<InputMode> inputModeBindable = new();
		Bindable<float> screenHeightBindable = new( 1.8f );

		Bindable<int> screenResX = new( 1920 * 2 );
		Bindable<int> screenResY = new( 1080 );

		void onControllersMutated () {
			wasInKeyboardProximity = false;
			foreach ( var controller in controllers.Values ) {
				controller.ModeOverrideBindable.Value = ControllerMode.Disabled;
			}

			var main = MainController;
			if ( inputModeBindable.Value == InputMode.SinglePointer ) {
				foreach ( var controller in controllers.Values ) {
					controller.Mode = controller == main ? ControllerMode.Pointer : ControllerMode.Disabled;
					controller.IsSoloMode = true;
				}
			}
			else if ( inputModeBindable.Value == InputMode.DoublePointer ) {
				foreach ( var controller in controllers.Values ) {
					controller.Mode = ControllerMode.Pointer;
					controller.IsSoloMode = false;
				}
			}
			else if ( inputModeBindable.Value == InputMode.TouchScreen ) {
				foreach ( var controller in controllers.Values ) {
					controller.Mode = ControllerMode.Touch;
					controller.IsSoloMode = false;
				}
			}

			foreach ( var controller in controllers.Values ) {
				controller.IsMainControllerBindable.Value = controller == main;
			}
		}

		bool wasInKeyboardProximity = false;
		protected override void Update () {
			base.Update();
			var inKeyboardProximity = controllers.Values.Any( i => {
				return i.Position.X - Keyboard.Position.X > -Keyboard.Size.X * Keyboard.Scale.X * 2 && i.Position.X - Keyboard.Position.X < Keyboard.Size.X * Keyboard.Scale.X * 2
					&& i.Position.Z - Keyboard.Position.Z > -Keyboard.Size.Z * Keyboard.Scale.Z * 2 && i.Position.Z - Keyboard.Position.Z < Keyboard.Size.Z * Keyboard.Scale.Z * 2
					&& i.Position.Y + 0.1 > Keyboard.Position.Y;
			} );
			if ( inKeyboardProximity != wasInKeyboardProximity ) {
				if ( inKeyboardProximity ) {
					foreach ( var i in controllers ) {
						i.Value.ModeOverrideBindable.Value = ControllerMode.Touch;
					}
				}
				else {
					foreach ( var i in controllers ) {
						i.Value.ModeOverrideBindable.Value = ControllerMode.Disabled;
					}
				}
				wasInKeyboardProximity = inKeyboardProximity;
			}
		}

		protected override void LoadComplete () {
			base.LoadComplete();

			OsuGame = new OsuGame( args ) { RelativeSizeAxes = Axes.None, Size = new Vector2( 1920 * 2, 1080 ) };
			OsuGame.SetHost( Host );
			AddInternal( OsuGame );

			// this is done like this because otherwise DI stealing is harder to do
			OsuGame.OnLoadComplete += _ => {
				RemoveInternal( OsuGame );
				osuLoaded();
			};
		}

		void stealOsuDI () {
			// TODO somehow just cache everything osugame caches ( either set our dep container to osu's + ours or somehow retreive all of its cache )
			// or maybe we can put the scene root as osus child and proxy it but i dont think it is possible
			Resources.AddStore( new DllResourceStore( typeof( OsuGameXr ).Assembly ) );
			Resources.AddStore( new DllResourceStore( typeof( OsuGame ).Assembly ) );
			Resources.AddStore( new DllResourceStore( OsuResources.ResourceAssembly ) );
			Resources.AddStore( new DllResourceStore( osu.Framework.XR.Resources.ResourceAssembly ) );

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

			dependency.CacheAs( OsuGame.Dependencies.Get<PreviewTrackManager>() );
			dependency.CacheAs( OsuGame.Dependencies.Get<OsuColour>() );
			dependency.CacheAs( OsuGame.Dependencies.Get<RulesetStore>() );
			dependency.CacheAs( OsuGame.Dependencies.Get<SessionStatics>() );
			dependency.CacheAs( OsuGame.Dependencies.Get<IBindable<WorkingBeatmap>>() );
			dependency.CacheAs<OsuGameBase>( OsuGame );
			dependency.CacheAs<Framework.Game>( OsuGame );
			dependency.CacheAs( OsuGame.Dependencies.Get<Storage>() );

			dependency.CacheAs( Config = new XrConfigManager( OsuGame.Dependencies.Get<Storage>() ) );
		}

		readonly Bindable<Hand> dominantHandBindable = new( Hand.Auto );
		void osuLoaded () {
			stealOsuDI();

			OsuPanel.Source.Add( OsuGame );
			OsuPanel.AutosizeBoth();

			// TODO transparency that either doesnt depend on order or is transparent-shader agnostic
			// for now we are just sorting objects here
			AddInternal( BeatProvider );
			AddInternal( Scene );
			Scene.Add( new SkyBox() );
			Scene.Add( new FloorGrid() );
			Scene.Add( new BeatingScenery() );
			Scene.Add( Camera );
			Scene.Add( OsuPanel );
			Scene.Add( new HandheldMenu().With( s => s.Panels.AddRange( new FlatPanel[] { new XrConfigPanel(), Notifications, Inspector } ) ) );
			Scene.Add( Keyboard );
			Keyboard.LoadModel( @".\Resources\keyboard.obj" );

			Config.BindWith( XrConfigSetting.InputMode, inputModeBindable );
			Config.BindWith( XrConfigSetting.DominantHand, dominantHandBindable );
			inputModeBindable.BindValueChanged( v => {
				onControllersMutated();
			}, true );
			dominantHandBindable.BindValueChanged( v => {
				onControllersMutated();
			} );

			Config.BindWith( XrConfigSetting.ScreenHeight, screenHeightBindable );
			screenHeightBindable.BindValueChanged( v => OsuPanel.Y = v.NewValue, true );

			screenResX.BindValueChanged( v => OsuGame.Width = v.NewValue, true );
			screenResY.BindValueChanged( v => OsuGame.Height = v.NewValue, true );

			Config.BindWith( XrConfigSetting.ScreenRadius, OsuPanel.RadiusBindable );
			Config.BindWith( XrConfigSetting.ScreenArc, OsuPanel.ArcBindable );

			Config.BindWith( XrConfigSetting.ScreenResolutionX, screenResX );
			Config.BindWith( XrConfigSetting.ScreenResolutionY, screenResY );

			VR.BindNewControllerAdded( c => {
				this.ScheduleAfterChildren( () => {
					var controller = new XrController( c );
					controllers.Add( c, controller );
					Scene.Add( controller );

					c.BindEnabled( () => {
						onControllersMutated();
					}, true );
					c.BindDisabled( () => {
						onControllersMutated();
					}, true );

					Config.BindWith( XrConfigSetting.SinglePointerTouch, controller.SinglePointerTouchBindable );
					Config.BindWith( XrConfigSetting.TapOnPress, controller.TapTouchBindable );
				} );
			}, true );

			Config.BindWith( XrConfigSetting.RenderToScreen, Scene.RenderToScreenBindable );
		}
	}
}
