namespace osu.XR.Configuration;

[Flags]
public enum Fingers {
	None = 0,
	All = Pinky | Ring | Middle | Index | Thumb,

	Pinky = 0x1,
	Ring = 0x2,
	Middle = 0x4,
	Index = 0x8,
	Thumb = 0x10
}
