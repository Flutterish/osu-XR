using System.ComponentModel;

namespace osu.XR.Configuration;

public enum SkyBoxType {
	Solid,

	[Description( "Lights Out" )]
	LightsOut,

	// TODO Storyboard?
}