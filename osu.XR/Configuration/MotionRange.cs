using System.ComponentModel;

namespace osu.XR.Configuration;

public enum MotionRange {
	[Description(@"With Controller")]
	WithController,

	[Description( @"Without Controller" )]
	WithoutController
}
