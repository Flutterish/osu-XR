using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Framework.XR.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.XR.Components.Groups;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.XR.Drawables.Containers {
	public class ConfigurationContainer : CompositeDrawable {
		OsuTextFlowContainer header;
		Drawable headerOffset;
		protected readonly AdvancedSearchContainer<string> Content;
		protected readonly SearchTextBox SearchTextBox;
		Drawable stickyHeader;
		Drawable stickyHeaderBackground;
		Drawable titleBackground;
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
					Content = new AdvancedSearchContainer<string> {
						Direction = FillDirection.Vertical,
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						RecursionMode = RecursiveFilterMode.ChildrenFirst
					}
				}
			} );

			Drawable title;
			Content.Add( title = wrap( new FillFlowContainer {
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

			SearchTextBox = new SearchTextBox {
				RelativeSizeAxes = Axes.X,
				Width = 0.95f,
				Anchor = Anchor.TopCentre,
				Origin = Anchor.TopCentre,
				Margin = new MarginPadding { Vertical = 4 }
			};

			AddInternal( stickyHeader = wrap( CreateStickyHeader( SearchTextBox ), filterable: false ) );
			stickyHeaderBackground = ( stickyHeader as Container ).Children[ 0 ];
			titleBackground = ( title as Container ).Children[ 0 ];

			SearchTextBox.Current.Value = "";
			SearchTextBox.Current.ValueChanged += v => Content.SearchTerm = v.NewValue;
		}

		protected virtual Drawable CreateStickyHeader ( SearchTextBox search ) {
			return search;
		}

		Dictionary<Drawable, Drawable> sections = new();
		public void AddSection ( Drawable section, bool filterable = true, string name = null ) {
			sections.Add( section, wrap( section, filterable: filterable, name: name ) );
			Content.Add( sections[ section ] );
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
			Content.Remove( drawable );
			if ( dispose ) drawable.Dispose();
			else ( section.Parent as Container ).Remove( section );
		}

		public void ClearSections ( bool dispose = true ) {
			ClearSections( sections.Keys, dispose );
		}
		public void ClearSections ( IEnumerable<Drawable> sections, bool dispose = true ) {
			foreach ( var i in sections.ToArray() ) {
				RemoveSection( i, dispose );
			}
		}
		public void ClearSections ( Func<Drawable, bool> sections, bool dispose = true ) {
			foreach ( var i in this.sections.Keys.Where( sections ).ToArray() ) {
				RemoveSection( i, dispose );
			}
		}

		protected override void UpdateAfterChildren () {
			base.UpdateAfterChildren();
			stickyHeader.Y = Math.Max( -stickyHeader.Margin.Top - 1, header.LayoutSize.Y - scroll.Current );
			stickyHeaderBackground.Colour = titleBackground.Colour
				= Interpolation.ValueAt( Math.Clamp( scroll.Current, 0, stickyHeader.LayoutSize.Y ), OsuColour.Gray( 0.05f ), OsuColour.Gray( 0.025f ), 0, stickyHeader.LayoutSize.Y, Easing.In );
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

		FilterableContainer wrap ( Drawable other, bool filterable = true, string name = null ) {
			Drawable[] final;
			var wrapper = new FilterableContainer {
				Margin = new MarginPadding { Vertical = 6 },
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Child = other,
				CanBeFiltered = filterable
			};
			if ( other is IHasName hasName ) {
				name ??= hasName.DisplayName;
			}

			if ( name is not null ) {
				final = new Drawable[] {
					new OsuSpriteText {
						RelativeSizeAxes = Axes.X,
						Height = 32,
						Margin = new MarginPadding { Left = 15 },
						Font = OsuFont.GetFont( size: 24 ),
						Colour = Colour4.Yellow,
						Text = name
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
					new FilterableFillFlowContainer {
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Direction = FillDirection.Vertical,
						Margin = new MarginPadding { Vertical = 10 },

						Children = final,
						CanBeFiltered = filterable
					}
				},
				Margin = new MarginPadding { Vertical = 1 },
				Masking = true,
				CornerRadius = 5,

				FilterTerms = name is not null ? new[] { name } : Array.Empty<string>(),
				CanBeFiltered = filterable
			};
		}
	}
}
