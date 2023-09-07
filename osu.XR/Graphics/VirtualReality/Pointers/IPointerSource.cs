using osu.Framework.XR.Allocation;

namespace osu.XR.Graphics.VirtualReality.Pointers;

public interface IPointerSource : IPointerBase {
	IEnumerable<Pointer> Pointers { get; }
	RentedArray<PointerHit?> UpdatePointers ( Vector3 playerPosition, Vector3 position, Quaternion rotation );
}
