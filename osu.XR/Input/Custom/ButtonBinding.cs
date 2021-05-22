using Newtonsoft.Json.Linq;
using OpenVR.NET.Manifests;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.XR.Input.Custom.Components;
using osu.XR.Input.Custom.Persistence;
using osu.XR.Settings;

namespace osu.XR.Input.Custom {
	public class ButtonBinding : CustomBinding {
		public readonly Hand Hand;
		public override string Name => $"{Hand} Buttons";
		public ButtonBinding ( Hand hand ) {
			Hand = hand;
			PrimaryAction.ValueChanged += v => OnSettingsChanged();
			SecondaryAction.ValueChanged += v => OnSettingsChanged();
		}

		public readonly Bindable<object> PrimaryAction = new();
		public readonly Bindable<object> SecondaryAction = new();
		public override CustomBindingHandler CreateHandler ()
			=> new ButtonBindingHandler( this );

		public override object CreateSaveData ( SaveDataContext context )
			=> new {
				Primary = context.SaveActionBinding( PrimaryAction.Value ),
				Secondary = context.SaveActionBinding( SecondaryAction.Value ),
			};

		public override void Load ( JToken data, SaveDataContext context ) {
			PrimaryAction.Value = context.LoadActionBinding( data, "Primary" );
			SecondaryAction.Value = context.LoadActionBinding( data, "Secondary" );
		}
	}

	public class ButtonBindingHandler : CustomBindingHandler {
		public readonly RulesetActionBinding primaryBinding = new();
		public readonly RulesetActionBinding secondaryBinding = new();

		BoundComponent<ControllerButton, bool> primary;
		BoundComponent<ControllerButton, bool> secondary;
		public ButtonBindingHandler ( ButtonBinding backing ) : base( backing ) {
			AddInternal( primary = new( XrAction.MouseLeft, x => x?.Role == OsuGameXr.RoleForHand( backing.Hand ) ) );
			AddInternal( secondary = new( XrAction.MouseRight, x => x?.Role == OsuGameXr.RoleForHand( backing.Hand ) ) );

			primaryBinding.IsActive.BindTo( primary.Current );
			secondaryBinding.IsActive.BindTo( secondary.Current );
			primaryBinding.RulesetAction.BindTo( backing.PrimaryAction );
			secondaryBinding.RulesetAction.BindTo( backing.SecondaryAction );

			primaryBinding.Press += TriggerPress;
			secondaryBinding.Press += TriggerPress;
			primaryBinding.Release += TriggerRelease;
			secondaryBinding.Release += TriggerRelease;
		}

		public override CustomBindingDrawable CreateSettingsDrawable ()
			=> new ButtonBindingDrawable( this );
	}

	public class ButtonBindingDrawable : CustomBindingDrawable {
		ButtonSetup primary;
		ButtonSetup secondary;

		public ButtonBindingDrawable ( ButtonBindingHandler handler ) : base( handler ) {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;

			AddInternal( new FillFlowContainer {
				Direction = FillDirection.Vertical,
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Children = new Drawable[] {
					primary = new ButtonSetup( true ),
					secondary = new ButtonSetup( false )
				}
			} );

			primary.Indicator.IsActive.BindTo( handler.primaryBinding.IsActive );
			primary.ActionDropdown.RulesetAction.BindTo( handler.primaryBinding.RulesetAction );
			secondary.Indicator.IsActive.BindTo( handler.secondaryBinding.IsActive );
			secondary.ActionDropdown.RulesetAction.BindTo( handler.secondaryBinding.RulesetAction );
		}

		private class ButtonSetup : FillFlowContainer {
			public ActivationIndicator Indicator;
			public RulesetActionDropdown ActionDropdown;
			public ButtonSetup ( bool isPrimary ) {
				Direction = FillDirection.Vertical;
				RelativeSizeAxes = Axes.X;
				AutoSizeAxes = Axes.Y;
				Children = new Drawable[] {
					new Container {
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Children = new Drawable[] {
							new OsuSpriteText { Text = $"{(isPrimary ? "Primary" : "Secondary")} Button", Margin = new MarginPadding { Left = 16 } },
							Indicator = new ActivationIndicator {
								Margin = new MarginPadding { Right = 16 },
								Origin = Anchor.CentreRight,
								Anchor = Anchor.CentreRight
							}
						}
					},
					ActionDropdown = new RulesetActionDropdown()
				};
			}
		}
	}
}
