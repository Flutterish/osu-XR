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
			Child = Section = new() {
				Margin = new() { Bottom = PREFFERED_CONTENT_HEIGHT - 100 }
			}
		} );
	}
}
