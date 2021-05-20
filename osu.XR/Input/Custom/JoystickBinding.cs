using Newtonsoft.Json.Linq;
using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.XR.Input.Custom.Components;
using osu.XR.Input.Custom.Persistence;
using osu.XR.Settings;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Input.Custom {
	public class JoystickBinding : CompositeCustomBinding {
		public readonly Hand Hand;
		public JoystickBinding ( Hand hand ) {
			Hand = hand;
		}

		public override string Name => $"{Hand} Joystick";

		public override CompositeCustomBindingHandler CreateHandler ()
			=> new JoystickBindingHandler( this );

		protected override object CreateSaveData ( Dictionary<CustomBinding, object> childrenData )
			=> childrenData.Select( x => new BindingData {
				Type = x.Key is JoystickMovementBinding ? "Movement" : "Zone",
				Data = x.Value
			} ).ToList();

		public override void Load ( JToken data, SaveDataContext context ) {
			Children.Clear();

			foreach ( var i in data.ToObject<List<BindingData>>() ) {
				CustomBinding child;
				if ( i.Type == "Movement" ) {
					Children.Add( child = new JoystickMovementBinding() );
				}
				else {
					Children.Add( child = new JoystickZoneBinding() );
				}
				child.Load( i.Data as JToken, context );
			}
		}
	}

	public class JoystickBindingHandler : CompositeCustomBindingHandler {
		public readonly BoundComponent<Controller2DVector, System.Numerics.Vector2, Vector2> joystick;

		public JoystickBindingHandler ( JoystickBinding backing ) : base( backing ) {
			AddInternal( joystick = new( XrAction.Scroll, x => x?.Role == OsuGameXr.RoleForHand( backing.Hand ), x => new Vector2( x.X, -x.Y ) ) );
		}

		public override CompositeCustomBindingDrawable CreateSettingsDrawable ()
			=> new JoystickBindingDrawable( this );

		protected override void OnChildAdded ( CustomBindingHandler child ) {
			base.OnChildAdded( child );
			( (CustomJoystickBindingHandler)child ).JoystickPosition.BindTo( joystick.Current );
		}
	}

	public class JoystickBindingDrawable : CompositeCustomBindingDrawable {
		OsuButton addButton;
		SettingsDropdown<string> dropdown;
		FillFlowContainer content;

		const string ZONE = "Zone";
		const string MOVEMENT = "Movement";
		static readonly Dictionary<string, Func<CustomBinding>> factory = new() {
			[ ZONE ] = () => new JoystickZoneBinding(),
			[ MOVEMENT ] = () => new JoystickMovementBinding()
		};

		public JoystickBindingDrawable ( JoystickBindingHandler handler ) : base( handler ) {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;

			AddInternal( content = new FillFlowContainer {
				Direction = FillDirection.Vertical,
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Children = new Drawable[] {
					addButton = new OsuButton {
						Height = 25,
						Width = 120,
						Margin = new MarginPadding { Left = 16 },
						Text = "Add",
						Action = () => {
							handler.Sources.Add( factory[ dropdown.Current.Value ]() );
						}
					},
					dropdown = new SettingsDropdown<string> {
						Current = new Bindable<string>( "Select type" )
					}
				}
			} );

			dropdown.Current.BindValueChanged( v => {
				addButton.Enabled.Value = v.NewValue != dropdown.Current.Default;
			}, true );
		}

		protected override void Update () {
			base.Update();
			foreach ( var i in ChildrenMap.Values ) {
				i.Width = DrawWidth - 32;
			}
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			sharedSettings.BindCollectionChanged( ( _, _ ) => {
				updateDropdown();
			}, true );
		}

		private class JoystickMovementLock { }
		[Resolved]
		BindableList<object> sharedSettings { get; set; }

		void updateDropdown () {
			if ( sharedSettings.Any( x => x is JoystickMovementLock ) ) {
				dropdown.Items = new string[] { ZONE }.Prepend( dropdown.Current.Default );
			}
			else {
				dropdown.Items = new string[] { ZONE, MOVEMENT }.Prepend( dropdown.Current.Default );
			}
			dropdown.Current.SetDefault();
		}

		protected override Drawable CreateDrawable ( CustomBindingDrawable settingDrawable ) {
			Drawable drawable = null;
			drawable = new Container {
				Masking = true,
				CornerRadius = 5,
				AutoSizeAxes = Axes.Y,
				Margin = new MarginPadding { Left = 16, Right = 16, Bottom = 4 },
				Children = new Drawable[] {
					new Box {
						RelativeSizeAxes = Axes.Both,
						Colour = OsuColour.Gray( 0.075f )
					},
					new FillFlowContainer {
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Direction = FillDirection.Vertical,
						Children = new Drawable[] {
							new Container {
								RelativeSizeAxes = Axes.X,
								AutoSizeAxes = Axes.Y,
								Margin = new MarginPadding { Bottom = 8 },
								Children = new Drawable[] {
									new OsuTextFlowContainer( x => x.Font = OsuFont.GetFont( size: 24 ) ) {
										Text = settingDrawable.Backing.Name,
										Margin = new MarginPadding { Bottom = 4, Left = 6, Top = 4 },
										RelativeSizeAxes = Axes.X,
										AutoSizeAxes = Axes.Y
									},
									new OsuButton {
										Anchor = Anchor.CentreRight,
										Origin = Anchor.CentreRight,
										Text = "X",
										BackgroundColour = Color4.HotPink,
										Action = () => Handler.Sources.Remove( settingDrawable.Backing ),
										Width = 25,
										Height = 25
									}
								}
							},
							settingDrawable
						}
					}
				}
			};

			return drawable;
		}

		protected override void AddDrawable ( Drawable drawable, CustomBindingHandler source ) {
			content.Add( drawable );

			if ( source.Backing is JoystickMovementBinding ) sharedSettings.Add( new JoystickMovementLock() );
			updateDropdown();
		}

		protected override void RemoveDrawable ( Drawable drawable, CustomBindingHandler source ) {
			content.Remove( drawable );

			if ( source.Backing is JoystickMovementBinding ) sharedSettings.RemoveAll( x => x is JoystickMovementLock );
			updateDropdown();
		}
	}

	public abstract class CustomJoystickBindingHandler : CustomBindingHandler {
		public readonly Bindable<Vector2> JoystickPosition = new();

		protected CustomJoystickBindingHandler ( CustomBinding backing ) : base( backing ) { }
	}
	public abstract class CustomJoystickBindingDrawable : CustomBindingDrawable {
		protected FillFlowContainer Content;
		public CustomJoystickBindingDrawable ( CustomJoystickBindingHandler handler ) : base( handler ) {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			AddInternal( Content = new FillFlowContainer {
				Direction = FillDirection.Vertical,
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y
			} );
		}
	}

	public class JoystickZoneBinding : CustomBinding {
		public override string Name => "Zone";
		public readonly BindableDouble StartAngle = new( -30 );
		public readonly BindableDouble Arc = new( 60 ) { MinValue = 0, MaxValue = 360 };
		public readonly BindableDouble Deadzone = new( 0.4 ) { MinValue = 0, MaxValue = 1 };
		public readonly Bindable<object> Action = new();

		public override CustomBindingHandler CreateHandler ()
			=> new JoystickZoneBindingHandler( this );

		public JoystickZoneBinding () {
			StartAngle.ValueChanged += v => OnSettingsChanged();
			Arc.ValueChanged += v => OnSettingsChanged();
			Deadzone.ValueChanged += v => OnSettingsChanged();
			Action.ValueChanged += v => OnSettingsChanged();
		}

		public override object CreateSaveData ( SaveDataContext context )
			=> new {
				StartAngle = StartAngle.Value,
				Arc = Arc.Value,
				Deadzone = Deadzone.Value,
				Action = context.SaveActionBinding( Action.Value )
			};

		public override void Load ( JToken data, SaveDataContext context ) {
			Action.Value = context.LoadActionBinding( data, "Action" );
			StartAngle.Value = (double)( data as JObject )[ "StartAngle" ];
			Arc.Value = (double)( data as JObject )[ "Arc" ];
			Deadzone.Value = (double)( data as JObject )[ "Deadzone" ];
		}
	}

	public class JoystickZoneBindingHandler : CustomJoystickBindingHandler {
		public readonly BindableDouble StartAngle = new( -30 );
		public readonly BindableDouble Arc = new( 60 ) { MinValue = 0, MaxValue = 360 };
		public readonly BindableDouble Deadzone = new( 0.4 ) { MinValue = 0, MaxValue = 1 };
		public readonly RulesetActionBinding Binding = new();
		public JoystickZoneBindingHandler ( JoystickZoneBinding backing ) : base( backing ) {
			StartAngle.BindTo( backing.StartAngle );
			Arc.BindTo( backing.Arc );
			Deadzone.BindTo( backing.Deadzone );
			Binding.RulesetAction.BindTo( backing.Action );

			Binding.Press += TriggerPress;
			Binding.Release += TriggerRelease;

			StartAngle.ValueChanged += v => updateActivation();
			Arc.ValueChanged += v => updateActivation();
			Deadzone.ValueChanged += v => updateActivation();
			JoystickPosition.ValueChanged += v => updateActivation();
		}

		double deltaAngle ( double current, double goal ) {
			var diff = ( goal - current ) % 360;
			if ( diff < 0 ) diff += 360;
			if ( diff > 180 ) diff -= 360;

			return diff;
		}
		public bool IsNormalizedPointInside ( Vector2 pos ) {
			if ( pos.Length < Deadzone.Value ) return false;
			if ( pos.Length == 0 ) return true;
			return IsAngleInside( pos );
		}
		public bool IsAngleInside ( Vector2 direction ) {
			var angle = Math.Atan2( direction.Y, direction.X ) / Math.PI * 180;
			angle = deltaAngle( StartAngle.Value, angle );
			if ( angle < 0 ) angle += 360;
			return angle <= Arc.Value;
		}
		void updateActivation () {
			Binding.IsActive.Value = IsNormalizedPointInside( JoystickPosition.Value ); // NOTE this is copied from the visual
		}

		public override CustomBindingDrawable CreateSettingsDrawable ()
			=> new JoystickZoneBindingDrawable( this );
	}

	public class JoystickZoneBindingDrawable : CustomJoystickBindingDrawable {
		JoystickZoneVisual visual;
		RulesetActionDropdown dropdown;
		ActivationIndicator indicator;

		public JoystickZoneBindingDrawable ( JoystickZoneBindingHandler handler ) : base( handler ) {
			Content.AddRange( new Drawable[] {
				new Container {
					Child = visual = new JoystickZoneVisual {
						Size = new Vector2( 300 ),
						Origin = Anchor.TopCentre,
						Anchor = Anchor.TopCentre
					},
					Margin = new MarginPadding { Bottom = 16 },
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y
				},
				new Container {
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y,
					Child = indicator = new ActivationIndicator {
						Anchor = Anchor.Centre,
						Origin = Anchor.Centre
					}
				},
				dropdown = new()
			} );

			visual.JoystickPosition.BindTo( handler.JoystickPosition );
			visual.ZoneStartAngle.BindTo( handler.StartAngle );
			visual.ZoneDeltaAngle.BindTo( handler.Arc );
			visual.DeadzonePercentage.BindTo( handler.Deadzone );

			indicator.IsActive.BindTo( handler.Binding.IsActive );
			dropdown.RulesetAction.BindTo( handler.Binding.RulesetAction );
		}
	}

	public enum JoystickMovementType {
		Absolute,
		Relative
	}
	public class JoystickMovementBinding : CustomBinding {
		public override string Name => "Movement";

		public readonly Bindable<JoystickMovementType> MovementType = new( JoystickMovementType.Absolute );
		public readonly BindableDouble Distance = new( 100 ) { MinValue = 0, MaxValue = 100 };

		public JoystickMovementBinding () {
			MovementType.ValueChanged += v => OnSettingsChanged();
			Distance.ValueChanged += v => OnSettingsChanged();
		}

		public override CustomBindingHandler CreateHandler ()
			=> new JoystickMovementBindingHandler( this );

		public override object CreateSaveData ( SaveDataContext context )
			=> new {
				Type = MovementType.Value.ToString(),
				Distance = Distance.Value
			};

		public override void Load ( JToken data, SaveDataContext context ) {
			MovementType.Value = Enum.Parse<JoystickMovementType>( (string)( data as JObject )[ "Type" ] );
			Distance.Value = (double)( data as JObject )[ "Distance" ];
		}
	}

	public class JoystickMovementBindingHandler : CustomJoystickBindingHandler {
		public readonly Bindable<JoystickMovementType> MovementType = new( JoystickMovementType.Absolute );
		public readonly BindableDouble Distance = new( 100 ) { MinValue = 0, MaxValue = 100 };

		public JoystickMovementBindingHandler ( JoystickMovementBinding backing ) : base( backing ) {
			MovementType.BindTo( backing.MovementType );
			Distance.BindTo( backing.Distance );
		}

		protected override void Update () {
			base.Update();
			if ( MovementType.Value == JoystickMovementType.Absolute ) {
				MoveTo( JoystickPosition.Value * (float)Distance.Value / 100, isNormalized: true );
			}
			else {
				MoveBy( JoystickPosition.Value * (float)(Distance.Value / 100 * Time.Elapsed / 100), isNormalized: true );
			}
		}

		public override CustomBindingDrawable CreateSettingsDrawable ()
			=> new JoystickMovementBindingSettings( this );
	}

	public class JoystickMovementBindingSettings : CustomJoystickBindingDrawable {
		JoystickVisual visual;
		SettingsSlider<double> slider;
		SettingsEnumDropdown<JoystickMovementType> type;

		public JoystickMovementBindingSettings ( JoystickMovementBindingHandler handler ) : base( handler ) {
			Content.AddRange( new Drawable[] {
				new Container {
					Child = visual = new JoystickVisual {
						Size = new Vector2( 300 ),
						Origin = Anchor.TopCentre,
						Anchor = Anchor.TopCentre
					},
					Margin = new MarginPadding { Bottom = 16 },
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y
				},
				type = new() {
					Current = handler.MovementType
				},
				slider = new() {
					LabelText = "Distance",
					Current = handler.Distance
				}
			} );

			visual.JoystickPosition.BindTo( handler.JoystickPosition );
			type.OnLoadComplete += x => {
				type.WarningText = "Current implementation disables all input from outside the ruleset binding section.\nMake sure to bind your other buttons here too.";
			};
		}
	}
}
