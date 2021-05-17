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
		}
	}

	public class JoystickBindingSettings : FillFlowContainer {
		OsuButton addButton;
		SettingsDropdown<string> dropdown;
		JoystickBindingHandler handler;

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
					addSetting( dropdown.Current.Value );
				}
			} );

			Add( dropdown = new SettingsDropdown<string> {
				Current = new Bindable<string>( "Select type" )
			} );

			dropdown.Current.BindValueChanged( v => {
				addButton.Enabled.Value = v.NewValue != dropdown.Current.Default;
			}, true );
		}

		List<Drawable> settings = new();
		protected override void Update () {
			base.Update();
			foreach ( var i in settings ) {
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
				dropdown.Items = new string[] { "Zone" }.Prepend( dropdown.Current.Default );
			}
			else {
				dropdown.Items = new string[] { "Zone", "Movement" }.Prepend( dropdown.Current.Default );
			}
			dropdown.Current.SetDefault();
		}

		void removeSetting ( Drawable drawable, bool isMovement ) {
			Remove( drawable );
			settings.Remove( drawable );
			if ( isMovement ) sharedSettings.RemoveAll( x => x is JoystickMovementLock );

			updateDropdown();
		}
		void addSetting ( string type ) {
			if ( type == "Movement" ) sharedSettings.Add( new JoystickMovementLock() );

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
							Children = (new Container {
								RelativeSizeAxes = Axes.X,
								AutoSizeAxes = Axes.Y,
								Margin = new MarginPadding { Bottom = 8 },
								Children = new Drawable[] {
									new OsuTextFlowContainer( x => x.Font = OsuFont.GetFont( size: 24 ) ) {
										Text = type,
										Margin = new MarginPadding { Bottom = 4, Left = 6, Top = 4 },
										RelativeSizeAxes = Axes.X,
										AutoSizeAxes = Axes.Y
									},
									new OsuButton {
										Anchor = Anchor.CentreRight,
										Origin = Anchor.CentreRight,
										Text = "X",
										BackgroundColour = Color4.HotPink,
										Action = () => removeSetting( drawable, type == "Movement" ),
										Width = 25,
										Height = 25
									}
								}
							}).Yield().Concat( type == "Zone" ? makeZone() : makeMovement() ).ToArray()
						}
					}
			};

			IEnumerable<Drawable> makeZone () {
				JoystickZoneVisual visual = new JoystickZoneVisual();
				var zone = style( visual );
				var indicator = new ActivationIndicator { Anchor = Anchor.Centre, Origin = Anchor.Centre };
				yield return zone;
				yield return new Container {
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y,
					Child = indicator
				};
				visual.JoystickPosition.BindTo( handler.joystick.Current );
				visual.IsActive.BindValueChanged( v => indicator.IsActive.Value = v.NewValue );
				yield return new RulesetActionDropdown();
			}

			IEnumerable<Drawable> makeMovement () {
				JoystickVisual visual = new JoystickVisual();
				visual.JoystickPosition.BindTo( handler.joystick.Current );
				yield return style( visual );
				yield return new SettingsDropdown<string> {
					Current = new Bindable<string>( "Absolute" ),
					Items = new string[] {
							"Absolute",
							"Delta"
						}
				};
				yield return new SettingsSlider<double> {
					LabelText = "Distance",
					Current = new BindableDouble( 100 ) { MinValue = 0, MaxValue = 100 }
				};
			}

			Container style ( Drawable d ) {
				d.Size = new Vector2( 300 );
				d.Origin = Anchor.TopCentre;
				d.Anchor = Anchor.TopCentre;

				return new Container {
					Child = d,
					Margin = new MarginPadding { Bottom = 16 },
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y
				};
			}

			settings.Add( drawable );
			Add( drawable );

			updateDropdown();
		}
	}

	public class JoystickSettings : CompositeDrawable {

	}
	public class JoystickHandler : Component {

	}

	public class JoystickZoneSettings : JoystickSettings {

	}
	public class JoystickZoneHandler : JoystickHandler {

	}

	public class JoystickMovementSettings : JoystickSettings {

	}
	public class JoystickMovementHandler : JoystickHandler {

	}
}
