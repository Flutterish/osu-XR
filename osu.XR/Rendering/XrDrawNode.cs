using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osu.XR.Projection;
using osuTK;
using Quad = osu.XR.Projection.Quad;

namespace osu.XR.Rendering {
	public abstract class XrDrawNode<T> : DrawNode where T : IXrDrawable {
        protected new T Source => (T)base.Source;
        protected Camera Camera;
        protected IShader TextureShader;
        protected Texture Texture;
        protected XrDrawNode ( T source ) : base( source ) {
            Texture = Texture.WhitePixel;
        }

        private Vector2 size;
        private Vector2 correctedSize;
        private float aspectRatio => Camera.AspectRatio;
        public override void ApplyState () {
            base.ApplyState();
            Camera = Source.Camera;
            TextureShader = Source.TextureShader;

            size = Source.DrawSize;
            if ( size.X / size.Y > aspectRatio ) {
                correctedSize = new( size.Y * aspectRatio, size.Y );
            }
            else {
                correctedSize = new( size.X, size.X / aspectRatio );
            }
        }

        protected void Draw ( Texture texture, in Quad quad, in ColourInfo colourInfo, RectangleF? textureRect = null ) {
            if ( toScreenSpace( quad, out var projQuad ) ) {
                DrawQuad( texture, projQuad, colourInfo, textureRect );
            }
        }
        protected void DrawSubdivided ( Texture texture, in Quad quad, in ColourInfo colourInfo, int xSubdivisions = 0, int ySubdivisions = 0 ) {
            xSubdivisions++;
            ySubdivisions++;
            for ( int x = 0; x < xSubdivisions; x++ ) {
                float xProgA = ( (float)x / xSubdivisions );
                float xProgB = ( (float)( x + 1 ) / xSubdivisions );
                var xQuad = new Quad(
                    quad.TL + ( quad.TR - quad.TL ) * xProgA, quad.TL + ( quad.TR - quad.TL ) * xProgB,
                    quad.BL + ( quad.BR - quad.BL ) * xProgA, quad.BL + ( quad.BR - quad.BL ) * xProgB
                );
                for ( int y = 0; y < ySubdivisions; y++ ) {
                    float yProgA = ( (float)y / ySubdivisions );
                    float yProgB = ( (float)( y + 1 ) / ySubdivisions );
                    var subQuad = new Quad(
                        xQuad.TL + ( xQuad.BL - xQuad.TL ) * yProgA, xQuad.TR + ( xQuad.BR - xQuad.TR ) * yProgA,
                        xQuad.TL + ( xQuad.BL - xQuad.TL ) * yProgB, xQuad.TR + ( xQuad.BR - xQuad.TR ) * yProgB
                    );
                    if ( toScreenSpace( subQuad, out var projQuad ) ) {
                        DrawQuad( texture, projQuad, colourInfo );
                    }
                }
            }
        }
        protected void Draw ( TextureGL texture, in Quad quad, in ColourInfo colourInfo, RectangleF? textureRect = null ) {
            if ( toScreenSpace( quad, out var projQuad ) ) {
                DrawQuad( texture, projQuad, colourInfo, textureRect );
            }
        }

        private bool toScreenSpace ( in Quad quad, out Framework.Graphics.Primitives.Quad projection ) {
            if ( Camera.Project( quad.TL, out var tl ) && Camera.Project( quad.TR, out var tr )
                && Camera.Project( quad.BL, out var bl ) && Camera.Project( quad.BR, out var br ) ) {
                projection = new Framework.Graphics.Primitives.Quad(
                    toScreen( size * 0.5f + correctedSize / 2 * tl ),
                    toScreen( size * 0.5f + correctedSize / 2 * tr ),
                    toScreen( size * 0.5f + correctedSize / 2 * bl ),
                    toScreen( size * 0.5f + correctedSize / 2 * br )
                );
                return true;
            }
            else {
                projection = default;
                return false;
            }
        }

        private Vector2 toScreen ( Vector2 pos )
            => Vector2Extensions.Transform( pos, DrawInfo.Matrix );
    }

    public interface IXrDrawable : IDrawable {
        Camera Camera { get; }
        IShader TextureShader { get; }
    }
}
