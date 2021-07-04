using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.XR.Drawables.Containers {
	public class ExpandableSection : Container {
		OsuTextFlowContainer title;
		CalmOsuAnimatedButton button;
		FillFlowContainer layout;
		FillFlowContainer content;
		Drawable expandableElement;
		SpriteIcon chevron;
		protected override Container<Drawable> Content => content;

		public ExpandableSection () {
			AutoSizeAxes = Axes.Y;

			AddInternal( new Box {
				RelativeSizeAxes = Axes.Both,
				Colour = OsuColour.Gray( 0.025f )
			} );

			AddInternal( layout = new FillFlowContainer {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Direction = FillDirection.Vertical,

				Children = new Drawable[] {
					button = new CalmOsuAnimatedButton {
						Margin = new MarginPadding { Vertical = 2 },
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y
					},
					expandableElement = new Container {
						Masking = true,
						RelativeSizeAxes = Axes.X,
						Child = content = new FillFlowContainer {
							RelativeSizeAxes = Axes.X,
							AutoSizeAxes = Axes.Y,
							Direction = FillDirection.Vertical
						}
					}
				}
			} );

			Masking = true;
			CornerRadius = 5;
			BorderColour = OsuColour.Gray( 0.075f );
			BorderThickness = 2;

			button.AddRange( new Drawable[] {
				new Box {
					Alpha = 0,
					AlwaysPresent = true,
					Height = 25,
					RelativeSizeAxes = Axes.X
				},
				title = new OsuTextFlowContainer( x => x.AllowMultiline = true ) {
					AutoSizeAxes = Axes.Y,
					Anchor = Anchor.CentreLeft,
					Origin = Anchor.CentreLeft,
					TextAnchor = Anchor.CentreLeft,
					Margin = new MarginPadding { Left = 15, Vertical = 2 }
				},
				chevron = new SpriteIcon {
					Icon = FontAwesome.Solid.ChevronDown,
					Origin = Anchor.CentreRight,
					Anchor = Anchor.CentreRight,
					Margin = new MarginPadding { Right = 15 },
					Size = new Vector2( 16 )
				}
			} );

			button.Action = () => {
				IsExpanded.Value = !IsExpanded.Value;
			};

			IsExpanded.BindValueChanged( v => {
				if ( v.NewValue ) {
					this.TransformBindableTo( size, 1, 300, Easing.Out );
					chevron.FadeTo( 1, 100, Easing.Out );
				}
				else {
					this.TransformBindableTo( size, 0, 300, Easing.Out );
					chevron.FadeTo( 0.6f, 100, Easing.Out );
				}
			}, true );
		}

		public readonly BindableBool IsExpanded = new BindableBool( false );
		BindableFloat size = new( 0 );

		public string Title {
			set => title.Text = value;
		}

		protected override void Update () {
			base.Update();
			Width = Parent.DrawWidth - Margin.Left - Margin.Right;
			expandableElement.Height = size.Value * ( content.LayoutSize.Y + 5 );
			title.Width = button.DrawWidth - 60;
		}
	}
}
