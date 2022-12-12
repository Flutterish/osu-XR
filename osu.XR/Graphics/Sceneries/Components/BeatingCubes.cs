using osu.Framework.XR;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;

namespace osu.XR.Graphics.Sceneries.Components;

public partial class BeatingCubes : CompositeDrawable3D {
	public BeatingCubes ( int? seed = 0 ) {
		Random random = seed.HasValue ? new( seed.Value ) : new();

		double next () => random.NextDouble( 0.02, 0.1 );
		for ( double theta = next(); theta < Math.PI * 2; theta += next() ) {
			if ( random.NextSingle() <= 0.2f ) {
				double radius = random.NextDouble( 4, 7 );
				double x = Math.Cos( theta ) * radius;
				double y = Math.Sin( theta ) * radius;

				AddInternal( new BeatingGroup {
					Position = new Vector3( (float)x, 0, (float)y ),
					Child = new BasicModel {
						Mesh = BasicMesh.UnitCube,
						IsColliderEnabled = true,
						OriginY = -1f,
						Scale = new Vector3( (float)( random.NextDouble( 0.05, 0.2 ) * radius ) ),
						Rotation = Quaternion.FromAxisAngle( Vector3.UnitY, (float)random.NextDouble( 0, Math.PI * 2 ) )
					}
				} );
			}
		}
	}
}
