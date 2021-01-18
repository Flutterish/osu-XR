using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Shaders;
using osu.XR.Graphics;
using osu.XR.Projection;
using osuTK;
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
		public readonly XrObject Root = new XrObject() { RelativeSizeAxes = Axes.Both };
		public Camera Camera;

		public static implicit operator XrObject ( XrScene scene )
			=> scene.Root;

		[BackgroundDependencyLoader]
		private void load ( ShaderManager shaders ) {
			XrShader.Shader3D ??= shaders.Load( XrShader.VERTEX_3D, XrShader.FRAGMENT_3D ) as Shader;
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
