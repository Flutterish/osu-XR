using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;
using osu.XR.Configuration;
using osu.XR.Graphics.Particles;
using osuTK.Graphics;

namespace osu.XR.Graphics.Sceneries.Components;

public partial class DustEmitter : SpriteParticleEmitter<DustParticle2> {
	// TODO track player
	//[Resolved]
	//VrPlayer player { get; set; }

	public DustEmitter () {
		RenderStage = RenderingStage.Transparent;
	}

	protected override DustParticle2 CreateParticle () {
		return new DustParticle2 {
			TotalLifetime = 1200,
			InitialPosition = new Vector3( MathF.CopySign( RNG.NextSingle( 0.5f, 5 ), RNG.NextSingle( -1, 1 ) ), RNG.NextSingle( 0, 6 ), MathF.CopySign( RNG.NextSingle( 0.5f, 5 ), RNG.NextSingle( -1, 1 ) ) ) /*+ player.GlobalPosition.With( y: 0 )*/,
			Velocity = new Vector3( RNG.NextSingle( -1, 1 ), RNG.NextSingle( -1, 1 ), RNG.NextSingle( -1, 1 ) ) * 0.1f / 1200
		};
	}

	double emitTimer;
	double emitInterval = 5;
	protected override void Update () {
		base.Update();

		if ( !showDust.Value ) return;

		if ( ActiveParticles < 300 ) {
			emitTimer += Time.Elapsed;
		}
		else emitTimer = 0;
		while ( ActiveParticles < 300 && emitTimer > emitInterval ) {
			Emit();
			emitTimer -= emitInterval;
		}
	}

	Bindable<bool> showDust = new( true );

	[BackgroundDependencyLoader]
	private void load ( OsuXrConfigManager config, TextureStore textures ) {
		config.BindWith( OsuXrSetting.ShowDust, showDust );
		Material.SetTexture( "tex", textures.Get( "dust" ) );
	}

	protected override bool UpdateParticle ( ref DustParticle2 particle, float deltaTime ) {
		particle.Lifetime += deltaTime;
		return particle.Lifetime < particle.TotalLifetime;
	}

	protected override Material CreateDefaultMaterial ( MaterialStore materials ) {
		return materials.GetNew( Materials.MaterialNames.Transparent );
	}

	protected override SpriteParticleEmitter<DustParticle2>.DrawNode CreateDrawNode3D ( int subtreeIndex )
		=> new DrawNode( this, subtreeIndex );

	new class DrawNode : SpriteParticleEmitter<DustParticle2>.DrawNode {
		public DrawNode ( BatchedParticleEmitter<DustParticle2, BasicMesh> source, int index ) : base( source, index ) { }

		Framework.XR.Graphics.Shaders.IUniform<Color4> tint = null!;
		Color4 tintValue;
		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			tint = Material.Shader.GetUniform<Color4>( "tint" );
			tintValue = Material.Get<Color4>( "tint" );
			base.Draw( renderer, ctx );
		}

		protected override void Draw ( in DustParticle2 particle, IRenderer renderer, object? ctx = null ) {
			tintValue.A = particle.Alpha;
			tint.UpdateValue( ref tintValue );
			base.Draw( particle, renderer, ctx );
		}
	}
}

public struct DustParticle2 : IHasMatrix {
	float fadeInDuration => TotalLifetime / 3;
	public float Alpha => Lifetime < fadeInDuration
		? Interpolation.ValueAt( Lifetime, 0f, 1f, 0, fadeInDuration, Easing.Out )
		: Interpolation.ValueAt( Lifetime, 1f, 0f, fadeInDuration, TotalLifetime, Easing.In );
	public float TotalLifetime;
	public float Lifetime;
	public Vector3 InitialPosition;
	public Vector3 Position => InitialPosition + Velocity * Lifetime;
	public Vector3 Velocity;

	public Matrix4 Matrix => Matrix4.CreateScale( 0.03f ) * Matrix4.CreateTranslation( Position );
}