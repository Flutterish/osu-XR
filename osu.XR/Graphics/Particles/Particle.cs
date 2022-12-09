using osu.Framework.Graphics.Rendering;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;

namespace osu.XR.Graphics.Particles;

abstract partial class ParticleEmitter {
	public partial class Particle : BasicModel {
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

		ParticleEmitter? emiter;
		public bool IsApplied { get; private set; }
		public Particle () {
			Mesh = quadMesh;
			Scale = new Vector3( 0.03f );
		}

		public void Apply ( ParticleEmitter emiter ) {
			if ( IsApplied ) throw new InvalidOperationException( "Cannot apply an already applied praticle" );

			IsApplied = true;
			OnApply( this.emiter = emiter );
		}
		protected virtual void OnApply ( ParticleEmitter emmiter ) { }

		public void Release () {
			if ( !IsApplied ) throw new InvalidOperationException( "Cannot release a non-applied praticle" );

			IsApplied = false;
			OnReleased();

			emiter!.release( this );
		}
		protected virtual void OnReleased () { }

		protected override ModelDrawNode? CreateDrawNode3D ( int index )
			=> new DrawNode( this, index );

		class DrawNode : ModelDrawNode {
			public DrawNode ( Model<BasicMesh> source, int index ) : base( source, index ) { }

			Matrix4 baseMatrix;
			protected override void UpdateState () {
				base.UpdateState();
				baseMatrix = Matrix;
			}

			public override void Draw ( IRenderer renderer, object? ctx = null ) {
				Matrix = Matrix4.CreateFromQuaternion( (renderer.ProjectionMatrix.ExtractCameraPosition() - baseMatrix.ExtractTranslation()).LookRotation() ) * baseMatrix;
				base.Draw( renderer, ctx );
			}
		}
	}
}