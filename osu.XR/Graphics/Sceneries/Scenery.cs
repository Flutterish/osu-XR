﻿using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.XR.Graphics.Sceneries.Components;

namespace osu.XR.Graphics.Sceneries;

public partial class Scenery : CompositeDrawable3D {
	public Scenery () {
		AddInternal( new BasicModel {
			IsColliderEnabled = true,
			IsVisible = false,
			Mesh = BasicMesh.UnitCornerCube,
			Scale = new( 100, 0.01f, 100 )
		} );
		AddInternal( new DustEmitter() );
	}
}
