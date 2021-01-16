using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Buffers;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Shaders;
using osu.XR.Graphics;
using osu.XR.Projection;
using osu.XR.Rendering;
using osuTK;
using osuTK.Graphics.ES30;
using osuTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.XR.Components {
	public class XrScene : Drawable {
		public readonly XrObject Root = new XrObject();
		public Camera Camera;

		public static implicit operator XrObject ( XrScene scene )
			=> scene.Root;

		[BackgroundDependencyLoader]
		private void load ( ShaderManager shaders ) {
			XrShader.Shader3D ??= shaders.Load( XrShader.VERTEX_3D, XrShader.FRAGMENT_3D ) as Shader;
		}

		protected override DrawNode CreateDrawNode ()
			=> new XrSceneDrawNode( this );

		private class XrSceneDrawNode : DrawNode {
			new private XrScene Source;
			public XrSceneDrawNode ( XrScene source ) : base( source ) {
				Source = source;
			}

			Vector2 size;
			public override void ApplyState () {
				base.ApplyState();
				size = Source.DrawSize;
			}

			public override void Draw ( Action<TexturedVertex2D> vertexAction ) {
				base.Draw( vertexAction );
				Source.Camera?.Render( Source.Root, size.X, size.Y );

			}
		}
	}
}
