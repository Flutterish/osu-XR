using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.XR.Graphics.Bindings;

namespace osu.XR.Graphics.Panels.Menu;

public partial class BindingsPanel : MenuPanel {
	public readonly RulesetBindingsSection Section;

	public BindingsPanel () {
		Content.Add( new Box {
			RelativeSizeAxes = Axes.Both,
			Colour = ColourProvider.Background4
		} );
		Content.Add( new OsuScrollContainer {
			Masking = true,
			RelativeSizeAxes = Axes.Both,
			ScrollbarVisible = false,
			Child = new FillFlowContainer {
				Direction = FillDirection.Vertical,
				AutoSizeAxes = Axes.Y,
				RelativeSizeAxes = Axes.X,
				Children = new Drawable[] {
					Section = new(),
					new ThankYouFooter()
				}
			}
		} );
	}
}
