using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Maths;

namespace osu.XR.Graphics.Panels;

public class CurvedPanel : Panel {
	public float Arc { get => ArcBindable.Value; set => ArcBindable.Value = value; }
	public float Radius { get => RadiusBindable.Value; set => RadiusBindable.Value = value; }
	public float AspectRatio => aspectRatioBindable.Value;
	public int Resolution { get => ResolutionBindable.Value; set => ResolutionBindable.Value = value; }
	public readonly BindableFloat ArcBindable = new( MathF.PI * 0.7f ) { MinValue = 0, MaxValue = MathF.PI * 2 };
	public readonly BindableFloat RadiusBindable = new( 1.6f );
	BindableFloat aspectRatioBindable = new( 1 );
	public readonly BindableInt ResolutionBindable = new( 64 );

	public CurvedPanel () {
		ArcBindable.ValueChanged += _ => MeshCache.Invalidate();
		RadiusBindable.ValueChanged += _ => MeshCache.Invalidate();
		aspectRatioBindable.ValueChanged += _ => MeshCache.Invalidate();
		ResolutionBindable.ValueChanged += _ => MeshCache.Invalidate();
	}

	protected override void UpdateAfterChildren () {
		aspectRatioBindable.Value = ContentDrawSize.X / ContentDrawSize.Y;
	}

	protected override void RegenrateMesh () {
		var width = Arc * Radius;
		var height = width / AspectRatio;

		for ( int i = 0; i < Resolution; i++ ) {
			var start = (float)i / Resolution;
			var end = (float)(i+1) / Resolution;
			var startAngle = Arc * start - Arc / 2;
			var endAngle = Arc * end - Arc / 2;

			Mesh.AddQuad( 
				new Quad3 {
					TL = new( MathF.Sin( startAngle ) * Radius, height / 2, MathF.Cos( startAngle ) * Radius ),
					BL = new( MathF.Sin( startAngle ) * Radius, -height / 2, MathF.Cos( startAngle ) * Radius ),
					TR = new( MathF.Sin( endAngle ) * Radius, height / 2, MathF.Cos( endAngle ) * Radius ),
					BR = new( MathF.Sin( endAngle ) * Radius, -height / 2, MathF.Cos( endAngle ) * Radius )
				},
				TL: new( start, 1 ),
				BL: new( start, 0 ),
				TR: new( end, 1 ),
				BR: new( end, 0 )
			);
		}
	}
}
