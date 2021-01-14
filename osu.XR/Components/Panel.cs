using osu.Framework.Graphics.Containers;
using osu.XR.Rendering;
using osuTK;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.XR.Components {
	public class Panel : XrMesh {
		public BufferedCapture Source;

		public Panel ( BufferedCapture source ) {
			Source = source;
            UseGammaCorrection = true;
		}

		public void SetCurvature ( float arc = MathF.PI * 0.2f, float radius = 4 ) {
            Mesh = new();

            var points = 100;
            var arclength = arc * radius;
            var height = arclength / ( (float)Texture.Width / Texture.Height );
            for ( var i = 0; i < points; i++ ) {
                var start = arc / points * i - arc / 2;
                var end = arc / points * ( i + 1 ) - arc / 2;

                var posA = new Vector2( MathF.Sin( end ), MathF.Cos( end ) ) * radius;
                var posB = new Vector2( MathF.Sin( start ), MathF.Cos( start ) ) * radius;

                Mesh.AddAABBQuad( new Maths.Quad(
                    new Vector3( posB.X, height / 2, posB.Y ), new Vector3( posA.X, height / 2, posA.Y ),
                    new Vector3( posB.X, -height / 2, posB.Y ), new Vector3( posA.X, -height / 2, posA.Y )
                ), new Vector2( (float)i / points, 1 ), new Vector2( (float)(i+1) / points, 1 ), new Vector2( (float)i / points, 0 ), new Vector2( (float)(i+1) / points, 0 ) );
            }
        }

        private bool initilized;
		public override void BeforeDraw () {
            Texture = Source.Capture;
            if ( !initilized ) {
                initilized = true;
                SetCurvature();
			}
		}
	}
}
