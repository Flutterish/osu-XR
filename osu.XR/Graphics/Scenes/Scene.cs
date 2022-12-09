using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;

namespace osu.XR.Graphics.Scenes;

public partial class Scene : CompositeDrawable3D {
	public Scene () {
		AddInternal( new BasicModel {
			IsColliderEnabled = true,
			IsVisible = false,
			Mesh = BasicMesh.UnitCube,
			Scale = new( 100, 0, 100 )
		} );
	}
}
