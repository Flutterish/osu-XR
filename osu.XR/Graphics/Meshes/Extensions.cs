using osu.Framework.XR.Graphics.Meshes;

namespace osu.XR.Graphics.Meshes;

public static class Extensions {
	public static void AddArcedPlane ( this BasicMesh self, Vector3 up, Vector3 forward, float height, float radius, float angle, int? steps = null, Vector3? origin = null ) {
		forward.Normalize();
		up.Normalize();

		origin ??= Vector3.Zero;
		steps ??= (int)( angle / MathF.PI * 128 );
		if ( steps < 1 ) steps = 1;
		var deltaAngle = angle / steps.Value;

		(uint a, uint b) addVertices ( float angle ) {
			var middle = Quaternion.FromAxisAngle( up, angle ).Apply( forward ) * radius + origin.Value;
			self.Vertices.Add( new() { Position = middle - up * height / 2 } );
			self.Vertices.Add( new() { Position = middle + up * height / 2 } );

			return ((uint)self.Vertices.Count - 2, (uint)self.Vertices.Count - 1);
		}

		var (lastVerticeA, lastVerticeB) = addVertices( 0 );
		for ( int i = 1; i <= steps; i++ ) {
			var (a, b) = addVertices( deltaAngle * i );

			self.AddFace( lastVerticeA, lastVerticeB, b );
			self.AddFace( a, b, lastVerticeA );

			(lastVerticeA, lastVerticeB) = (a, b);
		}
	}
}
