using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Vertices;
using osu.Framework.Graphics.Shaders;
using osu.XR.Components;
using osu.XR.Shaders;
using osuTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace osu.XR.Rendering {
	public class GlXrDrawable : Drawable {
        public Mesh Mesh = new();
        public IShader Shader3D { get; private set; }
        [BackgroundDependencyLoader]
        private void load ( ShaderManager shaders ) {
            Shader3D = shaders.Load( XrShader.VERTEX_3D, XrShader.FRAGMENT_3D );
        }

        protected override DrawNode CreateDrawNode ()
            => new GlXrDrawNode( this );


        private class GlXrDrawNode : DrawNode {
            private GlXrDrawable source => (GlXrDrawable)base.Source;
            public GlXrDrawNode ( GlXrDrawable source ) : base( source ) { }

            private bool notInitialized = true;
            private bool isMeshInvalidated = true;
            private IShader shader;
            private Mesh mesh;
            public override void ApplyState () {
                base.ApplyState();
                shader = source.Shader3D;
                mesh = source.Mesh;
            }

            private int VAO;
            private int VertexBuffer;
            private int EBO;
            private int indiceCount;
            protected void Initialize () {
                VAO = GL.GenVertexArray();
                VertexBuffer = GL.GenBuffer();
                EBO = GL.GenBuffer();
            }
            protected void UpdateMesh () {
                float[] vertices = {
                     0.5f,  0.5f, 0.0f,  // top right
                     0.5f, -0.5f, 0.0f,  // bottom right
                    -0.5f, -0.5f, 0.0f,  // bottom left
                    -0.5f,  0.5f, 0.0f   // top left
                };
                uint[] indices = {  // note that we start from 0!
                    0, 1, 3,   // first triangle
                    1, 2, 3    // second triangle
                };

                GL.BindVertexArray( VAO );
                indiceCount = mesh.UploadToGPU( AttribLocation( "vertex" ), VertexBuffer, EBO );
                GL.BindVertexArray( 0 );
            }
            public override void Draw ( Action<TexturedVertex2D> vertexAction ) {
                base.Draw( vertexAction );

                if ( !( shader as Shader ).IsLoaded ) return;

                if ( notInitialized ) {
                    Initialize();
                    notInitialized = false;
                }

                if ( isMeshInvalidated ) {
                    UpdateMesh();
                    isMeshInvalidated = false;
                }

                shader.Bind();
                GL.BindVertexArray( VAO );
                GL.DrawElements( PrimitiveType.Triangles, indiceCount, DrawElementsType.UnsignedInt, 0 );
                GL.BindVertexArray( 0 );
                shader.Unbind();
            }

            protected override void Dispose ( bool isDisposing ) {
                base.Dispose( isDisposing );
                GL.DeleteVertexArray( VAO );
                GL.DeleteBuffer( VertexBuffer );
                GL.DeleteBuffer( EBO );
            }

            private Dictionary<string, int> attribs = new();
            protected int AttribLocation ( string name ) {
                if ( attribs.TryGetValue( name, out var handle ) ) return handle;
                handle = GL.GetAttribLocation( shader as Shader /* this casts to int */, name );
                attribs.Add( name, handle );
                return handle;
            }
        }
	}
}
