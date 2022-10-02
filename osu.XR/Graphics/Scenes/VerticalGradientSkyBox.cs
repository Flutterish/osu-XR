using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics.Vertices;
using osuTK.Graphics;

namespace osu.XR.Graphics.Scenes;

public class VerticalGradientSkyBox : BasicModel {
	public readonly Bindable<Color4> TintBindable = new( new Color4( 253, 35, 115, 255 ) );
	public readonly BindableFloat OpacityBindable = new( 1 ) { MinValue = 0, MaxValue = 1 };

	public VerticalGradientSkyBox () {
		RenderStage = RenderingStage.Skybox;

		Mesh.Vertices.AddRange( new TexturedNormal[] {
			new() { Position = new(  1,  1,  1 ), UV = new( 0, 0 ) },
			new() { Position = new(  1,  1, -1 ), UV = new( 0, 0 ) },
			new() { Position = new(  1, -1,  1 ), UV = new( 0, 1 ) },
			new() { Position = new(  1, -1, -1 ), UV = new( 0, 1 ) },
			new() { Position = new( -1,  1,  1 ), UV = new( 0, 0 ) },
			new() { Position = new( -1,  1, -1 ), UV = new( 0, 0 ) },
			new() { Position = new( -1, -1,  1 ), UV = new( 0, 1 ) },
			new() { Position = new( -1, -1, -1 ), UV = new( 0, 1 ) }
		} );
		Mesh.Indices.AddRange( new uint[] {
			4, 7, 5,
			4, 7, 6,
			4, 2, 6,
			4, 2, 0,
			5, 3, 7,
			5, 3, 1,
			6, 3, 7,
			6, 3, 2,
			0, 2, 1,
			1, 2, 3
		} );
		Mesh.CreateFullUnsafeUpload().Enqueue();

		TintBindable.BindValueChanged( v => Colour = v.NewValue, true );
		OpacityBindable.BindValueChanged( v => Alpha = v.NewValue, true );
	}

	[BackgroundDependencyLoader]
	private void load ( IRenderer renderer ) {
		Material.Set( "useGamma", true );
		Material.SetTexture( "tex", TextureGeneration.VerticalGradient( renderer, Color4.Black, Color4.White, 100, x => MathF.Pow( x, 2 ) ) );
	}

	protected override ModelDrawNode? CreateDrawNode3D ( int index )
		=> new DrawNode( this, index );

	class DrawNode : ModelDrawNode {
		public DrawNode ( Model<BasicMesh> source, int index ) : base( source, index ) { }

		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			var camera = new Matrix3( renderer.ProjectionMatrix ) * -renderer.ProjectionMatrix.Row3.Xyz;
			Matrix = Matrix4.CreateTranslation( camera );
			base.Draw( renderer, ctx );
		}
	}
}
