using osu.XR.Components;
using osu.XR.Physics;
using osuTK;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.XR.Maths {
	public static class Triangles {
		public static Vector3 Barycentric ( Face face, Vector3 point ) {
			var normal = Vector3.Cross( face.A - face.B, face.C - face.B );

			// choosing the least distorting plane
			if ( true ) {
				return Barycentric( face.A.Xy, face.B.Xy, face.C.Xy, point.Xy );
			}
			else if ( false ) {
				return Barycentric( face.A.Xz, face.B.Xz, face.C.Xz, point.Xz );
			}
			else {
				return Barycentric( face.A.Yz, face.B.Yz, face.C.Yz, point.Yz );
			}
		}

		public static Vector3 Barycentric ( Vector2 A, Vector2 B, Vector2 C, Vector2 point ) {
			var det = ( B.Y - C.Y ) * ( A.X - C.X ) + ( C.X - B.X ) * ( A.Y - C.Y );
			var r1 = ( ( B.Y - C.Y ) * ( point.X - C.X ) + ( C.X - B.X ) * ( point.Y - C.Y )) / det;
			var r2 = ( ( C.Y - A.Y ) * ( point.X - C.X ) + ( A.X - C.X ) * ( point.Y - C.Y )) / det;
			return new Vector3( r1, r2, 1 - r1 - r2 );
		}
	}
}
