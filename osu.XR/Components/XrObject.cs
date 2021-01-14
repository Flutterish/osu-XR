using Microsoft.CodeAnalysis.Operations;
using osu.Game.Overlays.BeatmapSet;
using osu.XR.Projection;
using osu.XR.Rendering;
using osuTK;
using System;
using System.Collections.Generic;

namespace osu.XR.Components {
	public class XrObject : IDisposable {
		private List<XrObject> children = new();
		private XrObject parent;

		public IReadOnlyList<XrObject> Children => children.AsReadOnly();
		public XrObject Parent {
			get => parent;
			set {
				if ( parent == value ) return;

				parent?.children.Remove( this );
				parent = value;
				parent?.children.Add( this );
				Transform.SetParent( parent?.Transform, transformKey );
			}
		}
		public void Add ( XrObject child ) {
			child.Parent = this;
		}

		public XrObject () {
			Transform = new( transformKey );
		}

		public virtual void BeforeDraw () { }

		private readonly object transformKey = new { };
		public readonly Transform Transform;
		public Vector3 Position { get => Transform.Position; set => Transform.Position = value; }
		public float X { get => Transform.X; set => Transform.X = value; }
		public float Y { get => Transform.Y; set => Transform.Y = value; }
		public float Z { get => Transform.Z; set => Transform.Z = value; }

		public Vector3 Scale { get => Transform.Scale; set => Transform.Scale = value; }
		public float ScaleX { get => Transform.ScaleX; set => Transform.ScaleX = value; }
		public float ScaleY { get => Transform.ScaleY; set => Transform.ScaleY = value; }
		public float ScaleZ { get => Transform.ScaleZ; set => Transform.ScaleZ = value; }

		public Vector3 Offset { get => Transform.Offset; set => Transform.Offset = value; }
		public float OffsetX { get => Transform.OffsetX; set => Transform.OffsetX = value; }
		public float OffsetY { get => Transform.OffsetY; set => Transform.OffsetY = value; }
		public float OffsetZ { get => Transform.OffsetZ; set => Transform.OffsetZ = value; }

		public Quaternion Rotation { get => Transform.Rotation; set => Transform.Rotation = value; }
		public Vector3 EulerRotation { get => Transform.EulerRotation; set => Transform.EulerRotation = value; }
		public float EulerRotX { get => Transform.EulerRotX; set => Transform.EulerRotX = value; }
		public float EulerRotY { get => Transform.EulerRotY; set => Transform.EulerRotY = value; }
		public float EulerRotZ { get => Transform.EulerRotZ; set => Transform.EulerRotZ = value; }

		private XrObjectDrawNode drawNode;
		public XrObjectDrawNode DrawNode => drawNode ??= CreateDrawNode();
		protected virtual XrObjectDrawNode CreateDrawNode () => null;

		public virtual void Dispose () {
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

			public class DrawSettings {
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
