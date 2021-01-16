﻿using Microsoft.CodeAnalysis.Operations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.BeatmapSet;
using osu.XR.Maths;
using osu.XR.Projection;
using osu.XR.Rendering;
using osuTK;
using System;
using System.Collections.Generic;
using static osu.XR.Components.XrObject.XrObjectDrawNode;

namespace osu.XR.Components {
	/// <summary>
	/// An <see cref="XrObject"/> is the 3D counterpart of a <see cref="Drawable"/>.
	/// </summary>
	public class XrObject : Container { // has to be a "Drawable" because it gives us cool stuff.
		private List<XrObject> children = new();
		private XrObject parent;

		new public IReadOnlyList<XrObject> Children => children.AsReadOnly();
		new public XrObject Parent {
			get => parent;
			set {
				if ( parent == value ) return;

				( base.Parent as Container )?.Remove( this );
				parent?.children.Remove( this );
				parent = value;
				( base.Parent as Container )?.Add( this );
				parent?.children.Add( this );
				Transform.SetParent( parent?.Transform, transformKey );
			}
		}
		/// <summary>
		/// The topmost <see cref="XrObject"/> in the hierarchy. This operation performs upwards tree traveral and might be expensive.
		/// </summary>
		public XrObject Root => ( parent?.Root ?? parent ) ?? this;
		public T FindObject<T> () where T : XrObject {
			T find ( XrObject node ) {
				if ( node is T tnode ) return tnode;
				foreach ( var i in node.children ) {
					var res = find( i );
					if ( res is not null ) return res;
				}
				return null;
			}

			return find( Root );
		}
		public void Add ( XrObject child ) {
			child.Parent = this;
		}

		public XrObject () {
			Transform = new Transform( transformKey );
		}

		public virtual void BeforeDraw ( DrawSettings settings ) { }

		private readonly object transformKey = new { };
		public readonly Transform Transform;
		new public Vector3 Position { get => Transform.Position; set => Transform.Position = value; }
		public Vector3 Forward => ( Rotation * new Vector4( 0, 0, 1, 1 ) ).Xyz;
		public Vector3 Backwards => ( Rotation * new Vector4( 0, 0, -1, 1 ) ).Xyz;
		public float X { get => Transform.X; set => Transform.X = value; }
		public float Y { get => Transform.Y; set => Transform.Y = value; }
		public float Z { get => Transform.Z; set => Transform.Z = value; }

		new public Vector3 Scale { get => Transform.Scale; set => Transform.Scale = value; }
		public float ScaleX { get => Transform.ScaleX; set => Transform.ScaleX = value; }
		public float ScaleY { get => Transform.ScaleY; set => Transform.ScaleY = value; }
		public float ScaleZ { get => Transform.ScaleZ; set => Transform.ScaleZ = value; }

		public Vector3 Offset { get => Transform.Offset; set => Transform.Offset = value; }
		public float OffsetX { get => Transform.OffsetX; set => Transform.OffsetX = value; }
		public float OffsetY { get => Transform.OffsetY; set => Transform.OffsetY = value; }
		public float OffsetZ { get => Transform.OffsetZ; set => Transform.OffsetZ = value; }

		new public Quaternion Rotation { get => Transform.Rotation; set => Transform.Rotation = value; }
		public Vector3 EulerRotation { get => Transform.EulerRotation; set => Transform.EulerRotation = value; }
		public float EulerRotX { get => Transform.EulerRotX; set => Transform.EulerRotX = value; }
		public float EulerRotY { get => Transform.EulerRotY; set => Transform.EulerRotY = value; }
		public float EulerRotZ { get => Transform.EulerRotZ; set => Transform.EulerRotZ = value; }

		private XrObjectDrawNode drawNode;
		public XrObjectDrawNode DrawNode => drawNode ??= CreateDrawNode();
		new protected virtual XrObjectDrawNode CreateDrawNode () => null;

		protected override void Dispose ( bool isDisposing ) {
			base.Dispose( isDisposing );
			drawNode?.Dispose();
		}
		public abstract class XrObjectDrawNode : IDisposable {
			protected XrObject Source;
			protected Transform Transform => Source.Transform;
			public XrObjectDrawNode ( XrObject source ) {
				Source = source;
			}

			public abstract void Draw ( DrawSettings settings );

			public virtual void Dispose () { }

			public class DrawSettings { // TODO most of these should be in a global uniform block
				public readonly Matrix4 WorldToCamera;
				public readonly Matrix4 CameraToClip;
				public readonly Camera Camera;

				public DrawSettings ( Matrix4 worldToCamera, Matrix4 cameraToClip, Camera camera ) {
					WorldToCamera = worldToCamera;
					CameraToClip = cameraToClip;
					Camera = camera;
				}
			}
		}

		public abstract class XrObjectDrawNode<T> : XrObjectDrawNode where T : XrObject {
			new protected T Source => base.Source as T;
			public XrObjectDrawNode ( T source ) : base( source ) { }
		}
	}
}
