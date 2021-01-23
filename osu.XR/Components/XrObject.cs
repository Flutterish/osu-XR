using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.XR.Maths;
using osu.XR.Projection;
using osuTK;
using System;
using System.Collections.Generic;
using static osu.XR.Components.XrObject.XrObjectDrawNode;

namespace osu.XR.Components {
	/// <summary>
	/// An <see cref="XrObject"/> is the 3D counterpart of a <see cref="Drawable"/>.
	/// </summary>
	public class XrObject : Container { // has to be a "Drawable" because it gives us cool stuff
		public override bool RemoveCompletedTransforms => true;
		public XrObject () {
			Transform = new Transform( transformKey );
			RelativeSizeAxes = Axes.Both;
		}
		
		private List<XrObject> children = new();
		private XrObject parent;

		new public IReadOnlyList<XrObject> Children => children.AsReadOnly();
		new public XrObject Parent {
			get => parent;
			set {
				if ( parent == value ) return;
				
				if ( parent is Container con ) {
					parent.children.Remove( this );
					con.Remove( this );

					parent.onChildRemoved( this );
				}
				parent = value;
				if ( parent is Container con2 ) {
					parent.children.Add( this );
					con2.Add( this );

					parent.onChildAdded( this );
				}
				Transform.SetParent( parent?.Transform, transformKey );
			}
		}
		// These events are used for efficient hiererchy change scans used in for example the physics system.
		public delegate void ChildChangedHandler ( XrObject parent, XrObject child );
		/// <summary>
		/// Occurs whenever a child is added to this <see cref="XrObject"/>
		/// </summary>
		public event ChildChangedHandler ChildAdded;
		/// <summary>
		/// Occurs whenever a child is removed from this <see cref="XrObject"/>
		/// </summary>
		public event ChildChangedHandler ChildRemoved;
		/// <summary>
		/// Occurs whenever an <see cref="XrObject"/> is added under this <see cref="XrObject"/>
		/// </summary>
		public event ChildChangedHandler ChildAddedToHierarchy;
		/// <summary>
		/// Occurs whenever an <see cref="XrObject"/> is removed from under this <see cref="XrObject"/>
		/// </summary>
		public event ChildChangedHandler ChildRemovedFromHierarchy;

		private void onChildAdded ( XrObject child ) {
			ChildAdded?.Invoke( this, child );
			onChildAddedToHierarchy( this, child );
		}
		private void onChildAddedToHierarchy ( XrObject parent, XrObject child ) {
			ChildAddedToHierarchy?.Invoke( parent, child );
			this.parent?.onChildAddedToHierarchy( parent, child );
		}
		private void onChildRemoved ( XrObject child ) {
			ChildRemoved?.Invoke( this, child );
			onChildRemovedFromHierarchy( this, child );
		}
		private void onChildRemovedFromHierarchy ( XrObject parent, XrObject child ) {
			ChildRemovedFromHierarchy?.Invoke( parent, child );
			this.parent?.onChildRemovedFromHierarchy( parent, child );
		}
		public void BindHierarchyChange ( ChildChangedHandler added, ChildChangedHandler removed, bool runOnAllChildrenImmediately = false ) {
			if ( removed is not null ) ChildRemovedFromHierarchy += removed;
			if ( added is not null ) {
				ChildAddedToHierarchy += added;
				if ( runOnAllChildrenImmediately ) {
					foreach ( var i in GetAllChildrenInHiererchy() ) {
						added( i.parent, i );
					}
				}
			}
		}

		public IEnumerable<XrObject> GetAllChildrenInHiererchy () {
			List<XrObject> all = new() { this };
			for ( int i = 0; i < all.Count; i++ ) {
				var current = all[ i ];
				for ( int k = 0; k < current.children.Count; k++ ) {
					yield return current.children[ k ];
					all.Add( current.children[ k ] );
				}
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

		public virtual void BeforeDraw ( DrawSettings settings ) { }

		private readonly object transformKey = new { };
		public readonly Transform Transform;
		new public Vector3 Position { get => Transform.Position; set => Transform.Position = value; }
		new public float X { get => Transform.X; set => Transform.X = value; }
		new public float Y { get => Transform.Y; set => Transform.Y = value; }
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

		public Vector3 Forward => Transform.Forward;
		public Vector3 Backward => Transform.Backward;
		public Vector3 Left => Transform.Left;
		public Vector3 Right => Transform.Right;
		public Vector3 Up => Transform.Up;
		public Vector3 Down => Transform.Down;

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

			public record DrawSettings { // TODO most of these should be in a global uniform block
				public Matrix4 WorldToCamera { get; init; }
				public Matrix4 CameraToClip { get; init; }
				public Camera Camera { get; init; }
			}
		}

		public abstract class XrObjectDrawNode<T> : XrObjectDrawNode where T : XrObject {
			new protected T Source => base.Source as T;
			public XrObjectDrawNode ( T source ) : base( source ) { }
		}

		public static implicit operator Transform ( XrObject xro )
			=> xro?.Transform;
	}
}

namespace System.Runtime.CompilerServices {
	public class IsExternalInit { }
}