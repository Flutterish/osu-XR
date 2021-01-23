using OpenVR.NET;
using osu.XR.Graphics;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components {
	public class XrController : MeshedXrObject {
		public readonly Controller Controller;
		Mesh ControllerMesh;
		Mesh SphereMesh;

		public XrController ( Controller controller ) {
			Controller = controller;
			ControllerMesh = new Mesh();
			_ = controller.LoadModelAsync(
				begin: () => ControllerMesh.IsReady = false,
				finish: () => ControllerMesh.IsReady = true,
				addVertice: v => ControllerMesh.Vertices.Add( new osuTK.Vector3( v.X, v.Y, v.Z ) ),
				addTextureCoordinate: uv => ControllerMesh.TextureCoordinates.Add( new osuTK.Vector2( uv.X, uv.Y ) ),
				addTriangle: (a,b,c) => ControllerMesh.Tris.Add( new IndexedFace( (uint)a, (uint)b, (uint)c ) )
			);
			Mesh = ControllerMesh;

			SphereMesh = Mesh.FromOBJFile( "./Resources/shpere.obj" );
		}

		public void UseControllerMesh () {
			Mesh = ControllerMesh;
			Scale = Vector3.One;
		}
		public void UseSphereMesh () {
			Mesh = SphereMesh;
			Scale = new Vector3( 0.03f );
		}

		protected override void Update () {
			base.Update();
			Position = new osuTK.Vector3( Controller.Position.X, Controller.Position.Y, Controller.Position.Z );
			Rotation = new osuTK.Quaternion( Controller.Rotation.X, Controller.Rotation.Y, Controller.Rotation.Z, Controller.Rotation.W );
		}
	}
}
