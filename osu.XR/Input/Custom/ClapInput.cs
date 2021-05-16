using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.XR.Input.Custom.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Input.Custom {
	public class ClapInput : CustomInput {
		public override string Name => "Clap";

		protected override Drawable CreateSettingDrawable () {
			Drawable fill = null;
			SettingsDropdown<string> dropdown = null;
			ThresholdBar thresholdA = null;
			ThresholdBar thresholdB = null;
			ActivationIndicator indicator = null;
			Container thresholdBar = null;
			bool isActive = false;
			Distance.BindValueChanged( v => {
				var progress = (float)Math.Clamp( v.NewValue / maxDistance, 0, 1 );
				fill.Width = 1 - progress;

				if ( isActive && progress > Math.Max( thresholdA.Progress.Value, thresholdB.Progress.Value ) ) {
					isActive = false;
				}
				else if ( !isActive && progress < Math.Min( thresholdA.Progress.Value, thresholdB.Progress.Value ) ) {
					isActive = true;
				}
				indicator.IsActive.Value = isActive;
			} );

			OnUpdate += x => {
				thresholdBar.Width = thresholdBar.Parent.DrawWidth - 32;
			};

			var drawable = new FillFlowContainer {
				Direction = FillDirection.Vertical,
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Children = new Drawable[] {
					new Container {
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Child = indicator = new() { 
							Anchor = Anchor.Centre, 
							Origin = Anchor.Centre,
							Margin = new MarginPadding { Bottom = 6 }
						}
					},
					thresholdBar = new Container {
						Height = 32,
						Masking = true,
						CornerRadius = 5,
						Margin = new MarginPadding { Left = 16, Right = 16 },
						Children = new Drawable[] {
							new Box {
								RelativeSizeAxes = Axes.Both,
								Colour = Colour4.Cyan
							},
							fill = new Box {
								Colour = Colour4.Orange,
								RelativeSizeAxes = Axes.Both
							},
							thresholdA = new(),
							thresholdB = new()
						}
					},
					dropdown = new RulesetActionDropdown()
				}
			};

			thresholdA.Progress.Value = 0.325;
			thresholdB.Progress.Value = 0.275;

			return drawable;
		}

		public readonly BindableDouble Distance = new();
		double maxDistance = 0.5;
		protected override void Update () {
			base.Update();

			if ( game.SecondaryController != null ) {
				Distance.Value = ( game.MainController.Position - game.SecondaryController.Position ).Length;
			}
			else {
				Distance.Value = 0;
			}
		}
	}

	public class ThresholdBar : CompositeDrawable {
		public readonly BindableDouble Progress = new() { MinValue = 0, MaxValue = 1 };
		Circle child;

		public ThresholdBar () {
			RelativePositionAxes = Axes.X;
			AutoSizeAxes = Axes.Both;
			Anchor = Anchor.CentreLeft;
			Origin = Anchor.Centre;
			
			AddInternal( child = new Circle {
				Colour = Colour4.HotPink,
				Height = 35,
				Width = 14,
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft
			} );
			AddInternal( new HoverClickSounds() );
			
			Progress.BindValueChanged( v => {
				this.MoveToX( 1 - (float)v.NewValue, 50 );
			} );
		}

		protected override bool OnHover ( HoverEvent e ) {
			child.ScaleTo( 1.2f, 100, Easing.Out );
			return base.OnHover( e );
		}
		protected override void OnHoverLost ( HoverLostEvent e ) {
			child.ScaleTo( 1, 100, Easing.Out );
			base.OnHoverLost( e );
		}

		protected override bool OnDragStart ( DragStartEvent e ) {
			return true;
		}

		protected override void OnDrag ( DragEvent e ) {
			base.OnDrag( e );

			Progress.Value = 1 - e.MousePosition.X / Parent.DrawWidth;
		}
	}
}
