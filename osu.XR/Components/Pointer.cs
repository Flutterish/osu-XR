using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.XR.Graphics;
using osu.XR.Physics;
using osuTK;
using static osu.XR.Physics.Raycast;

namespace osu.XR.Components {
	/// <summary>
	/// A 3D cursor.
	/// </summary>
	public class Pointer : MeshedXrObject {
		[Resolved]
		private PhysicsSystem PhysicsSystem { get; set; }

		public double HitDistance { get => HitDistanceBindable.Value; set => HitDistanceBindable.Value = value; }
		public readonly BindableDouble HitDistanceBindable = new( 5 );

		public Pointer () { // TODO make colors reflect pressed buttons, possibly warp towards the held location ( easier to do with a circular texture )
			Mesh = new();
			Mesh.AddCircle( new Vector3( 0, 0, -0.01f ), Vector3.UnitZ, Vector3.UnitX * 0.04f, 30 );
			Mesh.AddCircle( new Vector3( 0, 0, -0.02f ), Vector3.UnitZ, Vector3.UnitX * 0.014f, 30 );

			MainTexture = Textures.Pixel( new osuTK.Graphics.Color4( 255, 255, 255, 100 ) ).TextureGL;
		}

		public override void BeforeDraw ( XrObjectDrawNode.DrawSettings settings ) {
			base.BeforeDraw( settings );
			if ( PhysicsSystem.TryHit( settings.Camera.Position, settings.Camera.Forward, out var hit ) && hit.Distance < HitDistance ) {
				Position = hit.Point;
				Rotation = Matrix4.LookAt( Vector3.Zero, hit.Normal, Vector3.UnitY ).ExtractRotation().Inverted();

				NewHit?.Invoke( hit );
			}
			else {
				Position = settings.Camera.Position + settings.Camera.Forward * (float)HitDistance;
				Rotation = settings.Camera.Rotation;
			}
		}

		public delegate void PointerUpdate ( RaycastHit hit );
		public event PointerUpdate NewHit;
	}
}
