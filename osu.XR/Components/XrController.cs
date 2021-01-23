using OpenVR.NET;
using osu.Framework.Graphics;
using osu.XR.Graphics;
using osuTK;
using osuTK.Graphics;
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

		public readonly Pointer Pointer = new();

		public XrController ( Controller controller ) {
			MainTexture = Textures.Pixel( controller.IsMainController ? Color4.Orange : Color4.LightBlue ).TextureGL;
			Pointer.MainTexture = Textures.Pixel( (controller.IsMainController ? Colour4.Orange : Colour4.LightBlue ).MultiplyAlpha( 100f / 255f ) ).TextureGL;
			Pointer.Source = this;

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

			controller.BindDisabled( () => {
				IsVisible = false;
				foreach ( var i in reversibleActions ) {
					i.Value.undo();
				}
				Pointer.IsVisible = false;
				reversibleActions.Clear();
			} );

			controller.BindEnabled( () => {
				IsVisible = true;
				Pointer.IsVisible = true;
			} );
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			Root.Add( Pointer );
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

		Dictionary<object,(System.Action @do, System.Action undo)> reversibleActions = new();
		public void PerformReversibleAction ( object name, System.Action @do, System.Action undo ) {
			reversibleActions.Add( name, (@do, undo) );
			@do();
		}
		public void UndoReversibleAction ( object name ) {
			var (_, undo) = reversibleActions[ name ];
			reversibleActions.Remove( name );
			undo();
		}
	}
}
