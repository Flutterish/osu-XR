using osu.XR.Components;
using osu.XR.Graphics;
using osuTK;
using System;
using System.Collections.Generic;
using static osu.XR.Physics.Raycast;

namespace osu.XR.Physics {
	public class PhysicsSystem : IDisposable {
		private List<IHasCollider> colliders = new();
		private XrObject root;
		public XrObject Root {
			get => root;
			set {
				if ( root == value ) return;

				if ( root is not null ) {
					root.ChildAddedToHierarchy -= addXrObject;
					root.ChildRemovedFromHierarchy -= removeXrObject;
				}
				colliders.Clear();
				root = value;
				root?.BindHierarchyChange( addXrObject, removeXrObject, true );
			}
		}

		private void addXrObject ( XrObject parent, XrObject child ) {
			if ( child is IHasCollider collider ) {
				colliders.Add( collider );
			}
		}
		private void removeXrObject ( XrObject parent, XrObject child ) {
			if ( child is IHasCollider collider ) {
				colliders.Remove( collider );
			}
		}

		/// <summary>
		/// Intersect a 3D line and a the closest collider.
		/// </summary>
		public bool TryHit ( Vector3 origin, Vector3 direction, out RaycastHit hit, bool includeBehind = false ) {
			RaycastHit? closest = null;
			IHasCollider closestCollider = null;

			for ( int i = 0; i < colliders.Count; i++ ) {
				var collider = colliders[ i ];
				if ( collider.IsColliderEnabled && Raycast.TryHit( origin, direction, collider.Mesh, ( collider as XrObject ).Transform, out hit, includeBehind ) ) {
					if ( closest is null || Math.Abs( closest.Value.Distance ) > Math.Abs( hit.Distance ) ) {
						closest = hit;
						closestCollider = collider;
					}
				}
			}

			if ( closest is null ) {
				hit = default;
				return false;
			}
			else {
				hit = closest.Value;
				hit = new RaycastHit(
					hit.Point,
					hit.Origin,
					hit.Normal,
					hit.Direction,
					hit.Distance,
					hit.TrisIndex,
					closestCollider
				);
				return true;
			}
		}

		/// <summary>
		/// Intersect a shpere and a the closest collider.
		/// </summary>
		public bool TryHit ( Vector3 origin, double radius, out SphereHit hit ) {
			SphereHit? closest = null;
			IHasCollider closestCollider = null;

			for ( int i = 0; i < colliders.Count; i++ ) {
				var collider = colliders[ i ];
				if ( collider.IsColliderEnabled && Sphere.TryHit( origin, radius, collider.Mesh, ( collider as XrObject ).Transform, out hit ) ) {
					if ( closest is null || Math.Abs( closest.Value.Distance ) > Math.Abs( hit.Distance ) ) {
						closest = hit;
						closestCollider = collider;
					}
				}
			}

			if ( closest is null ) {
				hit = default;
				return false;
			}
			else {
				hit = closest.Value;
				hit = new SphereHit(
					hit.Distance,
					hit.Origin,
					hit.Radius,
					hit.Point,
					hit.TrisIndex,
					closestCollider
				);
				return true;
			}
		}

		public void Dispose () {
			Root = null;
		}
	}

	public interface IHasCollider {
		Mesh Mesh { get; }
		bool IsColliderEnabled { get; }
	}
}
