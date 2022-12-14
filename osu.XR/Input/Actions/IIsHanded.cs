using osu.Framework.XR.VirtualReality;

namespace osu.XR.Input.Actions;

public interface IIsHanded : IActionBinding {
	Hand Hand { get; }
}
