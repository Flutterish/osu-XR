using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.XR.Graphics;
using osu.XR.Maths;
using osu.XR.Physics;
using osuTK;
using System;
using static osu.XR.Physics.Raycast;

namespace osu.XR.Components {
	/// <summary>
	/// A 3D cursor.
	/// </summary>
	public class Pointer : MeshedXrObject {
		public Transform Source;
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

		/// <summary>
		/// The <see cref="IHasCollider"/> this pointer points at. Might be null.
		/// </summary>
		public IHasCollider CurrentHit { get; private set; }
		/// <summary>
		/// The last non-null <see cref="CurrentHit"/>.
		/// </summary>
		public IHasCollider CurrentFocus { get; private set; }

		public override void BeforeDraw ( XrObjectDrawNode.DrawSettings settings ) {
			base.BeforeDraw( settings );
			if ( Source is null ) return;

			if ( PhysicsSystem.TryHit( Source.Position, Source.Forward, out var hit ) && hit.Distance < HitDistance ) {
				Position = hit.Point;
				Rotation = Matrix4.LookAt( Vector3.Zero, hit.Normal, Vector3.UnitY ).ExtractRotation().Inverted();

				if ( CurrentFocus != hit.Collider ) {
					var prev = CurrentFocus;
					CurrentHit = CurrentFocus = hit.Collider;
					FocusChanged?.Invoke( new( prev, CurrentFocus ) );
				}
				NewHit?.Invoke( hit );
			}
			else {
				Position = Source.Position + Source.Forward * (float)HitDistance;
				Rotation = Source.Rotation;
				CurrentHit = null;
			}

			Scale = new Vector3( ( Position - Source.Position ).Length );
		}

		public event Action<ValueChangedEvent<IHasCollider>> FocusChanged;

		public delegate void PointerUpdate ( RaycastHit hit );
		public event PointerUpdate NewHit;
	}
}
