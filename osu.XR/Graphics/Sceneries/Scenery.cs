using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.XR.Graphics.Sceneries.Components;

namespace osu.XR.Graphics.Sceneries;

public partial class Scenery : CompositeDrawable3D {
	public Scenery () {
		AddInternal( new BasicModel {
			IsColliderEnabled = true,
			IsVisible = false,
			Mesh = BasicMesh.UnitCornerCube,
			Scale = new( 100, 0.01f, 100 )
		} );

		Components.BindCollectionChanged( (_, e) => {
			if ( e.OldItems != null ) {
				foreach ( ISceneryComponent i in e.OldItems ) {
					if ( i is Drawable3D drawable )
						RemoveInternal( drawable, disposeImmediately: false );
				}
			}
			if ( e.NewItems != null ) {
				foreach ( ISceneryComponent i in e.NewItems ) {
					if ( i is Drawable3D drawable )
						AddInternal( drawable );
				}
			}
		} );
	}

	public readonly BindableList<ISceneryComponent> Components = new();
	public void AddComponent ( ISceneryComponent component ) {
		Components.Add( component );
	}

	public void RemoveComponent ( ISceneryComponent component ) {
		Components.Remove( component );
	}
}
