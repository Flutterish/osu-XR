using osuTK;
using System;

namespace osu.XR.Maths {
	/// <summary>
	/// A 3D axis-aligned Box.
	/// </summary>
	public struct AABox {
		/// <summary>
		/// Origin located at X-, Y-, Z- most point of the box
		/// </summary>
		public Vector3 Min;
		/// <summary>
		/// How much the box expands into X+, Y+, Z+ from the origin.
		/// </summary>
		public Vector3 Size;
		public Vector3 Max => Min + Size;

		public static AABox operator * ( Matrix4x4 matrix, AABox box ) {
			var origin = ( matrix * new Vector4( box.Min, 1 ) ).Xyz;
			var point2 = ( matrix * new Vector4( box.Min + box.Size, 1 ) ).Xyz;

			var lowest = new Vector3( MathF.Min( origin.X, point2.X ), MathF.Min( origin.Y, point2.Y ), MathF.Min( origin.Z, point2.Z ) );
			var greatest = new Vector3( MathF.Max( origin.X, point2.X ), MathF.Max( origin.Y, point2.Y ), MathF.Max( origin.Z, point2.Z ) );

			return new AABox {
				Min = lowest,
				Size = greatest - lowest
			};
		}
	}
}
