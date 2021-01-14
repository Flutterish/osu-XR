using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Textures;
using osuTK;
using System;
using Quad = osu.XR.Projection.Quad;

namespace osu.XR.Rendering {
	internal class XrFloorGrid : XrDrawable {
        protected override DrawNode CreateDrawNode () => new XrFloorGridDrawNode( this );

        private class XrFloorGridDrawNode : XrDrawNode<XrFloorGrid> {
            public XrFloorGridDrawNode ( XrFloorGrid source )
                : base( source ) { }

            public override void Draw ( Action<TexturedVertex2D> vertexAction ) {
                base.Draw( vertexAction );
                TextureShader.Bind();

                if ( Texture.TextureGL.Bind() ) {
                    const int segments = 10;

                    const int x_segments = 7;
                    const float x_length = 16.7f;
                    const float x_spread = 1;
                    const float x_width = 0.01f;

                    const int z_segments = 7;
                    const float z_length = 16.7f;
                    const float z_spread = 1;
                    const float z_width = 0.01f;

                    for ( int x = -x_segments; x <= x_segments; x++ ) {
                        for ( int s = 0; s < segments; s++ ) {
                            float xFrom = x * x_spread - x_width / 2;
                            float xTo = x * x_spread + x_width / 2;
                            float zFrom = x_length * ( (float)s / segments - 0.5f );
                            float zTo = x_length * ( (float)( s + 1 ) / segments - 0.5f );

                            Draw( Texture, new Quad(
                                new Vector3( xFrom, 2, zFrom ), new Vector3( xFrom, 2, zTo ),
                                new Vector3( xTo, 2, zFrom ), new Vector3( xTo, 2, zTo )
                            ), Colour4.White );
                        }
                    }

                    for ( int z = -z_segments; z <= z_segments; z++ ) {
                        for ( int s = 0; s < segments; s++ ) {
                            float zFrom = z * z_spread - z_width / 2;
                            float zTo = z * z_spread + z_width / 2;
                            float xFrom = z_length * ( (float)s / segments - 0.5f );
                            float xTo = z_length * ( (float)( s + 1 ) / segments - 0.5f );

                            Draw( Texture, new Quad(
                                new Vector3( xFrom, 2, zFrom ), new Vector3( xFrom, 2, zTo ),
                                new Vector3( xTo, 2, zFrom ), new Vector3( xTo, 2, zTo )
                            ), Colour4.White );
                        }
                    }
                }
                TextureShader.Unbind();
            }
        }
    }
}
