using osu.Framework.Bindables;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;
using osuTK;
using System;

namespace osu.XR.Input.Pointers {
	/// <summary>
	/// A 3D cursor.
	/// </summary>
	public class RaycastPointer : Pointer {
		public Transform Source;

		public double HitDistance { get => HitDistanceBindable.Value; set => HitDistanceBindable.Value = value; }
		public readonly BindableDouble HitDistanceBindable = new( 5 );

		public RaycastPointer () { // TODO warp towards the held location
			Mesh = new();
			Mesh.AddCircle( new Vector3( 0, 0, -0.01f ), Vector3.UnitZ, Vector3.UnitX * 0.04f, 30 );
			Mesh.AddCircle( new Vector3( 0, 0, -0.02f ), Vector3.UnitZ, Vector3.UnitX * 0.014f, 30 );

			MainTexture = Textures.Pixel( new osuTK.Graphics.Color4( 255, 255, 255, 100 ) ).TextureGL;
			ShouldBeDepthSorted = true;
		}
		protected override void UpdatePointer () {
			if ( PhysicsSystem.TryHit( Source.Position, Source.Forward, out var hit ) && hit.Distance < HitDistance ) {
				Position = hit.Point;

				Rotation = hit.Normal.Y == 1
					? Quaternion.FromEulerAngles( MathF.PI / 2, 0, 0 )
					: hit.Normal.Y == -1
					? Quaternion.FromEulerAngles( -MathF.PI / 2, 0, 0 )
					: Matrix4.LookAt( Vector3.Zero, hit.Normal, Vector3.UnitY ).ExtractRotation().Inverted();

				RaycastHit = hit;
				CurrentHit = hit.Collider;
			}
			else {
				Position = Source.Position + Source.Forward * (float)HitDistance;
				Rotation = Source.Rotation;

				RaycastHit = hit;
				CurrentHit = null;
			}

			Scale = new Vector3( ( Position - Source.Position ).Length );
		}

		public override bool IsActive => Source is not null && IsVisible;
	}
}
