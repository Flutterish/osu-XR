using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Allocation;
using osu.Framework.XR.Collections;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;

namespace osu.XR.Graphics.Particles;

public abstract partial class BatchedParticleEmitter<T, Tmesh> : MeshRenderer<Tmesh> where T : struct, IHasMatrix where Tmesh : Mesh {
	List<T> particles = new();
	List<T> nextParticles = new();

	public int ActiveParticles => particles.Count;

	protected abstract T CreateParticle ();
	public ref T Emit () {
		particles.Add( CreateParticle() );
		return ref particles.AsSpan()[particles.Count - 1];
	}

	protected abstract bool UpdateParticle ( ref T particle, float deltaTime );
	protected override void Update () {
		base.Update();

		var delta = (float)Time.Elapsed;
		foreach ( ref var i in particles.AsSpan() ) {
			if ( UpdateParticle( ref i, delta ) )
				nextParticles.Add( i );
		}

		(particles, nextParticles) = (nextParticles, particles);
		nextParticles.Clear();
		Invalidate( Invalidation.DrawNode );
	}

	protected override DrawNode CreateDrawNode3D ( int subtreeIndex )
		=> new( this, subtreeIndex );

	protected class DrawNode : MeshRendererDrawNode {
		new protected BatchedParticleEmitter<T, Tmesh> Source => (BatchedParticleEmitter<T, Tmesh>)base.Source;
		
		RentedArray<T>? particles;
		public DrawNode ( BatchedParticleEmitter<T, Tmesh> source, int index ) : base( source, index ) { }

		protected override void UpdateState () {
			base.UpdateState();
			particles?.Dispose();
			particles = MemoryPool<T>.Shared.Rent( Source.particles );
		}

		protected virtual void Draw ( in T particle, IRenderer renderer, object? ctx = null ) {
			Material.Shader.SetUniform( "mMatrix", particle.Matrix * Matrix );
			Mesh!.Draw();
		}

		public override void Draw ( IRenderer renderer, object? ctx = null ) {
			if ( Mesh is null )
				return;

			Bind();

			foreach ( ref var i in particles!.Value.AsSpan() ) {
				Draw( i, renderer, ctx );
			}
		}

		public override void Dispose () {
			base.Dispose();
			particles?.Dispose();
		}
	}
}
