using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.XR;
using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
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
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osu.XR.Components;
using osu.XR.Components.Groups;
using osu.XR.Components.Panels;
using osu.XR.Drawables;
using osu.XR.Input;
using osu.XR.Input.Custom;
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
		readonly OsuPanel OsuPanel = new OsuPanel();
		public XrConfigManager Config { get; private set; }
		OsuGame OsuGame;
		[Cached]
		public readonly BeatProvider BeatProvider = new();
		[Cached]
		public readonly NotificationsPanel Notifications = new NotificationsPanel();
		[Cached]
		public readonly Bindable<IFocusable> GlobalFocusBindable = new(); // TODO this will be moved to Scene
		public readonly XrKeyboard Keyboard = new() { Scale = new Vector3( 0.04f ) };
		public readonly XrKeyboard FlatKeyboard = new() { Scale = new Vector3( 0.04f ) };
		[Cached]
		public readonly InspectorPanel Inspector = new();
		[Cached]
		public readonly RulesetInfoPanel InputBindings = new();

		public static ETrackedControllerRole RoleForHand ( Hand hand ) => hand switch {
			Hand.Right => ETrackedControllerRole.RightHand,
			Hand.Left => ETrackedControllerRole.LeftHand,
			Hand.Auto or _ => VR.DominantHand
		};
		ETrackedControllerRole dominantHandRole => RoleForHand( dominantHandBindable.Value );

		public XrController MainController => controllers.Values.FirstOrDefault( x => x.Source.IsEnabled && x.Source.Role == dominantHandRole ) ?? controllers.Values.FirstOrDefault( x => x.Source.IsEnabled );
		public XrController SecondaryController {
			get {
				var main = MainController;
				return controllers.Values.FirstOrDefault( x => x != main && x.Source.IsEnabled );
			}
		}
		public IEnumerable<XrController> FreeControllers => controllers.Values.Where( x => x.Source.IsEnabled && !x.IsHoldingAnything && x.Mode != ControllerMode.Disabled );

		Dictionary<Controller, XrController> controllers = new();
		public XrController GetControllerFor ( Controller controller ) => controller is null ? null : ( controllers.TryGetValue( controller, out var c ) ? c : null );
		public XrController GetControllerFor ( Hand hand ) => GetControllerFor( RoleForHand( hand ) );
		XrController GetControllerFor ( ETrackedControllerRole role ) => controllers.Values.FirstOrDefault( x => x.Source.Role == role );

		DependencyContainer dependency;
		protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
			return dependency = new DependencyContainer( base.CreateChildDependencies(parent) );
		}

		private string[] args;
		public OsuGameXr ( string[] args ) {
			this.args = args.ToArray();

			OpenVR.NET.Events.OnMessage += msg => {
				Schedule( () => Notifications.PostMessage( new SimpleNotification() { Text = msg } ) );
			};
			OpenVR.NET.Events.OnError += msg => {
				Schedule( () => Notifications.PostError( new SimpleNotification() { Text = msg, Icon = FontAwesome.Solid.Bomb } ) );
			};
			OpenVR.NET.Events.OnException += (msg,e) => {
				Schedule( () => Notifications.PostError( new SimpleNotification() { Text = msg + ": " + e.Message, Icon = FontAwesome.Solid.Bomb } ) );
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
						},
						new() {
							Name = XrAction.Grip,
							Type = ActionType.Boolean,
							Requirement = Requirement.Suggested,
							Localizations = new() { [ "en_us" ] = "Grip" }
						},
						new() {
							Name = XrAction.Move,
							Type = ActionType.Boolean,
							Requirement = Requirement.Suggested,
							Localizations = new() { [ "en_us" ] = "Teleport" }
						},
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

		void onControllersMutated () {
			wasInKeyboardProximity = false;
			foreach ( var controller in controllers.Values ) {
				controller.ModeOverrideBindable.Value = ControllerMode.Disabled;
			}

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
				listenForPlayer();
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
			AddFont( Resources, @"Fonts/Torus/Torus-Regular" );
			AddFont( Resources, @"Fonts/Torus/Torus-Light" );
			AddFont( Resources, @"Fonts/Torus/Torus-SemiBold" );
			AddFont( Resources, @"Fonts/Torus/Torus-Bold" );
			AddFont( Resources, @"Fonts/Inter/Inter-Regular" );
			AddFont( Resources, @"Fonts/Inter/Inter-RegularItalic" );
			AddFont( Resources, @"Fonts/Inter/Inter-Light" );
			AddFont( Resources, @"Fonts/Inter/Inter-LightItalic" );
			AddFont( Resources, @"Fonts/Inter/Inter-SemiBold" );
			AddFont( Resources, @"Fonts/Inter/Inter-SemiBoldItalic" );
			AddFont( Resources, @"Fonts/Inter/Inter-Bold" );
			AddFont( Resources, @"Fonts/Inter/Inter-BoldItalic" );
			AddFont( Resources, @"Fonts/Noto/Noto-Basic" );
			AddFont( Resources, @"Fonts/Noto/Noto-Hangul" );
			AddFont( Resources, @"Fonts/Noto/Noto-CJK-Basic" );
			AddFont( Resources, @"Fonts/Noto/Noto-CJK-Compatibility" );
			AddFont( Resources, @"Fonts/Noto/Noto-Thai" );
			AddFont( Resources, @"Fonts/Venera/Venera-Light" );
			AddFont( Resources, @"Fonts/Venera/Venera-Bold" );
			AddFont( Resources, @"Fonts/Venera/Venera-Black" );

			dependency.CacheAs( OsuGame.Dependencies.Get<PreviewTrackManager>() );
			dependency.CacheAs( OsuGame.Dependencies.Get<OsuColour>() );
			dependency.CacheAs( OsuGame.Dependencies.Get<RulesetStore>() );
			dependency.CacheAs( OsuGame.Dependencies.Get<SessionStatics>() );
			dependency.CacheAs( OsuGame.Dependencies.Get<IBindable<WorkingBeatmap>>() );
			dependency.CacheAs( OsuGame.Dependencies.Get<IBindable<RulesetInfo>>() );
			dependency.CacheAs<OsuGameBase>( OsuGame );
			dependency.CacheAs<Framework.Game>( OsuGame );
			dependency.CacheAs( OsuGame.Dependencies.Get<Storage>() );

			dependency.CacheAs( Config = new XrConfigManager( OsuGame.Dependencies.Get<Storage>() ) );
		}

		readonly Bindable<Hand> dominantHandBindable = new( Hand.Auto );
		void osuLoaded () {
			stealOsuDI();

			OsuPanel.SetSource( OsuGame );

			AddInternal( BeatProvider );
			AddInternal( Scene );
			Scene.Add( new SkyBox() );
			Scene.Add( new FloorGrid() );
			Scene.Add( new BeatingScenery() );
			Scene.Add( new Collider {
				Mesh = Mesh.XZPlane( 17, 17 ),
				IsVisible = false,
				PhysicsLayer = GamePhysicsLayer.Floor
			} );
			Scene.Add( Camera );
			Scene.Add( OsuPanel );
			Scene.Add( new HandheldMenu().With( s => s.Panels.AddRange( new FlatPanel[] { new ConfigPanel(), Notifications, Inspector, InputBindings, new ChangelogPanel() } ) ) );
			Scene.Add( Keyboard );
			Keyboard.LoadModel( @".\Resources\keyboard.obj" );
			//Scene.Add( FlatKeyboard );
			//FlatKeyboard.LayoutBindable.Value = KeyboardLayout.Simplified_Fix;
			//FlatKeyboard.LoadModel( @".\Resources\keyboard_flat.obj" );

			Config.BindWith( XrConfigSetting.InputMode, inputModeBindable );
			Config.BindWith( XrConfigSetting.DominantHand, dominantHandBindable );
			inputModeBindable.BindValueChanged( v => {
				onControllersMutated();
			}, true );
			dominantHandBindable.BindValueChanged( v => {
				onControllersMutated();
			} );

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

		PlayerInfo lastPlayer;
		void listenForPlayer () {
			var screens = OsuGame.GetField<OsuScreenStack>();

			screens.ScreenPushed += ( p, n ) => {
				if ( p != null && p == lastPlayer.Player ) {
					onPlayerExit( lastPlayer );
					lastPlayer = default;
				}
				if ( n is Player player ) { // TODO check if some player types should be excluded
					var drawableRuleset = player.GetProperty<DrawableRuleset>();
					var inputManager = drawableRuleset.GetField<PassThroughInputManager>();

					var managerType = inputManager.GetType();
					while ( !managerType.IsGenericType || managerType.GetGenericTypeDefinition() != typeof( RulesetInputManager<> ) ) {
						if ( managerType.BaseType != null ) {
							managerType = managerType.BaseType;
						}
						else {
							return;
						}
					}

					var actionType = managerType.GetGenericArguments()[ 0 ];
					var bindings = inputManager.GetField<KeyBindingContainer>();

					lastPlayer = new PlayerInfo {
						Player = player,
						DrawableRuleset = drawableRuleset,
						InputManager = inputManager,
						RulesetActionType = actionType,
						KeyBindingContainer = bindings,
						Mods = player.GetProperty<Bindable<IReadOnlyList<Mod>>>(),
						Variant = drawableRuleset.GetProperty<int>( nameof( DrawableRuleset<HitObject>.Variant ) )
					};

					onPlayerEntered( lastPlayer );
				}
			};
		}

		InjectedInput injectedInput;
		void onPlayerEntered ( PlayerInfo info ) {
			if ( info.Mods.Value.Any( x => x is ModAutoplay ) ) return;

			info.InputManager.Add( injectedInput = new InjectedInput( InputBindings.GetBindingsForVariant( info.Variant ), info ) );
		}

		void onPlayerExit ( PlayerInfo info ) {
			injectedInput = null;
		}
	}
}
