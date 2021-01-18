using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Buffers;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shaders;
using osu.XR.Graphics;
using osu.XR.Projection;
using osuTK;
using osuTK.Graphics.OpenGL4;
using System;

namespace osu.XR.Components {
	/// <summary>
	/// A scene containing Xr objects.
	/// </summary>
	public class XrScene : Container {
		public XrScene () {
			Add( new XrSceneDrawer( this ) );
			Add( Root );
		}
		public bool RenderToScreen { get => RenderToScreenBindable.Value; set => RenderToScreenBindable.Value = value; }
		public readonly BindableBool RenderToScreenBindable = new( true );
		public readonly XrObject Root = new XrObject();
		public Camera Camera;

		public static implicit operator XrObject ( XrScene scene )
			=> scene.Root;

		private IShader TextureShader;
		private FrameBuffer depthBuffer = new FrameBuffer( new osuTK.Graphics.ES30.RenderbufferInternalFormat[] { osuTK.Graphics.ES30.RenderbufferInternalFormat.DepthComponent32f } );
		[BackgroundDependencyLoader]
		private void load ( ShaderManager shaders ) {
			XrShader.Shader3D ??= shaders.Load( XrShader.VERTEX_3D, XrShader.FRAGMENT_3D ) as Shader;
			TextureShader = shaders.Load( VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE );
		}

		protected override void Dispose ( bool isDisposing ) {
			base.Dispose( isDisposing );
			depthBuffer.Dispose();
		}

		private class XrSceneDrawer : Drawable { // for whatever reason o!f doesnt use the XrScenes draw node ( prolly bc its a container )
			public XrScene Scene;

			public XrSceneDrawer ( XrScene scene ) {
				Scene = scene;
			}

			protected override DrawNode CreateDrawNode ()
				=> new XrSceneDrawNode( Scene );
		}

		private class XrSceneDrawNode : DrawNode {
			new private XrScene Source;
			public XrSceneDrawNode ( XrScene source ) : base( source ) {
				Source = source;
			}

			Vector2 size;
			Quad quad; 
			IShader textureShader;
			public override void ApplyState () {
				base.ApplyState();
				size = Source.DrawSize;
				quad = Source.ScreenSpaceDrawQuad;
				quad = new Quad( quad.BottomLeft, quad.BottomRight, quad.TopLeft, quad.TopRight );
				textureShader = Source.TextureShader;
			}

			public override void Draw ( Action<TexturedVertex2D> vertexAction ) {
				if ( !Source.RenderToScreen ) return;

				if ( Source.depthBuffer.Size != size ) Source.depthBuffer.Size = size;

				Source.depthBuffer.Bind();
				GLWrapper.PushDepthInfo( new DepthInfo( true, true, osuTK.Graphics.ES30.DepthFunction.Less ) );
				GL.Enable( EnableCap.DepthTest );
				GL.DepthMask( true );
				GL.DepthFunc( DepthFunction.Less );
				GL.DepthRange( 0.0, 1.0 );
				GL.ClearDepth( 1 );
				GL.Clear( ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit );
				Source.Camera?.Render( Source.Root, size.X, size.Y );
				GLWrapper.PopDepthInfo();
				Source.depthBuffer.Unbind();

				base.Draw( vertexAction );
				if ( Source.depthBuffer.Texture.Bind() ) {
					textureShader.Bind();
					DrawQuad( Source.depthBuffer.Texture, quad, DrawColourInfo.Colour );
					textureShader.Unbind();
				}
			}
		}
	}
}
