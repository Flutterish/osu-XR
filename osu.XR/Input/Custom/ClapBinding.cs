using MessagePack.Formatters;
using Newtonsoft.Json.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.XR.Input.Custom.Components;
using osu.XR.Input.Custom.Persistence;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Input.Custom {
	public class ClapBinding : CustomBinding {
		public override string Name => "Clap";

		public readonly BindableDouble ThresholdABindable = new( 0.325 );
		public readonly BindableDouble ThresholdBBindable = new( 0.275 );
		public readonly Bindable<object> Action = new();

		public ClapBinding () {
			ThresholdABindable.ValueChanged += v => OnSettingsChanged();
			ThresholdBBindable.ValueChanged += v => OnSettingsChanged();
			Action.ValueChanged += v => OnSettingsChanged();
		}

		public override CustomBindingHandler CreateHandler ()
			=> new ClapBindingHandler( this );

		public override object CreateSaveData ( SaveDataContext context )
			=> new { 
				Action = context.SaveActionBinding( Action.Value ),
				ThresholdA = ThresholdABindable.Value,
				ThresholdB = ThresholdBBindable.Value
			};

		public override void Load ( JToken data, SaveDataContext context ) {
			Action.Value = context.LoadActionBinding( data, "Action" );
			ThresholdABindable.Value = (double)( data as JObject )[ "ThresholdA" ];
			ThresholdBBindable.Value = (double)( data as JObject )[ "ThresholdB" ];
		}
	}

	public class ClapBindingHandler : CustomBindingHandler {
		public readonly RulesetActionBinding binding = new();

		[Resolved, MaybeNull, NotNull]
		protected OsuGameXr game { get; private set; }

		public readonly BindableDouble ThresholdABindable = new( 0.325 );
		public readonly BindableDouble ThresholdBBindable = new( 0.275 );
		public readonly BindableDouble DistanceBindable = new();
		public readonly BindableDouble ProgressBindable = new();
		const double maxDistance = 0.5;

		public ClapBindingHandler ( ClapBinding backing ) : base( backing ) {
			DistanceBindable.BindValueChanged( v => ProgressBindable.Value = Math.Clamp( v.NewValue / maxDistance, 0, 1 ) );

			ThresholdABindable.BindTo( backing.ThresholdABindable );
			ThresholdBBindable.BindTo( backing.ThresholdBBindable );
			binding.RulesetAction.BindTo( backing.Action );

			binding.Press += TriggerPress;
			binding.Release += TriggerRelease;
		}

		void updateActivation () {
			if ( binding.IsActive.Value )
				binding.IsActive.Value = ProgressBindable.Value < Math.Max( ThresholdABindable.Value, ThresholdBBindable.Value );
			else
				binding.IsActive.Value = ProgressBindable.Value < Math.Min( ThresholdABindable.Value, ThresholdBBindable.Value );
		}

		protected override void Update () {
			base.Update();

			if ( game.SecondaryController != null ) {
				DistanceBindable.Value = ( game.MainController.Position - game.SecondaryController.Position ).Length;
			}
			else {
				DistanceBindable.Value = 0;
			}

			updateActivation();
		}

		public override CustomBindingDrawable CreateSettingsDrawable ()
			=> new ClapBindingDrawable( this );
	}

	public class ClapBindingDrawable : CustomBindingDrawable {
		Container thresholdBar;
		Drawable fill;
		RulesetActionDropdown dropdown;
		ThresholdBar thresholdA;
		ThresholdBar thresholdB;
		ActivationIndicator indicator;
		public ClapBindingDrawable ( ClapBindingHandler handler ) : base( handler ) {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			InternalChild = new FillFlowContainer {
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

			handler.ProgressBindable.BindValueChanged( v => {
				fill.Width = 1 - (float)v.NewValue;
			}, true );
			indicator.IsActive.BindTo( handler.binding.IsActive );

			dropdown.RulesetAction.BindTo( handler.binding.RulesetAction );
			thresholdA.Progress.BindTo( handler.ThresholdABindable );
			thresholdB.Progress.BindTo( handler.ThresholdBBindable );
		}

		protected override void Update () {
			base.Update();
			thresholdBar.Width = thresholdBar.Parent.DrawWidth - 32;
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
