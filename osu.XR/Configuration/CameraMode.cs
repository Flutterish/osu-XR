using System.ComponentModel;

namespace osu.XR.Configuration;

public enum CameraMode {
	[Description( @"Disabled" )]
	Disabled,
	[Description( @"VR Camera" )]
	FirstPersonVR,
	[Description( @"First Person" )]
	FirstPerson,
	[Description( @"Third Person" )]
	ThirdPerson
}
