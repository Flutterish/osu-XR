using osu.Framework.Graphics.Textures;
using osuTK.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace osu.XR.Graphics {
	public static class Textures {
		public static Texture Generate ( int width, int height, Func<int, int, Rgba32> generator ) {
			Image<Rgba32> image = new Image<Rgba32>( width, height );
			for ( int y = 0; y < height; y++ ) {
				var span = image.GetPixelRowSpan( y );
				for ( int x = 0; x < width; x++ ) {
					span[ x ] = generator( x, y );
				}
			}
			Texture texture = new Texture( width, height, true );
			texture.SetData( new TextureUpload( image ) );
			return texture;
		}
		public static Texture GeneratePercentile ( int width, int height, Func<double, double, Rgba32> generator )
			=> Generate( width, height, ( x, y ) => generator( (double)x / width, (double)y / height ) );
		public static Texture GenerateMirroredPercentile ( int width, int height, Func<double, double, Rgba32> generator )
			=> Generate( width, height, ( x, y ) => generator( 1 - Math.Abs( 1 - x * 2d / width ), 1 - Math.Abs( 1 - y * 2d / height ) ) );
		public static Texture Pixel ( Color4 color ) {
			Image<Rgba32> image = new Image<Rgba32>( 1, 1, new Rgba32( color.R, color.G, color.B, color.A ) );
			Texture texture = new Texture( 1, 1, true );
			texture.SetData( new TextureUpload( image ) );
			return texture;
		}
		public static Texture VerticalGradient ( Color4 top, Color4 bottom, int height )
			=> GeneratePercentile( 10, height, ( x, y ) => ColorMixing.MixAdditive( top, bottom, (float)y ).ToRbga32() );

		public static Texture Vertical2SidedGradient ( Color4 edge, Color4 center, int height )
			=> GenerateMirroredPercentile( 10, height, ( x, y ) => ColorMixing.MixAdditive( edge, center, (float)y ).ToRbga32() );
	}
}
