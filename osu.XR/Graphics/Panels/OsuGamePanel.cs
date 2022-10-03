using osu.Framework.XR.Graphics.Materials;
using osu.XR.Osu;
using MaterialNames = osu.XR.Graphics.Materials.MaterialNames;

namespace osu.XR.Graphics.Panels;

public class OsuGamePanel : CurvedPanel {
	public readonly OsuGameContainer GameContainer;
	public OsuDependencies OsuDependencies => GameContainer.OsuDependencies;

	public OsuGamePanel () {
		RenderStage = RenderingStage.Transparent;
		Content.Add( GameContainer = new() );
	}

	protected override Material GetDefaultMaterial ( MaterialStore materials ) {
		return materials.GetNew( MaterialNames.PanelTransparent );
	}
}
