using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Buffers;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Layout;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;

namespace osu.XR.Rendering {
	/// <summary>
	/// A composite drawable which renders other drawables into a back buffer but does not draw them on screen.
	/// </summary>
	public class BufferedCapture : Container {
        public BufferedCapture () {
            sharedData = new BufferedDrawNodeSharedData( null, false ); // TODO customize this class ( might fix the cursor jitter bc double buffers or whatever coulbe be in there )
        }

        public ColourInfo EffectColour = Color4.White;
        public DrawColourInfo? FrameBufferDrawColour => base.DrawColourInfo;
        public Vector2 FrameBufferScale { get; set; } = Vector2.One;
        public Color4 BackgroundColour { get; set; } = new Color4( 0, 0, 0, 0 ); // TODO this should be user adjustable
        public IShader TextureShader { get; private set; }
        [BackgroundDependencyLoader]
        private void load ( ShaderManager shaders ) {
            TextureShader = shaders.Load( VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE );
        }
        public TextureGL Capture; // TODO make readonly ( reads from shared data )

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
        protected override DrawNode CreateDrawNode () => new BufferedCaptureDrawNode( this, sharedData );
        protected override void Dispose ( bool isDisposing ) {
            base.Dispose( isDisposing );
            sharedData.Dispose();
        }

        private class BufferedCaptureDrawNode : DrawNode, ICompositeDrawNode {
            protected IShader TextureShader;
            protected readonly BufferedDrawNodeSharedData SharedData;
            protected CompositeDrawableDrawNode Child;
            protected RectangleF DrawRectangle { get; private set; }
            private Color4 backgroundColour;
            private RectangleF screenSpaceDrawRectangle;
            private Vector2 frameBufferScale;
            private Vector2 frameBufferSize;
            private ColourInfo effectColour;
            protected new DrawColourInfo DrawColourInfo { get; private set; }

            new BufferedCapture Source => (BufferedCapture)base.Source;
            public BufferedCaptureDrawNode ( BufferedCapture source, BufferedDrawNodeSharedData sharedData )
                : base( source ) {
                Child = new CompositeDrawableDrawNode( source );
                SharedData = sharedData;
            }

            public override void ApplyState () {
                base.ApplyState();
                TextureShader = Source.TextureShader;
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
                var finalEffectColour = DrawColourInfo.Colour;
                finalEffectColour.ApplyChild( effectColour );

                using ( establishFrameBufferViewport() ) {
                    using ( BindFrameBuffer( SharedData.MainBuffer ) ) {
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
                Source.Capture = SharedData.CurrentEffectBuffer.Texture;
            }
            protected IDisposable BindFrameBuffer ( FrameBuffer frameBuffer ) {
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
                RectangleI screenSpaceMaskingRect = new RectangleI( (int)Math.Floor( screenSpaceDrawRectangle.X ), (int)Math.Floor( screenSpaceDrawRectangle.Y ), (int)frameBufferSize.X + 1, (int)frameBufferSize.Y + 1 );

                GLWrapper.PushMaskingInfo( new MaskingInfo {
                    ScreenSpaceAABB = screenSpaceMaskingRect,
                    MaskingRect = screenSpaceDrawRectangle,
                    ToMaskingSpace = Matrix3.Identity,
                    BlendRange = 1,
                    AlphaExponent = 1,
                }, true );

                GLWrapper.PushViewport( new RectangleI( 0, 0, (int)frameBufferSize.X, (int)frameBufferSize.Y ) );
                GLWrapper.PushScissor( new RectangleI( 0, 0, (int)frameBufferSize.X, (int)frameBufferSize.Y ) );
                GLWrapper.PushScissorOffset( screenSpaceMaskingRect.Location );

                return new ValueInvokeOnDisposal<BufferedCaptureDrawNode>( this, d => d.returnViewport() );
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
