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

		protected override Drawable CreateSettingDrawable () {
			Drawable SetupButton ( bool isPrimary ) {
				Circle indicator = null;
				var drawable = new FillFlowContainer {
					Direction = FillDirection.Vertical,
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y,
					Children = new Drawable[] {
						new Container {
							RelativeSizeAxes = Axes.X,
							AutoSizeAxes = Axes.Y,
							Children = new Drawable[] {
								new OsuSpriteText { Text = $"{(isPrimary ? "Primary" : "Secondary")} Button", Margin = new MarginPadding { Left = 16 } },
								indicator = new Circle {
									Colour = Colour4.HotPink.Darken( 0.9f ),
									Size = new osuTK.Vector2( 30, 16 ),
									Margin = new MarginPadding { Right = 16 },
									Origin = Anchor.CentreRight,
									Anchor = Anchor.CentreRight
								}
							}
						},
						new RulesetActionDropdown()
					}
				};

				void lookForValidController ( Controller controller ) {
					if ( controller.Role != OsuGameXr.RoleForHand( Hand ) ) return;

					var comp = VR.GetControllerComponent<ControllerButton>( isPrimary ? XrAction.MouseLeft : XrAction.MouseRight, controller );
					comp.BindValueChanged( v => { // TODO remove on disposal
						if ( v ) {
							indicator.FadeColour( Colour4.HotPink );
							indicator.FlashColour( Colour4.White, 200 );
							indicator.ResizeWidthTo( 45, 100, Easing.Out );
						}
						else {
							indicator.FadeColour( Colour4.HotPink.Darken( 0.9f ), 200 );
							indicator.ResizeWidthTo( 30, 100, Easing.Out );
						}
					}, true );
				}

				VR.BindComponentsLoaded( () => {
					VR.BindNewControllerAdded( lookForValidController, true );
				} );
				return drawable;
			}
			
			return new FillFlowContainer {
				Direction = FillDirection.Vertical,
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Children = new Drawable[] {
					SetupButton( true ),
					SetupButton( false )
				}
			};
		}
	}
}
