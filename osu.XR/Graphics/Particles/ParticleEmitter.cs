using osu.Framework.XR.Graphics;

namespace osu.XR.Graphics.Particles;

/// <inheritdoc/>
public abstract partial class ParticleEmitter<T> : ParticleEmitter where T : ParticleEmitter.Particle {
	protected override abstract T CreateParticle ();
	new protected T Emit () => (T)base.Emit();
}

/// <summary>
/// A primitive particle emmiter that uses sprite particles.
/// </summary>
public abstract partial class ParticleEmitter : CompositeDrawable3D { // TODO batched particle emiter
	readonly List<Particle> particlePool = new();

	public int ActiveParticles { get; private set; } = 0;
	protected abstract Particle CreateParticle ();
	Particle getParticle () {
		var particle = particlePool.FirstOrDefault( x => !x.IsApplied );
		if ( particle is null ) {
			particlePool.Add( particle = CreateParticle() );
		}
		return particle;
	}

	void release ( Particle particle ) {
		RemoveInternal( particle, disposeImmediately: false );
		ActiveParticles--;
	}
	protected Particle Emit () {
		var particle = getParticle();

		AddInternal( particle );
		ActiveParticles++;
		particle.Apply( this );

		return particle;
	}

	protected override void Dispose ( bool isDisposing ) {
		if ( !IsDisposed ) {
			foreach ( var i in particlePool ) {
				i.Dispose();
			}
		}

		base.Dispose( isDisposing );
	}
}
