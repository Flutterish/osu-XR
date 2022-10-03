using System.ComponentModel;

namespace osu.XR.Configuration;

public enum InputMode {
	[Description( "Single Pointer" )]
	SinglePointer,

	[Description( "Two Pointers" )]
	DoublePointer,

	[Description( "Touchscreen" )]
	TouchScreen
}