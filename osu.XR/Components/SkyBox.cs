﻿using osu.XR.Graphics;
using osu.XR.Projection;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.XR.Components {
	/// <summary>
	/// A hot pink skybox that fits the osu theme.
	/// </summary>
	public class SkyBox : MeshedXrObject, IBehindEverything {
		public SkyBox () {
			Texture = Textures.VerticalGradient( Color4.Black, Color4.HotPink, 100 ).TextureGL;
			Mesh.Vertices.AddRange( new[] {
				new Vector3(  1,  1,  1 ) * 6,
				new Vector3(  1,  1, -1 ) * 6,
				new Vector3(  1, -1,  1 ) * 6,
				new Vector3(  1, -1, -1 ) * 6,
				new Vector3( -1,  1,  1 ) * 6,
				new Vector3( -1,  1, -1 ) * 6,
				new Vector3( -1, -1,  1 ) * 6,
				new Vector3( -1, -1, -1 ) * 6,
			} );
			Mesh.TextureCoordinates.AddRange( new[] {
				new Vector2( 0, 0 ),
				new Vector2( 0, 0 ),
				new Vector2( 0, 1 ),
				new Vector2( 0, 1 ),
				new Vector2( 0, 0 ),
				new Vector2( 0, 0 ),
				new Vector2( 0, 1 ),
				new Vector2( 0, 1 )
			} );
			Mesh.Tris.AddRange( new IndexedFace[] {
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
			Mesh.TextureCoordinates.AddRange( new[] {
				new Vector2( 0, 1 ),
				new Vector2( 1, 1 ),
				new Vector2( 1, 0 ),
				new Vector2( 0, 0 )
			} );
			Mesh.Tris.AddRange( new IndexedFace[] {
				new( 0, 2, 1 ),
				new( 0, 2, 3 )
			} );
		}

		public override void BeforeDraw ( XrObjectDrawNode.DrawSettings settings ) {
			base.BeforeDraw( settings );
			Position = settings.Camera.Position;
		}
	}
}
