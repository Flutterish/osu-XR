using Microsoft.EntityFrameworkCore.Internal;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.XR;
using osu.Framework.XR.Graphics;
using osu.XR.Maths;
using osuTK;
using osuTK.Graphics.OpenGL4;
using System.Collections.Generic;

namespace osu.XR.Components {
	/// <summary>
	/// An <see cref="XrObject"/> which renders a <see cref="Framework.XR.Graphics.Mesh"/>.
	/// </summary>
	public class MeshedXrObject : XrObject {
		public bool IsVisible = true;
		public Mesh Mesh { get; set; } = new();
		public bool UseGammaCorrection = false;
		public TextureGL MainTexture {
			get => AllTextures[ 0 ];
			set => AllTextures[ 0 ] = value;
		}
		public readonly List<TextureGL> AllTextures = new();
		public MeshedXrObject () {
			Faces = new( i => Transform.Matrix * Mesh.Faces[ i ] );
			AllTextures.Add( osu.Framework.Graphics.Textures.Texture.WhitePixel.TextureGL );
		}
		protected override XrObjectDrawNode CreateDrawNode ()
			=> new XrMeshDrawNode( this );

		public readonly ReadonlyIndexer<int,Face> Faces;

		protected override Vector3 RequiredParentSizeToFit => Mesh.BoundingBox.Size;
		public override Vector3 Centre => Mesh.BoundingBox.Min + Mesh.BoundingBox.Size / 2;
	}
	public class XrMeshDrawNode : XrMeshDrawNode<MeshedXrObject> {
		public XrMeshDrawNode ( MeshedXrObject source ) : base( source ) { }
	}
	public class XrMeshDrawNode<T> : XrObject.XrObjectDrawNode<T> where T : MeshedXrObject {
		public XrMeshDrawNode ( T source ) : base( source ) { }
		protected virtual Mesh GetMesh () => Source.Mesh;

		private bool notInitialized = true;
		private Mesh mesh;
		private ulong lastUpdateVersion;

		public override void Draw ( DrawSettings settings ) {
			if ( !Source.IsVisible ) return;

			var newMesh = GetMesh();
			if ( mesh != newMesh ) {
				mesh = newMesh;
				lastUpdateVersion = 0;
			}

			if ( !XrShader.Shader3D.IsLoaded ) return;
			if ( mesh is null ) return;

			if ( notInitialized ) {
				Initialize();
				notInitialized = false;
			}

			if ( lastUpdateVersion != mesh.UpdateVersion && mesh.IsReady ) {
				UpdateMesh();
				lastUpdateVersion = mesh.UpdateVersion;
			}

			for ( int i = 0; i < Source.AllTextures.Count; i++ ) {
				if ( !Source.AllTextures[ i ].Bind( osuTK.Graphics.ES30.TextureUnit.Texture0 + i ) ) return;
			}

			XrShader.Shader3D.Bind();
			GL.BindVertexArray( VAO );

			var a = settings.WorldToCamera;
			var b = settings.CameraToClip;
			var c = (Matrix4)Source.Transform.Matrix;

			GL.UniformMatrix4( GL.GetUniformLocation( XrShader.Shader3D, XrShader.VERTEX_3D.WorldToCameraMatrix ), true, ref a );
			GL.UniformMatrix4( GL.GetUniformLocation( XrShader.Shader3D, XrShader.VERTEX_3D.CameraToClipMatrix ), true, ref b );
			GL.UniformMatrix4( GL.GetUniformLocation( XrShader.Shader3D, XrShader.VERTEX_3D.LocalToWorldMatrix ), true, ref c );
			GL.Uniform1( GL.GetUniformLocation( XrShader.Shader3D, XrShader.FRAGMENT_3D.UseGammaCorrection ), Source.UseGammaCorrection ? 1 : 0 );
			GL.DrawElements( PrimitiveType.Triangles, indiceCount, DrawElementsType.UnsignedInt, 0 );
			GL.BindVertexArray( 0 );
			XrShader.Shader3D.Unbind();
		}

		private int VAO;
		private int buffer;
		private int EBO;
		private int indiceCount;
		protected void Initialize () {
			VAO = GL.GenVertexArray();
			buffer = GL.GenBuffer();
			EBO = GL.GenBuffer();
		}
		protected void UpdateMesh () {
			GL.BindVertexArray( VAO );
			indiceCount = mesh.UploadToGPU( AttribLocation( "vertex" ), AttribLocation( "UV" ), buffer, EBO );
			GL.BindVertexArray( 0 );
		}

		public override void Dispose () {
			base.Dispose();
			GL.DeleteVertexArray( VAO );
			GL.DeleteBuffer( buffer );
			GL.DeleteBuffer( EBO );
		}

		private Dictionary<string, int> attribs = new();
		protected int AttribLocation ( string name ) {
			if ( attribs.TryGetValue( name, out var handle ) ) return handle;
			handle = GL.GetAttribLocation( XrShader.Shader3D, name );
			attribs.Add( name, handle );
			return handle;
		}
	}
}
