using FFmpeg.AutoGen;
using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.XR.Input.Custom.Components;
using osu.XR.Settings;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Input.Custom {
	public class JoystickInput : CustomInput {
		public Hand Hand { get; init; } = Hand.Auto;
		public override string Name => $"{Hand} Joystick";

		protected override Drawable CreateSettingDrawable () {
			return new JoystickBindingSettings( Handler );
		}

		JoystickBindingHandler handler;
		JoystickBindingHandler Handler {
			get {
				if ( handler is null ) {
					AddInternal( handler = new( Hand ) );
				}
				return handler;
			}
		}
		public override JoystickBindingHandler CreateHandler () {
			var handler = new JoystickBindingHandler( Hand );

			handler.Handlers.BindTo( Handler.Handlers );

			return handler;
		}
	}

	public class JoystickBindingHandler : CustomRulesetInputBindingHandler {
		public readonly BoundComponent<Controller2DVector, System.Numerics.Vector2, Vector2> joystick;
		public readonly BindableList<JoystickHandler> Handlers = new();

		public JoystickBindingHandler ( Hand hand ) {
			AddInternal( joystick = new( XrAction.Scroll, x => x.Role == OsuGameXr.RoleForHand( hand ), x => new Vector2( x.X, -x.Y ) ) );
			// TODO use the handlers

			Handlers.BindCollectionChanged( (_,a) => {
				if ( a.Action == NotifyCollectionChangedAction.Add ) {
					if ( a.NewItems is null ) return;
					foreach ( JoystickHandler i in a.NewItems ) {
						i.JoystickPosition.BindTo( joystick.Current );
						AddInternal( i );
					}
				}
				else {
					if ( a.OldItems is null ) return;
					foreach ( JoystickHandler i in a.OldItems ) {
						RemoveInternal( i );
					}
				}
			}, true );
		}
	}

	public class JoystickBindingSettings : FillFlowContainer {
		OsuButton addButton;
		SettingsDropdown<string> dropdown;
		JoystickBindingHandler handler;

		const string ZONE = "Zone";
		const string MOVEMENT = "Movement";
		static readonly Dictionary<string, Func<JoystickHandler>> factory = new() {
			[ ZONE ] = () => new JoystickZoneHandler(),
			[ MOVEMENT ] = () => new JoystickMovementHandler()
		};

		public JoystickBindingSettings ( JoystickBindingHandler handler ) {
			this.handler = handler;

			Direction = FillDirection.Vertical;
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Add( addButton = new OsuButton {
				Height = 25,
				Width = 120,
				Margin = new MarginPadding { Left = 16 },
				Text = "Add",
				Action = () => {
					handler.Handlers.Add( factory[ dropdown.Current.Value ]() );
				}
			} );

			Add( dropdown = new SettingsDropdown<string> {
				Current = new Bindable<string>( "Select type" )
			} );

			dropdown.Current.BindValueChanged( v => {
				addButton.Enabled.Value = v.NewValue != dropdown.Current.Default;
			}, true );
		}

		Dictionary<JoystickHandler, Drawable> settings = new();
		protected override void Update () {
			base.Update();
			foreach ( var i in settings.Values ) {
				i.Width = DrawWidth - 32;
			}
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			sharedSettings.BindCollectionChanged( ( _, _ ) => {
				updateDropdown();
			}, true );

			handler.Handlers.BindCollectionChanged( (_,b) => {
				if ( b.Action is NotifyCollectionChangedAction.Add ) {
					if ( b.NewItems is null ) return;
					foreach ( JoystickHandler i in b.NewItems ) addSetting( i );
				}
				else {
					if ( b.OldItems is null ) return;
					foreach ( JoystickHandler i in b.OldItems ) removeSetting( i );
				}
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

		void removeSetting ( JoystickHandler handler ) {
			Remove( settings[ handler ] );
			settings.Remove( handler );

			if ( handler is JoystickMovementHandler ) sharedSettings.RemoveAll( x => x is JoystickMovementLock );

			updateDropdown();
		}
		void addSetting ( JoystickHandler handler ) {
			if ( handler is JoystickMovementHandler ) sharedSettings.Add( new JoystickMovementLock() );

			var setting = handler.CreateSettings();

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
										Text = setting.LabelText,
										Margin = new MarginPadding { Bottom = 4, Left = 6, Top = 4 },
										RelativeSizeAxes = Axes.X,
										AutoSizeAxes = Axes.Y
									},
									new OsuButton {
										Anchor = Anchor.CentreRight,
										Origin = Anchor.CentreRight,
										Text = "X",
										BackgroundColour = Color4.HotPink,
										Action = () => removeSetting( handler ),
										Width = 25,
										Height = 25
									}
								}
							},
							setting
						}
					}
				}
			};

			settings.Add( handler, drawable );
			Add( drawable );

			updateDropdown();
		}
	}

	public abstract class JoystickSettings : FillFlowContainer {
		public JoystickSettings () {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Direction = FillDirection.Vertical;
		}

		public abstract string LabelText { get; }
	}
	public abstract class JoystickHandler : Component {
		public readonly Bindable<Vector2> JoystickPosition = new();
		[Resolved]
		public CustomRulesetInputBindingHandler Handler { get; private set; }

		public abstract JoystickSettings CreateSettings ();
	}

	public class JoystickZoneSettings : JoystickSettings {
		public override string LabelText => "Zone";

		public JoystickZoneSettings ( JoystickZoneHandler handler ) {
			JoystickZoneVisual visual;
			RulesetActionDropdown dropdown;
			ActivationIndicator indicator;

			Children = new Drawable[] {
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
			};

			visual.JoystickPosition.BindTo( handler.JoystickPosition );
			visual.ZoneStartAngle.BindTo( handler.StartAngle );
			visual.ZoneDeltaAngle.BindTo( handler.Arc );
			visual.DeadzonePercentage.BindTo( handler.Deadzone );

			indicator.IsActive.BindTo( handler.Binding.IsActive );
			dropdown.RulesetAction.BindTo( handler.Binding.RulesetAction );
		}
	}
	public class JoystickZoneHandler : JoystickHandler {
		public readonly RulesetActionBinding Binding = new();
		public readonly BindableDouble StartAngle = new( -30 );
		public readonly BindableDouble Arc = new( 60 ) { MinValue = 0, MaxValue = 360 };
		public readonly BindableDouble Deadzone = new( 0.4 ) { MinValue = 0, MaxValue = 1 };

		public override JoystickSettings CreateSettings () => new JoystickZoneSettings( this );

		public JoystickZoneHandler () {
			StartAngle.ValueChanged += v => updateActivation();
			Arc.ValueChanged += v => updateActivation();
			Deadzone.ValueChanged += v => updateActivation();
			JoystickPosition.ValueChanged += v => updateActivation();
		}

		protected override void LoadComplete () {
			base.LoadComplete();

			Binding.Press += Handler.TriggerPress;
			Binding.Release += Handler.TriggerRelease;
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
	}

	public enum JoystickMovementType {
		Absolute,

		[System.ComponentModel.Description( "Delta (TBD)" )]
		Delta
	}
	public class JoystickMovementSettings : JoystickSettings {
		public override string LabelText => "Movement";

		public JoystickMovementSettings ( JoystickMovementHandler hander ) {
			JoystickVisual visual;
			SettingsSlider<double> slider;
			SettingsEnumDropdown<JoystickMovementType> type;

			Children = new Drawable[] {
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
					Current = hander.MovementType
				},
				slider = new() {
					LabelText = "Distance",
					Current = hander.Distance
				}
			};

			visual.JoystickPosition.BindTo( hander.JoystickPosition );
		}
	}
	public class JoystickMovementHandler : JoystickHandler {
		public readonly Bindable<JoystickMovementType> MovementType = new( JoystickMovementType.Absolute );
		public readonly BindableDouble Distance = new( 100 ) { MinValue = 0, MaxValue = 100 };

		protected override void LoadComplete () {
			base.LoadComplete();

			JoystickPosition.BindValueChanged( v => {
				if ( MovementType.Value == JoystickMovementType.Absolute ) {
					Handler.MoveToAbsolute( JoystickPosition.Value * (float)Distance.Value / 100, isNormalized: true );
				}
				else {
					// TODO delta
				}
			} );
		}

		public override JoystickSettings CreateSettings () => new JoystickMovementSettings( this );
	}
}
