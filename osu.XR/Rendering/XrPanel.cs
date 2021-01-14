using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Buffers;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Layout;
using osu.XR.Projection;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using Quad = osu.XR.Projection.Quad;

namespace osu.XR.Rendering {
	internal class XrPanel : Container, IXrDrawable {
        public XrPanel () {
            sharedData = new BufferedDrawNodeSharedData( null, false );
        }

        public ColourInfo EffectColour = Color4.White;
        public DrawColourInfo? FrameBufferDrawColour => base.DrawColourInfo;
        public Vector2 FrameBufferScale { get; set; } = Vector2.One;
        public Color4 BackgroundColour { get; set; } = new Color4( 0, 0, 0, 0 );
        public IShader TextureShader { get; private set; }
        public Camera Camera { get; private set; }
        [BackgroundDependencyLoader]
        private void load ( ShaderManager shaders, Camera camera ) {
            TextureShader = shaders.Load( VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE );
            Camera = camera;
        }

        private long updateVersion;
        protected override void Update () {
            base.Update();
            Invalidate( Invalidation.DrawNode );
        }
        protected override bool OnInvalidate ( Invalidation invalidation, InvalidationSource source ) {
            var result = base.OnInvalidate( invalidation, source );

            if ( ( invalidation & Invalidation.DrawNode ) > 0 ) {
                ++updateVersion;
                result = true;
            }

            return result;
        }

        private readonly BufferedDrawNodeSharedData sharedData;
        protected override DrawNode CreateDrawNode () => new XrPanelDrawNode( this, sharedData );
        protected override void Dispose ( bool isDisposing ) {
            base.Dispose( isDisposing );
            sharedData.Dispose();
        }

        private class XrPanelDrawNode : XrDrawNode<XrPanel>, ICompositeDrawNode {
            protected readonly BufferedDrawNodeSharedData SharedData;
            protected CompositeDrawableDrawNode Child;
            private ColourInfo effectColour;
            protected RectangleF DrawRectangle { get; private set; }
            private Color4 backgroundColour;
            private RectangleF screenSpaceDrawRectangle;
            private Vector2 frameBufferScale;
            private Vector2 frameBufferSize;
            protected new DrawColourInfo DrawColourInfo { get; private set; }

            public XrPanelDrawNode ( XrPanel source, BufferedDrawNodeSharedData sharedData )
                : base( source ) {
                Child = new CompositeDrawableDrawNode( source );
                SharedData = sharedData;
            }

            public override void ApplyState () {
                base.ApplyState();
                updateVersion = Source.updateVersion;
                effectColour = Source.EffectColour;
                backgroundColour = Source.BackgroundColour;
                screenSpaceDrawRectangle = Source.ScreenSpaceDrawQuad.AABBFloat;
                DrawColourInfo = Source.FrameBufferDrawColour ?? new DrawColourInfo( Color4.White, base.DrawColourInfo.Blending );
                frameBufferScale = Source.FrameBufferScale;

                frameBufferSize = new Vector2( MathF.Ceiling( screenSpaceDrawRectangle.Width * frameBufferScale.X ), MathF.Ceiling( screenSpaceDrawRectangle.Height * frameBufferScale.Y ) );
                DrawRectangle = SharedData.PixelSnapping
                    ? new RectangleF( screenSpaceDrawRectangle.X, screenSpaceDrawRectangle.Y, frameBufferSize.X, frameBufferSize.Y )
                    : screenSpaceDrawRectangle;

                Child.ApplyState();
            }

