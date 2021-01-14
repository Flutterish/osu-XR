using osuTK.Graphics;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace osu.XR.Graphics {
	public static class ColorMixing {
		public static Color4 MixAdditive ( Color4 from, Color4 to, float amout ) {
			var R = MathF.Sqrt( from.R * from.R * ( 1 - amout ) + to.R * to.R * amout );
			var G = MathF.Sqrt( from.G * from.G * ( 1 - amout ) + to.G * to.G * amout );
			var B = MathF.Sqrt( from.B * from.B * ( 1 - amout ) + to.B * to.B * amout );
			var A = from.A + ( to.A - from.A ) * amout;

			return new Color4( R, G, B, A );
		}

		public static Rgba32 ToRbga32 ( this Color4 color )
			=> new Rgba32( color.R, color.G, color.B, color.A );
	}
}
