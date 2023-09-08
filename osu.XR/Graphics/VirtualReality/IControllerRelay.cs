using osu.XR.Graphics.VirtualReality.Pointers;

namespace osu.XR.Graphics.VirtualReality;

public interface IControllerRelay {
	IEnumerable<RelayButton> GetButtonsFor ( VrController source, VrAction action );
	void ScrollBy ( VrController source, Vector2 amount );
}
