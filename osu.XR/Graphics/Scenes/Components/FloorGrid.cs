using osu.Framework.Caching;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.XR;
using osu.Framework.XR.Graphics;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Graphics.Scenes.Components;

public class FloorGrid : BasicModel {
	Cached meshCache = new();
	public readonly BindableInt XSegmentsBindable = new( 7 ) { MinValue = 0, MaxValue = 20, Precision = 1 };
	public readonly BindableInt ZSegmentsBindable = new( 7 ) { MinValue = 0, MaxValue = 20, Precision = 1 };
	public readonly BindableFloat SegmentWidthBindable = new( 0.01f ) { MinValue = 0.001f, MaxValue = 0.05f };
	public readonly BindableFloat SegmentSpreadBindable = new( 1 ) { MinValue = 0.1f, MaxValue = 2 };
	public readonly BindableFloat SegmentLengthBindable = new( 16.7f ) { MinValue = 5, MaxValue = 50 };

	public FloorGrid () {
		(XSegmentsBindable, ZSegmentsBindable, SegmentWidthBindable, SegmentSpreadBindable, SegmentLengthBindable).BindValuesChanged( () => meshCache.Invalidate(), true );
	}

	protected override void Update () {
		if ( !meshCache.IsValid ) {
			Mesh.Clear();
			RegenerateMesh();
			Mesh.CreateFullUpload().Enqueue();
		}
		base.Update();
	}

	[BackgroundDependencyLoader]
	private void load ( IRenderer renderer ) {
		Material.SetTexture( "tex", TextureGeneration.Vertical2SidedGradient( renderer, Color4.Transparent, Color4.White, 200 ) );
	}

	void RegenerateMesh () {
		var (x_segments, z_segments, width, x_spread, z_spread, x_length, z_length) = (
			XSegmentsBindable.Value,
			ZSegmentsBindable.Value,
			SegmentWidthBindable.Value,
			SegmentSpreadBindable.Value,
			SegmentSpreadBindable.Value,
			SegmentLengthBindable.Value,
			SegmentLengthBindable.Value
		);

		for ( int x = -x_segments; x <= x_segments; x++ ) {
			float xFrom = x * x_spread - width / 2;
			float xTo = x * x_spread + width / 2;
			float zFrom = x_length * -0.5f;
			float zTo = x_length * 0.5f;
			Mesh.AddQuad( new Quad3(
				new Vector3( xFrom, 0, zFrom ), new Vector3( xFrom, 0, zTo ),
				new Vector3( xTo, 0, zFrom ), new Vector3( xTo, 0, zTo )
			), new Vector2( 1, 0 ), new Vector2( 1, 1 ), new Vector2( 0, 0 ), new Vector2( 0, 1 ) );
		}

		for ( int z = -z_segments; z <= z_segments; z++ ) {
			float xFrom = z_length * -0.5f;
			float xTo = z_length * 0.5f;
			float zFrom = z * z_spread - width / 2;
			float zTo = z * z_spread + width / 2;
			Mesh.AddQuad( new Quad3(
				new Vector3( xFrom, 0, zFrom ), new Vector3( xFrom, 0, zTo ),
				new Vector3( xTo, 0, zFrom ), new Vector3( xTo, 0, zTo )
			), new Vector2( 0, 1 ), new Vector2( 1, 1 ), new Vector2( 0, 0 ), new Vector2( 1, 0 ) );
		}
	}
}
