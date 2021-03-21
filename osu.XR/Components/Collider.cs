using osu.Framework.XR.Components;
using osu.XR.Physics;

namespace osu.XR.Components {
	public class Collider : Model, IHasCollider {
		public bool IsColliderEnabled { get; set; } = true;
	}
}
