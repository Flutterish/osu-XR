using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Particles;
using osu.Framework.XR.VirtualReality;
using osu.Game.Overlays.Settings;
using osu.XR.Graphics.Settings;
using osuTK.Graphics;

namespace osu.XR.Graphics.Sceneries.Components;

public partial class DustEmitter : SpriteParticleEmitter<DustParticle>, IConfigurableSceneryComponent {
	LocalisableString ISceneryComponent.Name => @"Dust";
	public SceneryComponentSettingsSection CreateSettings () => new DustEmitterSettingsSection( this );

	public readonly BindableInt MaxParticles = new( 300 ) { MinValue = 0, MaxValue = 1000 };
	public readonly BindableFloat ParticleLifetime = new( 1200 ) { MinValue = 100, MaxValue = 3000, Precision = 100 };
	public readonly BindableFloat EmitInterval = new( 5 ) { MinValue = 1, MaxValue = 100, Precision = 1 };
	public readonly Bindable<Colour4> TintBindable = new( Colour4.White );

	[Resolved(canBeNull: true)]
	VrPlayer? player { get; set; }
	Vector3 playerPosition;

	public DustEmitter () {
		RenderStage = RenderingStage.Transparent;
		TintBindable.BindValueChanged( v => Colour = v.NewValue, true );
	}

	protected override DustParticle CreateParticle () {
		return new DustParticle {
			TotalLifetime = ParticleLifetime.Value,
			InitialPosition = new Vector3( MathF.CopySign( RNG.NextSingle( 0.5f, 5 ), RNG.NextSingle( -1, 1 ) ), RNG.NextSingle( 0, 6 ), MathF.CopySign( RNG.NextSingle( 0.5f, 5 ), RNG.NextSingle( -1, 1 ) ) ) + playerPosition,
			Velocity = new Vector3( RNG.NextSingle( -1, 1 ), RNG.NextSingle( -1, 1 ), RNG.NextSingle( -1, 1 ) ) * 0.1f / 1200
		};
	}

	double emitTimer;
	protected override void Update () {
		base.Update();
		playerPosition = ( player?.GlobalPosition ?? Vector3.Zero ) with { Y = 0 };

		if ( ActiveParticles < 300 ) {
			emitTimer += Time.Elapsed;
		}
		else emitTimer = 0;
		while ( ActiveParticles < 300 && emitTimer > EmitInterval.Value ) {
			Emit();
			emitTimer -= EmitInterval.Value;
		}
	}

	[BackgroundDependencyLoader]
	private void load ( TextureStore textures ) {
		Material.SetTexture( "tex", textures.Get( "dust" ) );
		if ( colour is Color4 color )
			Material.SetIfDefault( Material.StandardTintName, color );
	}

	Color4? colour = null;
	public override Color4 Tint {
		get => Material?.Get<Color4>( Material.StandardTintName ) ?? colour ?? Color4.White;
		set {
			if ( Tint == value )
				return;

			base.Tint = value;
			colour = value;
			Material?.Set( Material.StandardTintName, value );
			Invalidate( Invalidation.DrawNode );
		}
	}
	override public float Alpha {
		get => Tint.A;
		set => Tint = Tint with { A = value };
	}

	protected override bool UpdateParticle ( ref DustParticle particle, float deltaTime ) {
		particle.Lifetime += deltaTime;
		return particle.Lifetime < particle.TotalLifetime;
	}

	protected override Material CreateDefaultMaterial ( MaterialStore materials ) {
		return materials.GetNew( Materials.MaterialNames.Transparent );
	}

	protected override SpriteParticleEmitterDrawNode CreateDrawNode3D ( int subtreeIndex )
		=> new DrawNode( this, subtreeIndex );

	class DrawNode : SpriteParticleEmitterDrawNode {
		public DrawNode ( DustEmitter source, int index ) : base( source, index ) { }

		Framework.XR.Graphics.Shaders.IUniform<Color4> tint = null!;
		Color4 tintValue;
		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			tint = Material.Shader.GetUniform<Color4>( "tint" );
			tintValue = Material.Get<Color4>( "tint" );
			base.Draw( renderer, ctx );
		}

		protected override void Draw ( in DustParticle particle, IRenderer renderer, object? ctx = null ) {
			tintValue.A = particle.Alpha;
			tint.UpdateValue( ref tintValue );
			base.Draw( particle, renderer, ctx );
		}
	}
}

public struct DustParticle : IHasMatrix {
	float fadeInDuration => TotalLifetime / 3;
	public float Alpha => Lifetime < fadeInDuration
		? Interpolation.ValueAt( Lifetime, 0f, 1f, 0, fadeInDuration, Easing.Out )
		: Interpolation.ValueAt( Lifetime, 1f, 0f, fadeInDuration, TotalLifetime, Easing.In );
	public float TotalLifetime;
	public float Lifetime;
	public Vector3 InitialPosition;
	public Vector3 Position => InitialPosition + Velocity * Lifetime;
	public Vector3 Velocity;

	public float Scale => ( MathF.Sin( Lifetime / TotalLifetime * MathF.PI ) + 1 ) / 4;
	public Matrix4 Matrix => Matrix4.CreateScale( 0.03f * Scale ) * Matrix4.CreateTranslation( Position );
}

public partial class DustEmitterSettingsSection : SceneryComponentSettingsSection {
	public DustEmitterSettingsSection ( DustEmitter source ) : base( source ) {
		Add( new SettingsColourPicker {
			LabelText = @"Tint",
			Current = source.TintBindable
		} );
		Add( new SettingsSlider<int> {
			LabelText = @"Max count",
			Current = source.MaxParticles
		} );
		Add( new SettingsSlider<float, MsSliderBar> {
			LabelText = @"Lifetime",
			Current = source.ParticleLifetime
		} );
		Add( new SettingsSlider<float, MsSliderBar> {
			LabelText = @"Emit Interval",
			Current = source.EmitInterval
		} );
	}
}