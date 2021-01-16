using osu.XR.Graphics;
using osuTK;
using System;

namespace osu.XR.Maths {
	public static class Triangles {
		/// <summary>
		/// Calculates the barycentric coordinates of a point that lies on a face.
		/// If the point does not lie on the face, it will return the barycentric coordinates with respect to the last distorted cardinal plane.
		/// </summary>
		public static Vector3 Barycentric ( Face face, Vector3 point ) {
			var normal = Vector3.Cross( face.A - face.B, face.C - face.B );
			var dotX = MathF.Abs( Vector3.Dot( normal, Vector3.UnitX ) );
			var dotY = MathF.Abs( Vector3.Dot( normal, Vector3.UnitY ) );
			var dotZ = MathF.Abs( Vector3.Dot( normal, Vector3.UnitZ ) );

			// choosing the least distorting plane
			if ( dotZ > dotX && dotZ > dotY ) {
				return Barycentric( face.A.Xy, face.B.Xy, face.C.Xy, point.Xy );
			}
			else if ( dotY > dotX && dotY > dotZ ) {
				return Barycentric( face.A.Xz, face.B.Xz, face.C.Xz, point.Xz );
			}
			else {
				return Barycentric( face.A.Yz, face.B.Yz, face.C.Yz, point.Yz );
			}
		}

		/// <summary>
		/// Calculates the barycentric coordinates of a point on a simplex.
		/// </summary>
		public static Vector3 Barycentric ( Vector2 A, Vector2 B, Vector2 C, Vector2 point ) {
			var det = ( B.Y - C.Y ) * ( A.X - C.X ) + ( C.X - B.X ) * ( A.Y - C.Y );
			var r1 = ( ( B.Y - C.Y ) * ( point.X - C.X ) + ( C.X - B.X ) * ( point.Y - C.Y )) / det;
			var r2 = ( ( C.Y - A.Y ) * ( point.X - C.X ) + ( A.X - C.X ) * ( point.Y - C.Y )) / det;
			return new Vector3( r1, r2, 1 - r1 - r2 );
		}
	}
}
