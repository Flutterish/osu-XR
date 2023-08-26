using osu.Framework.XR.Allocation;

namespace osu.XR.Graphics.VirtualReality.Pointers;

public interface IPointerSource {
	IEnumerable<Pointer> Pointers { get; }
	RentedArray<PointerHit?> UpdatePointers ( Vector3 playerPosition, Vector3 position, Quaternion rotation );
	void SetTint ( Colour4 tint );
}