            public sealed override void Draw ( Action<TexturedVertex2D> vertexAction ) {
                using ( establishFrameBufferViewport() ) {
                    // Fill the frame buffer with drawn children
                    using ( BindFrameBuffer( SharedData.MainBuffer ) ) {
                        // We need to draw children as if they were zero-based to the top-left of the texture.
                        // We can do this by adding a translation component to our (orthogonal) projection matrix.
                        GLWrapper.PushOrtho( screenSpaceDrawRectangle );
                        GLWrapper.Clear( new ClearInfo( backgroundColour ) );

                        Child.Draw( vertexAction );

                        GLWrapper.PopOrtho();
                    }
                }

                TextureShader.Bind();

                base.Draw( vertexAction );
                DrawContents();

                TextureShader.Unbind();
            }
            public float ScreenAspectRation => Source.ChildSize.X / Source.ChildSize.Y;
            protected void DrawContents () {
                var finalEffectColour = DrawColourInfo.Colour;
                finalEffectColour.ApplyChild( effectColour );

                TextureShader.Bind();
                var texture = SharedData.CurrentEffectBuffer.Texture;

                if ( texture.Bind() ) {
                    var arc = MathF.PI * 0.2f;
                    var points = 100;
                    var Ypoints = 5;
                    float radius = 4;
                    var arclength = arc * radius;
                    var height = arclength / ScreenAspectRation;
                    for ( var i = 0; i < points; i++ ) {
                        var start = arc / points * i - arc / 2;
                        var end = arc / points * ( i + 1 ) - arc / 2;

                        var posA = new Vector2( MathF.Sin( end ), MathF.Cos( end ) ) * radius;
                        var posB = new Vector2( MathF.Sin( start ), MathF.Cos( start ) ) * radius;

                        //Draw( texture, new Quad(
                        //    new Vector3(posB.X, height / 2, posB.Y), new Vector3(posA.X, height / 2, posA.Y),
                        //    new Vector3(posB.X, -height / 2, posB.Y), new Vector3(posA.X, -height / 2, posA.Y)), finalEffectColour, new RectangleF(texture.Width * (float)i / points, 0, texture.Width * 1f / points, texture.Height));
                        for ( int k = 0; k < Ypoints; k++ ) {
                            var yFrom = ( (float)k / Ypoints - 0.5f ) * height;
                            var yTo = ( (float)( k + 1 ) / Ypoints - 0.5f ) * height;
                            Draw( texture, new Quad(
                                new Vector3( posB.X, yTo, posB.Y ), new Vector3( posA.X, yTo, posA.Y ),
                                new Vector3( posB.X, yFrom, posB.Y ), new Vector3( posA.X, yFrom, posA.Y ) ), finalEffectColour, new RectangleF( texture.Width * (float)i / points, texture.Height - ( (float)( k + 1 ) / Ypoints ) * texture.Height, texture.Width * 1f / points, texture.Height / Ypoints ) );
                        }
                    }
                }
                TextureShader.Unbind();
            }
            protected IDisposable BindFrameBuffer ( FrameBuffer frameBuffer ) {
                // This setter will also take care of allocating a texture of appropriate size within the frame buffer.
                frameBuffer.Size = frameBufferSize;
                frameBuffer.Bind();
                return new ValueInvokeOnDisposal<FrameBuffer>( frameBuffer, b => b.Unbind() );
            }

            private long updateVersion;
            protected long GetDrawVersion () => updateVersion;
            public bool AddChildDrawNodes => true;
            public List<DrawNode> Children {
                get => Child.Children;
                set => Child.Children = value;
            }
            private IDisposable establishFrameBufferViewport () {
                // Disable masking for generating the frame buffer since masking will be re-applied
                // when actually drawing later on anyways. This allows more information to be captured
                // in the frame buffer and helps with cached buffers being re-used.
                RectangleI screenSpaceMaskingRect = new RectangleI( (int)Math.Floor( screenSpaceDrawRectangle.X ), (int)Math.Floor( screenSpaceDrawRectangle.Y ), (int)frameBufferSize.X + 1, (int)frameBufferSize.Y + 1 );

                GLWrapper.PushMaskingInfo( new MaskingInfo {
                    ScreenSpaceAABB = screenSpaceMaskingRect,
                    MaskingRect = screenSpaceDrawRectangle,
                    ToMaskingSpace = Matrix3.Identity,
                    BlendRange = 1,
                    AlphaExponent = 1,
                }, true );

                // Match viewport to FrameBuffer such that we don't draw unnecessary pixels.
                GLWrapper.PushViewport( new RectangleI( 0, 0, (int)frameBufferSize.X, (int)frameBufferSize.Y ) );
                GLWrapper.PushScissor( new RectangleI( 0, 0, (int)frameBufferSize.X, (int)frameBufferSize.Y ) );
                GLWrapper.PushScissorOffset( screenSpaceMaskingRect.Location );

                return new ValueInvokeOnDisposal<XrPanelDrawNode>( this, d => d.returnViewport() );
            }

            private void returnViewport () {
                GLWrapper.PopScissorOffset();
                GLWrapper.PopViewport();
                GLWrapper.PopScissor();
                GLWrapper.PopMaskingInfo();
            }

            protected override void Dispose ( bool isDisposing ) {
                base.Dispose( isDisposing );

                Child?.Dispose();
                Child = null;
            }
        }
    }
}
