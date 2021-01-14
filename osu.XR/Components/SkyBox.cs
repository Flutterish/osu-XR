using osu.XR.Graphics;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.XR.Components {
	public class SkyBox : XrMesh {
		public SkyBox () {
			Texture = Textures.VerticalGradient( Color4.Black, Color4.HotPink, 100 ).TextureGL;
		}
		protected override XrObjectDrawNode CreateDrawNode ()
			=> new SkyBoxDrawNode( this );

		private class SkyBoxDrawNode : XrMeshDrawNode<SkyBox> {
			private Mesh mesh;
			public SkyBoxDrawNode ( SkyBox source ) : base( source ) {
				mesh = new Mesh();
				mesh.Vertices.AddRange( new[] {
					new Vector3(  1,  1,  1 ) * 6,
					new Vector3(  1,  1, -1 ) * 6,
					new Vector3(  1, -1,  1 ) * 6,
					new Vector3(  1, -1, -1 ) * 6,
					new Vector3( -1,  1,  1 ) * 6,
					new Vector3( -1,  1, -1 ) * 6,
					new Vector3( -1, -1,  1 ) * 6,
					new Vector3( -1, -1, -1 ) * 6,
				} );
				mesh.TextureCoordinates.AddRange( new[] {
					new Vector2( 0, 0 ),
					new Vector2( 0, 0 ),
					new Vector2( 0, 1 ),
					new Vector2( 0, 1 ),
					new Vector2( 0, 0 ),
					new Vector2( 0, 0 ),
					new Vector2( 0, 1 ),
					new Vector2( 0, 1 )
				} );
				mesh.Tris.AddRange( new IndexedFace[] {
					new( 4, 7, 5 ),
					new( 4, 7, 6 ),
					new( 0, 3, 2 ),
					new( 0, 3, 1 ),
					new( 4, 2, 6 ),
					new( 4, 2, 0 ),
					new( 5, 3, 7 ),
					new( 5, 3, 1 ),
					new( 6, 3, 7 ),
					new( 6, 3, 2 )
				} );
				mesh.TextureCoordinates.AddRange( new[] {
					new Vector2( 0, 1 ),
					new Vector2( 1, 1 ),
					new Vector2( 1, 0 ),
					new Vector2( 0, 0 )
				} );
				mesh.Tris.AddRange( new IndexedFace[] {
					new( 0, 2, 1 ),
					new( 0, 2, 3 )
				} );
			}

			public override void Draw ( DrawSettings settings ) {
				Source.Position = settings.Camera.Position;
				base.Draw( settings );
			}

			protected override Mesh GetMesh ()
				=> mesh;
		}
	}
}
