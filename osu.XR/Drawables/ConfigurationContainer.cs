﻿using Humanizer;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.News;
using osu.XR.Components.Groups;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Drawables {
	public class ConfigurationContainer : CompositeDrawable {
		OsuTextFlowContainer header;
		Drawable headerOffset;
		SearchContainer content;
		SearchTextBox searchTextBox;
		Drawable stickyHeader;
		Drawable stickyHeaderBackground;
		OsuScrollContainer scroll;

		public ConfigurationContainer () {
			Masking = true;

			AddInternal( new Box {
				RelativeSizeAxes = Axes.Both,
				Colour = OsuColour.Gray( 0.05f )
			} );

			AddInternal( scroll = new OsuScrollContainer( Direction.Vertical ) {
				ScrollbarVisible = false,
				RelativeSizeAxes = Axes.Both,
				Children = new Drawable[] {
					new Box {
						RelativeSizeAxes = Axes.Both,
						Colour = OsuColour.Gray( 0.025f )
					},
					new Box {
						RelativeSizeAxes = Axes.X,
						Height = 4,
						Anchor = Anchor.TopCentre,
						Origin = Anchor.TopCentre,
						Colour = OsuColour.Gray( 0.05f )
					},
					new Box {
						RelativeSizeAxes = Axes.X,
						Height = 4,
						Anchor = Anchor.BottomCentre,
						Origin = Anchor.BottomCentre,
						Colour = OsuColour.Gray( 0.05f )
					},
					content = new SearchContainer {
						Direction = FillDirection.Vertical,
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y
					}
				}
			} );

			content.Add( wrap( new FillFlowContainer {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Direction = FillDirection.Vertical,
				Children = new Drawable[] {
					header = new OsuTextFlowContainer( s => s.Font = OsuFont.GetFont( Typeface.Torus, 50 ) ) {
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Margin = new MarginPadding { Left = 15, Right = 15, Bottom = 25, Top = 15 }
					},
					headerOffset = new Box {
						Alpha = 0,
						RelativeSizeAxes = Axes.X,
						AlwaysPresent = true
					}
				}
			}, filterable: false ) );

			searchTextBox = new SearchTextBox {
				RelativeSizeAxes = Axes.X,
				Width = 0.95f,
				Anchor = Anchor.TopCentre,
				Origin = Anchor.TopCentre,
				Margin = new MarginPadding { Vertical = 4 }
			};

			AddInternal( stickyHeader = wrap( CreateStickyHeader( searchTextBox ), filterable: false ) );
			stickyHeaderBackground = ( stickyHeader as Container ).Children[ 0 ];

			searchTextBox.Current.ValueChanged += v => content.SearchTerm = v.NewValue;
		}

		protected virtual Drawable CreateStickyHeader ( SearchTextBox search ) {
			return search;
		}

		Dictionary<Drawable, Drawable> sections = new();
		public void AddSection ( Drawable section ) {
			sections.Add( section, wrap( section ) );
			content.Add( sections[ section ] );
		}

		public FillFlowContainer CreateSection ( params Drawable[] drawables ) {
			var section = new FillFlowContainer {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Direction = FillDirection.Vertical,
				Children = drawables
			};
			AddSection( section );
			return section;
		}

		public void RemoveSection ( Drawable section, bool dispose = false ) {
			sections.Remove( section, out var drawable );
			content.Remove( drawable );
			if ( dispose ) drawable.Dispose();
		}

		public void ClearSections ( IEnumerable<Drawable> sections = null, bool dispose = true ) {
			sections ??= this.sections.Keys;
			foreach ( var i in sections.ToArray() ) {
				RemoveSection( i, dispose );
			}
		}

		protected override void UpdateAfterChildren () {
			base.UpdateAfterChildren();
			stickyHeader.Y = Math.Max( -stickyHeader.Margin.Top - 1, header.LayoutSize.Y - scroll.Current );
			stickyHeaderBackground.Colour = Interpolation.ValueAt( Math.Clamp( scroll.Current, 0, stickyHeader.LayoutSize.Y ), OsuColour.Gray( 0.05f ), OsuColour.Gray( 0.025f ), 0, stickyHeader.LayoutSize.Y, Easing.In );
		}

		protected override void Update () {
			base.Update();
			headerOffset.Height = stickyHeader.LayoutSize.Y;
		}

		string title;
		string description;
		public string Title {
			get => title;
			set {
				title = value;
				Schedule( updateHeader );
			}
		}
		public string Description {
			get => description;
			set {
				description = value;
				Schedule( updateHeader );
			}
		}

		void updateHeader () {
			header.Text = Title;
			header.AddParagraph( Description, s => { s.Font = OsuFont.GetFont( Typeface.Torus, 18 ); s.Colour = Colour4.HotPink; } );
		}

		FilterableContainer wrap ( Drawable other, bool filterable = true ) {
			Drawable[] final;
			var wrapper = new Container {
				Margin = new MarginPadding { Vertical = 6 },
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Child = other
			};
			if ( other is IHasName name ) {
				final = new Drawable[] {
					new OsuSpriteText {
						RelativeSizeAxes = Axes.X,
						Height = 32,
						Margin = new MarginPadding { Left = 15 },
						Font = OsuFont.GetFont( size: 24 ),
						Colour = Colour4.Yellow,
						Text = name.DisplayName
					},
					wrapper
				};
			}
			else {
				final = new[] { wrapper };
			}

			return new FilterableContainer {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Children = new Drawable[] {
					new Box {
						RelativeSizeAxes = Axes.Both,
						Colour = OsuColour.Gray( 0.05f )
					},
					new FillFlowContainer {
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Direction = FillDirection.Vertical,
						Margin = new MarginPadding { Vertical = 10 },

						Children = final
					}
				},
				Margin = new MarginPadding { Vertical = 1 },
				Masking = true,
				CornerRadius = 5,

				FilterTerms = ( other is IHasName named ) ? new[] { named.DisplayName } : Array.Empty<string>(),
				CanBeFiltered = filterable
			};
		}
	}

	public class FilterableContainer : Container, IFilterable {
		public IEnumerable<string> FilterTerms { get; set; }
		public bool MatchingFilter {
			set {
				if ( !CanBeFiltered ) return;

				if ( value ) Show();
				else Hide();
			}
		}

		public override void Hide () {
			this.ScaleTo( new Vector2( 1, 0 ), 100, Easing.Out );
			this.Delay( 50 ).FadeOut( 50 );
		}

		public override void Show () {
			this.ScaleTo( new Vector2( 1, 1 ), 100, Easing.Out );
			this.FadeIn( 50 );
		}

		public bool FilteringActive { get; set; }
		public bool CanBeFiltered = true;
	}
}