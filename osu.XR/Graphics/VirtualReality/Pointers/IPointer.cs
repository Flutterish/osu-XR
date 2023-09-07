using osu.Framework.XR.Physics;

namespace osu.XR.Graphics.VirtualReality.Pointers;

public interface IPointer : IPointerBase {
	PointerHit? UpdatePointer ( Vector3 playerPosition, Vector3 position, Quaternion rotation );
	/// <summary>
	/// Whether this pointer should emulate touch input
	/// </summary>
	bool IsTouchSource { get; }
}

public readonly struct PointerHit {
	public Vector3 Point { get; init; }
	public Vector3 Normal { get; init; }
	public int TrisIndex { get; init; }
	public IHasCollider? Collider { get; init; }

	public static implicit operator PointerHit ( RaycastHit hit )
		=> new() { Point = hit.Point, Normal = hit.Normal, TrisIndex = hit.TrisIndex, Collider = hit.Collider };

	public static implicit operator PointerHit ( SphereHit hit )
		=> new() { Point = hit.Point, Normal = hit.Normal, TrisIndex = hit.TrisIndex, Collider = hit.Collider };
}