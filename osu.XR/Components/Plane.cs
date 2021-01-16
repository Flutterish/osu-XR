using osu.XR.Graphics;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.XR.Components {
	public class Plane : XrMesh {
		public Plane () {
			Mesh = new();
			var z = 0;
			Mesh.AddAABBQuad(
				new Maths.Quad( 
					new osuTK.Vector3( -2, 2, z ), new osuTK.Vector3( 2, 2, z ), 
					new osuTK.Vector3( -2, -2, z ), new osuTK.Vector3( 2, -2, z ) 
				)	
			);
			Texture = Textures.Pixel( Color4.Red ).TextureGL;
		}
	}
}
