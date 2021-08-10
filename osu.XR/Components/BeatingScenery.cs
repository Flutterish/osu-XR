using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;
using osu.XR.Components.Groups;
using osu.XR.Editor;
using osuTK;
using System;

namespace osu.XR.Components {
	public class BeatingScenery : CompositeDrawable3D {
		public BeatingScenery ( int? seed = 0 ) {
			Random random = seed.HasValue ? new( seed.Value ) : new();

			double next () => random.NextDouble( 0.02, 0.1 );
			for ( double theta = next(); theta < Math.PI * 2; theta += next() ) {
				if ( random.Chance( 0.2 ) ) {
					double radius = random.NextDouble( 4, 7 );
					double x = Math.Cos( theta ) * radius;
					double y = Math.Sin( theta ) * radius;

					AddInternal( new BeatingGroup { Position = new Vector3( (float)x, 0, (float)y ), Child = new Collider { 
						Mesh = Mesh.UnitCube,
						AutoOffsetOriginY = -0.5f,
						Scale = new Vector3( (float)(random.NextDouble(0.05,0.2) * radius) ),
						Rotation = Quaternion.FromAxisAngle( Vector3.UnitY, (float)random.NextDouble( Math.PI * 2 ) )
					} } );
				}
			}
		}

		public class GripableCollider : Collider, IGripable {
			public bool CanBeGripped => true;
			public bool AllowsGripMovement => true;
			public bool AllowsGripScaling => true;
			public bool AllowsGripRotation => true;

			public void OnGripped ( object source, GripGroup group ) { }

			public void OnGripReleased ( object source, GripGroup group ) { }
		}
	}
}
