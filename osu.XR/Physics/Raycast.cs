﻿using FFmpeg.AutoGen;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using osu.Game.Rulesets.Scoring;
using osu.XR.Components;
using osu.XR.Maths;
using osuTK;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.XR.Physics {
	public static class Raycast {
		public static bool TryHit ( Vector3 origin, Vector3 direction, Vector3 pointOnPlane, Vector3 planeNormal, out RaycastHit hit, bool includeBehind = false ) {
			// plane := all points where ( point - pointOnPlane ) dot planeNormal = 0
			// line := all points where ( point - pointOnLine ) - d * direction = 0
			// in other words, point = pointOnLine + d * direction
			// the intersection of the two: ( pointOnLine + d * direction - pointOnPlane ) dot planeNormal = 0
			// direction dot planeNormal * d + ( pointOnLine - pointOnPlane ) dot planeNormal = 0
			// d = (( pointOnPlane - pointOnLine ) dot planeNormal) / (direction dot planeNormal)
			// therefore if direction dot planeNormal is 0, there is no intersection or they are on top of each other
			direction.Normalize();
			planeNormal.Normalize();

			var dot = Vector3.Dot( direction, planeNormal );
			if ( dot == 0 ) {
				if ( Vector3.Dot( origin - pointOnPlane, planeNormal ) == 0 ) {
					hit = new RaycastHit(
						origin,
						origin,
						planeNormal,
						direction,
						0
					);
					return true;
				}
				else {
					hit = default;
					return false;
				}
			}
			else {
				var distance = Vector3.Dot( pointOnPlane - origin, planeNormal ) / dot;

				hit = new RaycastHit(
					origin + direction * distance,
					origin,
					planeNormal,
					direction,
					distance
				);
				return distance >= 0 || includeBehind;
			}
		}

		public static bool TryHitLine ( Vector3 pointOnLineA, Vector3 directionA, Vector3 pointOnLineB, Vector3 directionB, out Vector3 hit ) {
			bool Approx ( float a, float b, float tolerance = 0.001f )
				=> MathF.Abs( a - b ) <= tolerance;

			float sum = 0;
			int count = 0;
			// p = (o2.y - o1.y) + (d2.y/d2.x)(o1.x - o2.x)/(d1.y - (d2.y/d2.x)(d1.x))
			if ( directionB.X != 0 ) {
				var div = directionA.Y - directionA.X * directionB.Y / directionB.X;
				if ( !Approx( div, 0 ) ) {
					var q = ( pointOnLineB.Y - pointOnLineA.Y + ( pointOnLineA.X - pointOnLineB.X ) * directionB.Y / directionB.X ) / div;
					if ( count == 0 || Approx( sum / count, q ) ) {
						sum += q;
						count++;
					}
				}
			}
			if ( directionB.Y != 0 ) {
				var div = directionA.Z - directionA.Y * directionB.Z / directionB.Y;
				if ( !Approx( div, 0 ) ) {
					var q = ( pointOnLineB.Z - pointOnLineA.Z + ( pointOnLineA.Y - pointOnLineB.Y ) * directionB.Z / directionB.Y ) / div;
					if ( count == 0 || Approx( sum / count, q ) ) {
						sum += q;
						count++;
					}
				}
			}
			if ( directionB.Z != 0 ) {
				var div = directionA.X - directionA.Z * directionB.X / directionB.Z;
				if ( !Approx( div, 0 ) ) {
					var q = ( pointOnLineB.X - pointOnLineA.X + ( pointOnLineA.Z - pointOnLineB.Z ) * directionB.X / directionB.Z ) / div;
					if ( count == 0 || Approx( sum / count, q ) ) {
						sum += q;
						count++;
					}
				}
			}

			if ( sum != 0 ) {
				hit = pointOnLineA + sum / count * directionA;
				return true;
			}

			hit = default;
			return false;
		}


		public static bool TryHitLine ( Vector2 pointOnLineA, Vector2 directionA, Vector2 pointOnLineB, Vector2 directionB, out Vector2 hit ) { // TODO vector math this
			// y = m1 x + b1
			// y = m2 x + b2
			// m1 x + b1 = m2 x + b2
			// (b1-b2) = x(m2-m1)
			// x = (m2-m1)/(b1-b2)
			// y - m1 x = b1

			if ( directionA.X == 0 ) {
				var m1 = directionA.X / directionA.Y;
				var b1 = pointOnLineA.X - m1 * pointOnLineA.Y;
				var m2 = directionB.X / directionB.Y;
				var b2 = pointOnLineB.X - m2 * pointOnLineB.Y;

				if ( m1 == m2 ) {
					if ( b1 == b2 ) {
						hit = pointOnLineA;
						return true;
					}
					else {
						hit = default;
						return false;
					}
				}
				else {
					var y = ( b1 - b2 ) / ( m2 - m1 );
					hit = new Vector2( m1 * y + b1, y );
					return true;
				}
			}
			else {
				var m1 = directionA.Y / directionA.X;
				var b1 = pointOnLineA.Y - m1 * pointOnLineA.X;
				var m2 = directionB.Y / directionB.X;
				var b2 = pointOnLineB.Y - m2 * pointOnLineB.X;

				if ( m1 == m2 ) {
					if ( b1 == b2 ) {
						hit = pointOnLineA;
						return true;
					}
					else {
						hit = default;
						return false;
					}
				}
				else {
					var x = ( b1 - b2 ) / ( m2 - m1 );
					hit = new Vector2( x, m1 * x + b1 );
					return true;
				}
			}
		}

		public static bool TryHit ( Vector3 origin, Vector3 direction, Face face, out RaycastHit hit, bool includeBehind = false ) {
			if ( TryHit( origin, direction, face.A, Vector3.Cross( face.B - face.A, face.C - face.A ), out hit, includeBehind ) ) {
				var directionFromC = ( face.C - hit.Point ).Normalized();
				if ( TryHitLine( hit.Point, directionFromC, face.A, face.B - face.A, out var pointOnAB ) ) {
					var distanceFromAToB = Extensions.SignedDistance( face.A, pointOnAB, face.B );
					if ( distanceFromAToB >= -0.01f && distanceFromAToB <= ( face.B - face.A ).Length + 0.01f ) {
						var distanceToC = Extensions.SignedDistance( face.C, hit.Point, pointOnAB );
						if ( distanceToC >= -0.01f && distanceToC <= ( face.C - pointOnAB ).Length + 0.01f ) {
							return true;
						}
					}
				}
			}

			hit = default;
			return false;
		}

		public static bool TryHit ( Vector3 origin, Vector3 direction, Mesh mesh, Transform transform, out RaycastHit hit, bool includeBehind = false ) {
			for ( int i = 0; i < mesh.Tris.Count; i++ ) {
				var face = mesh.Faces[ i ];
				face.A = ( transform.Matrix * new Vector4( face.A, 1 ) ).Xyz;
				face.B = ( transform.Matrix * new Vector4( face.B, 1 ) ).Xyz;
				face.C = ( transform.Matrix * new Vector4( face.C, 1 ) ).Xyz;
				if ( TryHit( origin, direction, face, out hit, includeBehind ) ) {
					hit = new RaycastHit(
						hit.Point,
						hit.Origin,
						hit.Normal,
						hit.Direction,
						hit.Distance,
						i
					);
					return true;
				}
			}
			hit = default;
			return false;
		}

		public static bool TryHit ( Vector3 origin, Vector3 direction, XrMesh target, out RaycastHit hit, bool includeBehind = false ) {
			return TryHit( origin, direction, target.Mesh, target.Transform, out hit, includeBehind );
		}

		public struct RaycastHit {
			public readonly Vector3 Point;
			public readonly Vector3 Origin;
			public readonly Vector3 Normal;
			public readonly Vector3 Direction;
			public readonly double Distance;
			public readonly int TrisIndex;

			public RaycastHit ( Vector3 point, Vector3 origin, Vector3 normal, Vector3 direction, double distance, int trisIndex = -1 ) {
				Point = point;
				Origin = origin;
				Normal = normal;
				Direction = direction;
				Distance = distance;
				TrisIndex = trisIndex;
			}
		}
	}
}
