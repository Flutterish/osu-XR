using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Materials;
using osu.Framework.XR.Graphics.Meshes;

namespace osu.XR.Graphics.Particles;

public abstract partial class SpriteParticleEmitter<T> : BatchedParticleEmitter<T, BasicMesh> where T : struct, IHasMatrix {
	static BasicMesh? _quadMesh;
	static BasicMesh quadMesh {
		get {
			if ( _quadMesh is null ) {
				_quadMesh = new();
				_quadMesh.AddQuad( new Quad3(
					new Vector3( -0.5f, 0.5f, 0 ),
					new Vector3( 0.5f, 0.5f, 0 ),
					new Vector3( -0.5f, -0.5f, 0 ),
					new Vector3( 0.5f, -0.5f, 0 )
				) );
				_quadMesh.CreateFullUnsafeUpload().Enqueue();
			}
			return _quadMesh;
		}
	}

	public SpriteParticleEmitter () {
		Mesh = quadMesh;
	}

	protected override DrawNode CreateDrawNode3D ( int subtreeIndex )
		=> new( this, subtreeIndex );

	new protected class DrawNode : BatchedParticleEmitter<T, BasicMesh>.DrawNode {
		public DrawNode ( BatchedParticleEmitter<T, BasicMesh> source, int index ) : base( source, index ) { }

		protected override void Draw ( in T particle, IRenderer renderer, object? ctx = null ) {
			var baseMatrix = particle.Matrix * Matrix;
			Material.Shader.SetUniform( "mMatrix", Matrix4.CreateFromQuaternion( ( renderer.ProjectionMatrix.ExtractCameraPosition() - baseMatrix.ExtractTranslation() ).LookRotation() ) * baseMatrix );
			Mesh!.Draw();
		}
	}
}