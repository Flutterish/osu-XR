using osu.Framework.XR.Graphics.Rendering;

namespace osu.XR.Graphics.VirtualReality.Pointers;

public interface IPointerBase {
	void AddToScene ( Scene scene );
	void RemoveFromScene ( Scene scene );
	void SetTint ( Colour4 tint );
}
