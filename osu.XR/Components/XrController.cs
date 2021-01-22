using OpenVR.NET;
using osu.XR.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components {
	public class XrController : MeshedXrObject {
		public readonly Controller Controller;
		public XrController ( Controller controller ) {
			Controller = controller;
			Mesh = new Mesh();
			_ = controller.LoadModelAsync(
				begin: () => Mesh.IsReady = false,
				finish: () => Mesh.IsReady = true,
				addVertice: v => Mesh.Vertices.Add( new osuTK.Vector3( v.X, v.Y, v.Z ) ),
				addTextureCoordinate: uv => Mesh.TextureCoordinates.Add( new osuTK.Vector2( uv.X, uv.Y ) ),
				addTriangle: (a,b,c) => Mesh.Tris.Add( new IndexedFace( (uint)a, (uint)b, (uint)c ) )
			);
		}

		protected override void Update () {
			base.Update();
			Position = new osuTK.Vector3( Controller.Position.X, Controller.Position.Y, Controller.Position.Z );
			Rotation = new osuTK.Quaternion( Controller.Rotation.X, Controller.Rotation.Y, Controller.Rotation.Z, Controller.Rotation.W );
		}
	}
}
