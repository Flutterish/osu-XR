using osuTK.Graphics;

namespace osu.XR.Graphics.Scenes;

public class GridScene : Scene {
	public readonly Bindable<Color4> TintBindable = new( new Color4( 253, 35, 115, 255 ) );
	public readonly BindableFloat OpacityBindable = new( 1 ) { MinValue = 0, MaxValue = 1 };

	public GridScene () {
		VerticalGradientSkyBox skybox;
		AddInternal( skybox = new VerticalGradientSkyBox() );

		TintBindable.BindTo( skybox.TintBindable );
		OpacityBindable.BindTo( skybox.OpacityBindable );
	}
}
