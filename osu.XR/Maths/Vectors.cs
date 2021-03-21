using osu.XR.Physics;
using osuTK;

namespace osu.XR.Maths {
	public static class Vectors {
		/// <summary>
		/// Computes the closest point on the direction line to this point
		/// </summary>
		public static Vector3 AlignedWith ( this Vector3 vector, Vector3 direction ) {
			Raycast.TryHit( Vector3.Zero, direction, vector, direction, out var hit, true );
			return hit.Point;
		}

		/// <summary>
		/// Computes the shadow of this vector on a plane
		/// </summary>
		public static Vector3 ProjectedOn ( this Vector3 vector, Vector3 normal ) {
			return vector - vector.AlignedWith( normal );
		}
	}
}
