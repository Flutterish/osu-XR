﻿using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;
using osuTK.Graphics;

namespace osu.XR.Components {
	/// <summary>
	/// A simple quad for testing purposes.
	/// </summary>
	public class Plane : MeshedXrObject {
		public Plane () {
			Mesh = new();
			var z = 0;
			Mesh.AddQuad(
				new Quad( 
					new osuTK.Vector3( -2, 2, z ), new osuTK.Vector3( 2, 2, z ), 
					new osuTK.Vector3( -2, -2, z ), new osuTK.Vector3( 2, -2, z ) 
				)	
			);
			MainTexture = Textures.Pixel( Color4.Red ).TextureGL;
		}
	}
}
