using osuTK;
using System;

namespace osu.XR.Maths {
	public static class Extensions {
		public static Vector3 ToEuler ( this Quaternion q ) {
			// using https://gamedev.net/forums/topic/597324-quaternion-to-euler-angles-and-back-why-is-the-rotation-changing/4784042/
			var xSquare = q.X * q.X;
			var ySquare = q.Y * q.Y;
			var zSquare = q.Z * q.Z;
			var wSquare = q.W * q.W;
			return new Vector3(
				MathF.Atan2( -2 * ( q.Y * q.Z - q.W * q.X ), wSquare - xSquare - ySquare + zSquare ),
				MathF.Asin( 2 * ( q.X * q.Z + q.W * q.Y ) ),
				MathF.Atan2( -2 * ( q.X * q.Y - q.W * q.Z ), wSquare + xSquare - ySquare - zSquare )
			);
		}

		public static Vector3 With ( this Vector3 v, float? x = null, float? y = null, float? z = null )
			=> new Vector3( x ?? v.X, y ?? v.Y, z ?? v.Z );

		public static Vector4 Reversed ( this Vector4 v )
			=> new Vector4( v.W, v.Z, v.Y, v.X );

		public static float SignedDistance ( Vector3 from, Vector3 to, Vector3 towards ) {
			var direction = to - from;
			return ( direction ).Length * ( ( Vector3.Dot( direction, towards - from ) > 0 ) ? 1 : -1 );
		}
	}
}
