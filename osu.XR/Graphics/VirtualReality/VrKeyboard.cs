using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Graphics.Panels;
using osu.Framework.XR.Parsing.Wavefront;
using osuTK.Graphics;

namespace osu.XR.Graphics.VirtualReality;

public partial class VrKeyboard : CompositeDrawable3D {

	[BackgroundDependencyLoader]
	private void load ( MeshStore meshStore ) {
		base.LoadComplete();
		var kb = meshStore.GetCollection( "keyboard" );
		foreach ( var i in kb.AllObjects ) {
			var mesh = i.MeshParts[0].Mesh.Mesh;
			AddInternal( new Model {
				Mesh = mesh
			} );

			if ( mesh is not ITriangleMesh tringular )
				continue;

			Panel panel;
			if ( tringular.FindFlatMeshPlane() is Plane plane ) {
				var rotation = plane.Normal.LookRotation();
				var rotationInverse = rotation.Inverted();
				var bb = new AABox( tringular.EnumerateVertices().Select( x => rotationInverse.Apply( x ) ) );
				var forward = plane.Normal.Z > 0 ? plane.Normal : -plane.Normal;

				panel = new FlatPanel() {
					Origin = new( -1f ),
					Position = rotation.Apply( bb.Min ) + forward * 0.05f,
					Scale = bb.Size / 2,
					Rotation = rotation
				};
				panel.ContentSize = new( bb.Size.X * 64, bb.Size.Y * 64 );
			}
			else {
				var objMesh = (ObjFile.ObjMesh)mesh;

				panel = new ModelledPanel( objMesh );
				panel.ContentSize = new( tringular.BoundingBox.Size.X * 64, tringular.BoundingBox.Size.Y * 64 );
				panel.Position += Vector3.UnitZ * 0.05f;
			}

			panel.Content.Add( new Key() );
			AddInternal( panel );
		}
	}

	partial class ModelledPanel : Panel {
		public ModelledPanel ( ObjFile.ObjMesh mesh ) {
			var indices = mesh.ElementBuffer.Indices;
			for ( int i = 0; i < indices.Count / 3; i++ ) {
				var (a, b, c) = (indices[i * 3], indices[i * 3 + 1], indices[i * 3 + 2]);
				Mesh.AddTriangle( new() {
					Position = mesh.Positions.Data[(int)a],
					UV = new() {
						X = mesh.UVs.Data[(int)a].U,
						Y = mesh.UVs.Data[(int)a].V
					}
				}, new() {
					Position = mesh.Positions.Data[(int)b],
					UV = new() {
						X = mesh.UVs.Data[(int)b].U,
						Y = mesh.UVs.Data[(int)b].V
					}
				}, new() {
					Position = mesh.Positions.Data[(int)c],
					UV = new() {
						X = mesh.UVs.Data[(int)c].U,
						Y = mesh.UVs.Data[(int)c].V
					}
				}, computeNormal: true );
			}
		}

		protected override bool ClearMeshOnInvalidate => false;
		protected override void RegenrateMesh () { }

		protected override void Update () {
			base.Update();
		}
	}

	partial class Key : CompositeDrawable {
		public Key () {
			RelativeSizeAxes = Axes.Both;
			AddInternal( new Box {
				Colour = Color4.Red,
				RelativeSizeAxes = Axes.Both
			} );
		}
	}
}
