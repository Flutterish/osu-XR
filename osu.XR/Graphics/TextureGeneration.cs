using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osuTK.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace osu.XR.Graphics;

public static class TextureGeneration {
	public static Texture Generate ( IRenderer renderer, int width, int height, Func<int, int, Rgba32> generator ) {
		Image<Rgba32> image = new Image<Rgba32>( width, height );
		image.ProcessPixelRows( rows => {
			for ( int y = 0; y < height; y++ ) {
				var span = rows.GetRowSpan( y );
				for ( int x = 0; x < width; x++ ) {
					span[x] = generator( x, y );
				}
			}
		} );

		Texture texture = renderer.CreateTexture( width, height, true );
		texture.SetData( new TextureUpload( image ) );
		return texture;
	}
	public static Texture GeneratePercentile ( IRenderer renderer, int width, int height, Func<double, double, Rgba32> generator )
		=> Generate( renderer, width, height, ( x, y ) => generator( (double)x / width, (double)y / height ) );
	public static Texture GenerateMirroredPercentile ( IRenderer renderer, int width, int height, Func<double, double, Rgba32> generator )
		=> Generate( renderer, width, height, ( x, y ) => generator( 1 - Math.Abs( 1 - x * 2d / width ), 1 - Math.Abs( 1 - y * 2d / height ) ) );
	public static Texture Pixel ( IRenderer renderer, Color4 color ) {
		Image<Rgba32> image = new Image<Rgba32>( 1, 1, new Rgba32( color.R, color.G, color.B, color.A ) );
		Texture texture = renderer.CreateTexture( 1, 1, true );
		texture.SetData( new TextureUpload( image ) );
		return texture;
	}
	public static Texture VerticalGradient ( IRenderer renderer, Color4 top, Color4 bottom, int height )
		=> GeneratePercentile( renderer, 10, height, ( x, y ) => ColorMixing.MixAdditive( top, bottom, (float)y ).ToRbga32() );

	public static Texture VerticalGradient ( IRenderer renderer, Color4 top, Color4 bottom, int height, Func<float, float> amoutTransform )
		=> GeneratePercentile( renderer, 10, height, ( x, y ) => ColorMixing.MixAdditive( top, bottom, amoutTransform( (float)y ) ).ToRbga32() );

	public static Texture Vertical2SidedGradient ( IRenderer renderer, Color4 edge, Color4 center, int height )
		=> GenerateMirroredPercentile( renderer, 10, height, ( x, y ) => ColorMixing.MixAdditive( edge, center, (float)y ).ToRbga32() );
}

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