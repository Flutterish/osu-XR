﻿using osu.Framework.Allocation;
using osu.XR.Graphics;
using osu.XR.Physics;
using osu.XR.Projection;
using osuTK;
using System;
using System.Collections.Generic;
using System.Text;
using static osu.XR.Physics.Raycast;

namespace osu.XR.Components {
	/// <summary>
	/// A 3D cursor.
	/// </summary>
	public class Pointer : MeshedXrObject {
		[Resolved]
		private PhysicsSystem PhysicsSystem { get; set; }

		public Pointer () {
			Mesh = new();
			Mesh.Vertices.Add( Vector3.Zero );
			float radius = 0.05f;
			int res = 30;
			Mesh.Vertices.Add( new Vector3( radius, 0, 0 ) );
			for ( int i = 1; i < res; i++ ) {
				var angle = ( (float)i / res ) * MathF.PI * 2;
				Mesh.Vertices.Add( new Vector3( MathF.Cos( angle ), MathF.Sin( angle ), 0 ) * radius );
				Mesh.Tris.Add( new IndexedFace( 0, (uint)i, (uint)i+1 ) );
			}
			Mesh.Tris.Add( new IndexedFace( 0, 1, (uint)res ) );
			Texture = Textures.Pixel( new osuTK.Graphics.Color4( 255, 255, 255, 100 ) ).TextureGL;
		}

		public override void BeforeDraw ( XrObjectDrawNode.DrawSettings settings ) {
			base.BeforeDraw( settings );
			if ( PhysicsSystem.TryHit( settings.Camera.Position, settings.Camera.Forward, out var hit ) && hit.Distance < 10 ) {
				Position = hit.Point;
				Rotation = Matrix4.LookAt( Vector3.Zero, hit.Normal, Vector3.UnitY ).ExtractRotation().Inverted();

				OnUpdate?.Invoke( hit );
			}
			else {
				Position = settings.Camera.Position + settings.Camera.Forward * 10;
				Rotation = settings.Camera.Rotation;
			}
		}

		public delegate void PointerUpdate ( RaycastHit hit );
		new public event PointerUpdate OnUpdate;
	}
}
