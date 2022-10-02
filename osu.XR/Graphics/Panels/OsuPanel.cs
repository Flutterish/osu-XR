using osu.Framework.XR.Graphics.Materials;
using osu.XR.Graphics.Materials;
using osu.XR.Osu;

namespace osu.XR.Graphics.Panels;

public class OsuPanel : CurvedPanel {
	public readonly OsuGameContainer GameContainer;

	public OsuPanel () {
		RenderStage = RenderingStage.Transparent;
		Content.Add( GameContainer = new() );
	}

	protected override Material GetDefaultMaterial ( MaterialStore materials ) {
		return materials.GetNew( MaterialNames.PanelTransparent );
	}
}
