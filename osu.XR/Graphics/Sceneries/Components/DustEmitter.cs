using osu.Framework.Graphics.Textures;
using osu.Framework.Utils;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Transforms;
using osu.XR.Configuration;
using osu.XR.Graphics.Particles;

namespace osu.XR.Graphics.Sceneries.Components;

public partial class DustEmitter : ParticleEmitter<DustParticle> {
	protected override DustParticle CreateParticle ()
		=> new();

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
	private void load ( OsuXrConfigManager config ) {
		//config.BindWith( OsuXrConfigManager.ShowDust, showDust );
	}
}

public partial class DustParticle : ParticleEmitter.Particle {
	public DustParticle () {
		RenderStage = RenderingStage.Transparent;
	}

	// TODO track player
	//[Resolved]
	//VrPlayer player { get; set; }

	[BackgroundDependencyLoader]
	private void load ( TextureStore textures ) {
		var all = textures.GetAvailableResources();
		Material.SetTexture( "tex", textures.Get( "dust" ) );
	}

	protected override Material CreateDefaultMaterial ( MaterialStore materials ) {
		return materials.GetNew( Materials.MaterialNames.Transparent );
	}

	protected override void OnApply ( ParticleEmitter emmiter ) {
		base.OnApply( emmiter );

		this.FadeInFromZero( 400, Easing.Out ).Then().FadeOut( 800, Easing.In ).Then().Schedule( () => Release() );
		this.MoveTo( new Vector3( MathF.CopySign( RNG.NextSingle( 0.5f, 5 ), RNG.NextSingle( -1, 1 ) ), RNG.NextSingle( 0, 6 ), MathF.CopySign( RNG.NextSingle( 0.5f, 5 ), RNG.NextSingle( -1, 1 ) ) ) /*+ player.GlobalPosition.With( y: 0 )*/ )
			.MoveToOffset( new Vector3( RNG.NextSingle( -1, 1 ), RNG.NextSingle( -1, 1 ), RNG.NextSingle( -1, 1 ) ) * 0.1f, 1200 );
	}
}