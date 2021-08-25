using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.XR.Drawables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace osu.XR.Panels.Overlays {
	public class FileSelectionOverlay : PanelOverlay { // TODO better design for this
		FileHierarchyViewWithPreview hiererchy;
		OsuTextFlowContainer selected;
		FillFlowContainer header;

		BindableList<HierarchyStep<string>> selectedSteps = new();

		public event Action<IEnumerable<string>> Confirmed;
		public event Action Cancelled;

		public FileSelectionOverlay () {
			FillFlowContainer container;
			AddInternal( new OsuScrollContainer( Direction.Vertical ) {
				RelativeSizeAxes = Axes.Both,
				ScrollbarVisible = false,
				Child = container = new FillFlowContainer {
					AutoSizeAxes = Axes.Y,
					RelativeSizeAxes = Axes.X,
					Direction = FillDirection.Vertical
				}
			} );

			CalmOsuAnimatedButton buttonA;
			CalmOsuAnimatedButton buttonB;

			container.Add( header = new FillFlowContainer {
				AutoSizeAxes = Axes.Y,
				RelativeSizeAxes = Axes.X,
				Direction = FillDirection.Horizontal,
				Margin = new MarginPadding { Left = 15, Vertical = 20 },
				Children = new Drawable[] {
					buttonA = new CalmOsuAnimatedButton {
						Width = 100,
						Height = 20,
						Action = () => {
							Confirmed?.Invoke( selectedSteps.Select( x => x.Value ).ToArray() );
							Confirmed = null;
							Cancelled = null;
							Hide();
						}
					},
					buttonB = new CalmOsuAnimatedButton {
						Width = 100,
						Height = 20,
						Action = () => {
							Hide();
						}
					}
				}
			} );

			buttonA.Add( new Box {
				RelativeSizeAxes = Axes.Both,
				Alpha = 0.6f,
				Colour = Colour4.HotPink
			} );
			buttonB.Add( new Box {
				RelativeSizeAxes = Axes.Both,
				Alpha = 0.6f,
				Colour = Colour4.HotPink
			} );

			buttonA.Add( new OsuSpriteText {
				Origin = Anchor.Centre,
				Anchor = Anchor.Centre,
				UseFullGlyphHeight = true,
				Text = "Confirm"
			} );
			buttonB.Add( new OsuSpriteText {
				Origin = Anchor.Centre,
				Anchor = Anchor.Centre,
				UseFullGlyphHeight = true,
				Text = "Cancel"
			} );

			container.Add( selected = new OsuTextFlowContainer {
				AutoSizeAxes = Axes.Y,
				RelativeSizeAxes = Axes.X,
				Width = 0.9f,
				Origin = Anchor.TopCentre,
				Anchor = Anchor.TopCentre
			} );

			container.Add( hiererchy = new FileHierarchyViewWithPreview() {
				IsMultiselect = true,
				Margin = new Framework.Graphics.MarginPadding { Vertical = 20 }
			} );

			selectedSteps.BindTo( hiererchy.MultiselectSelection );

			selectedSteps.BindCollectionChanged( (e,o) => {
				selected.Clear();
				selected.AddText( "Selected: ", s => s.Colour = Colour4.HotPink );
				if ( selectedSteps.Any() ) {
					selected.AddText( string.Join( ", ", selectedSteps.Select( x => Path.GetFileName( x.Value ) ) ) );
				}
				else {
					selected.AddText( "Nothing" );
				}
			}, true );
		}

		public override void Hide () {
			Cancelled?.Invoke();
			Confirmed = null;
			Cancelled = null;
			selectedSteps.Clear();
			base.Hide();
		}
	}
}
