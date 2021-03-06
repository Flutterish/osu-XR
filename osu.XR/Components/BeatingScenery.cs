using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;
using osu.XR.Components.Groups;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components {
	public class BeatingScenery : XrObject {
		public BeatingScenery ( int? seed = null ) {
			Random random;
			if ( seed.HasValue ) random = new( seed.Value );
			else random = new();

			double next () => random.NextDouble( 0.02, 0.1 );
			for ( double theta = next(); theta < Math.PI * 2; theta += next() ) {
				if ( random.Chance( 0.2 ) ) {
					double radius = random.NextDouble( 4, 7 );
					double x = Math.Cos( theta ) * radius;
					double y = Math.Sin( theta ) * radius;

					Add( new BeatingGroup { Position = new Vector3( (float)x, 0, (float)y ), Child = new Collider { 
						Mesh = Mesh.UnitCube,
						AutoOffsetOriginY = -0.5f,
						Scale = new Vector3( (float)(random.NextDouble(0.05,0.2) * radius) ),
						Rotation = Quaternion.FromAxisAngle( Vector3.UnitY, (float)random.NextDouble( Math.PI * 2 ) )
					} } );
				}
			}
		}
	}
}
