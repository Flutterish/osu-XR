using osu.Framework.XR.Components;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;
using osu.XR.Maths;
using osuTK;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Physics {
	public static class Sphere {
		public static bool TryHit ( Vector3 origin, double radius, Face face, out SphereHit hit ) {
			Vector3 normal = Vector3.Cross( face.A - face.B, face.C - face.B ).Normalized();
			Raycast.TryHit( origin, normal, face.A, normal, out var rh, true );
			if ( Triangles.IsPointInside( rh.Point, face ) ) {
				if ( Math.Abs( rh.Distance ) <= radius ) {
					hit = new SphereHit(
						distance: Math.Abs( rh.Distance ),
						origin: origin,
						radius: radius,
						point: rh.Point
					);
					return true;
				}
				else {
					hit = default;
					return false;
				}
			}
			else {
				var A = Raycast.ClosestPoint( face.A, face.B, rh.Point );
				var B = Raycast.ClosestPoint( face.B, face.C, rh.Point );
				var C = Raycast.ClosestPoint( face.C, face.A, rh.Point );

				var al = A.Length;
				var bl = B.Length;
				var cl = C.Length;

				if ( al > radius && bl > radius && cl > radius ) {
					hit = default;
					return false;
				}
				else if ( al < bl && al < cl ) {
					hit = new SphereHit(
						distance: al,
						origin: origin,
						radius: radius,
						point: A
					);
					return true;
				}
				else if ( bl < al && bl < cl ) {
					hit = new SphereHit(
						distance: bl,
						origin: origin,
						radius: radius,
						point: B
					);
					return true;
				}
				else {
					hit = new SphereHit(
						distance: cl,
						origin: origin,
						radius: radius,
						point: C
					);
					return true;
				}
			}
		}

		public static bool TryHit ( Vector3 origin, double radius, Mesh mesh, Transform transform, out SphereHit hit ) {
			SphereHit? closest = null;
			// TODO optimize this with an AABB check
			for ( int i = 0; i < mesh.Tris.Count; i++ ) {
				var face = mesh.Faces[ i ];
				face.A = ( transform.Matrix * new Vector4( face.A, 1 ) ).Xyz;
				face.B = ( transform.Matrix * new Vector4( face.B, 1 ) ).Xyz;
				face.C = ( transform.Matrix * new Vector4( face.C, 1 ) ).Xyz;
				if ( TryHit( origin, radius, face, out hit ) && ( closest is null || closest.Value.Distance > hit.Distance ) ) {
					closest = new SphereHit(
						distance: hit.Distance,
						origin: hit.Origin,
						radius: hit.Radius,
						point: hit.Point,
						trisIndex: i
					);
				}
			}

			if ( closest is null ) {
				hit = default;
				return false;
			}
			else {
				hit = closest.Value;
				return true;
			}
		}

		public static bool TryHit ( Vector3 origin, double radius, MeshedXrObject target, out SphereHit hit ) {
			var ok = TryHit( origin, radius, target.Mesh, target.Transform, out hit );
			if ( ok ) {
				hit = new SphereHit(
					distance: hit.Distance,
					origin: hit.Origin,
					radius: hit.Radius,
					point: hit.Point,
					trisIndex: hit.TrisIndex,
					collider: target as IHasCollider
				);
			}
			return ok;
		}
	}

	public readonly struct SphereHit {
		public readonly double Distance;
		public readonly Vector3 Origin;
		public readonly double Radius;
		public readonly Vector3 Point;
		public readonly int TrisIndex;
		public readonly IHasCollider Collider;

		public SphereHit ( double distance, Vector3 origin, double radius, Vector3 point, int trisIndex = -1, IHasCollider collider = null ) {
			Distance = distance;
			this.Origin = origin;
			Radius = radius;
			Point = point;
			TrisIndex = trisIndex;
			Collider = collider;
		}
	}
}
