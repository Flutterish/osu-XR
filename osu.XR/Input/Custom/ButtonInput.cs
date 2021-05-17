using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osu.XR.Input.Custom.Components;
using osu.XR.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Input.Custom {
	public class ButtonInput : CustomInput {
		public Hand Hand { get; init; } = Hand.Auto;
		public override string Name => $"{Hand} Buttons";

		ButtonsBindingHandler handler;
		ButtonsBindingHandler Handler {
			get {
				if ( handler is null ) {
					AddInternal( handler = new( Hand ) );
				}
				return handler;
			}
		}
		public override ButtonsBindingHandler CreateHandler () {
			var handler = new ButtonsBindingHandler( Hand );

			handler.primaryBinding.RulesetAction.BindTo( Handler.primaryBinding.RulesetAction );
			handler.secondaryBinding.RulesetAction.BindTo( Handler.secondaryBinding.RulesetAction );

			return handler;
		}

		protected override Drawable CreateSettingDrawable ()
			=> new ButtonBindingSettings( Handler );
	}

	public class ButtonsBindingHandler : CustomRulesetInputBindingHandler {
		public readonly RulesetActionBinding primaryBinding = new();
		public readonly RulesetActionBinding secondaryBinding = new();

		BoundComponent<ControllerButton, bool> primary;
		BoundComponent<ControllerButton, bool> secondary;
		public ButtonsBindingHandler ( Hand hand ) {
			AddInternal( primary = new( XrAction.MouseLeft, x => x.Role == OsuGameXr.RoleForHand( hand ) ) );
			AddInternal( secondary = new( XrAction.MouseRight, x => x.Role == OsuGameXr.RoleForHand( hand ) ) );

			primaryBinding.IsActive.BindTo( primary.Current );
			secondaryBinding.IsActive.BindTo( secondary.Current );

			primaryBinding.Press += TriggerPress;
			secondaryBinding.Press += TriggerPress;
			primaryBinding.Release += TriggerRelease;
			secondaryBinding.Release += TriggerRelease;
		}
	}

	public class ButtonBindingSettings : FillFlowContainer {
		public ButtonBindingSettings ( ButtonsBindingHandler handler ) {
			ButtonSetup primary;
			ButtonSetup secondary;

			Direction = FillDirection.Vertical;
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Children = new Drawable[] {
				primary = new ButtonSetup( true ),
				secondary = new ButtonSetup( false )
			};

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
