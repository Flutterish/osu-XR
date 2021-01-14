using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.OpenGL.Vertices;
using System;

namespace osu.XR.Rendering {
	public class XrSkybox : XrDrawable {
        protected override DrawNode CreateDrawNode ()
            => new XrSkyboxDrawNode( this );

        private class XrSkyboxDrawNode : XrDrawNode<XrSkybox> {
            public XrSkyboxDrawNode ( XrSkybox source ) : base( source ) { }

            public override void Draw ( Action<TexturedVertex2D> vertexAction ) {
                base.Draw( vertexAction );
                TextureShader.Bind();
                float size = 10;
                var floor = Colour4.HotPink;
                var colours = ColourInfo.GradientVertical( floor, new Colour4( 0, 0, 0, 0 ) );
                DrawSubdivided( Texture, new Projection.Quad( new osuTK.Vector3( -size, 4, size ), new osuTK.Vector3( size, 4, size ), new osuTK.Vector3( -size, -size, size ), new osuTK.Vector3( size, -size, size ) ) + Camera.Position, colours, 32 );
                DrawSubdivided( Texture, new Projection.Quad( new osuTK.Vector3( -size, 4, -size ), new osuTK.Vector3( size, 4, -size ), new osuTK.Vector3( -size, -size, -size ), new osuTK.Vector3( size, -size, -size ) ) + Camera.Position, colours, 32 );
                DrawSubdivided( Texture, new Projection.Quad( new osuTK.Vector3( size, 4, -size ), new osuTK.Vector3( size, 4, size ), new osuTK.Vector3( size, -size, -size ), new osuTK.Vector3( size, -size, size ) ) + Camera.Position, colours, 32 );
                DrawSubdivided( Texture, new Projection.Quad( new osuTK.Vector3( -size, 4, -size ), new osuTK.Vector3( -size, 4, size ), new osuTK.Vector3( -size, -size, -size ), new osuTK.Vector3( -size, -size, size ) ) + Camera.Position, colours, 32 );

                DrawSubdivided( Texture, new Projection.Quad( new osuTK.Vector3( -size, 4, size ), new osuTK.Vector3( size, 4, size ), new osuTK.Vector3( -size, 4, -size ), new osuTK.Vector3( size, 4, -size ) ) + Camera.Position, floor, 16, 16 );
                TextureShader.Unbind();
            }
        }
    }
}
