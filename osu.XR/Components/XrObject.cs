using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Maths;
using osu.XR.Projection;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
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
		new public XrObject Child {
			get => children.Single();
			set {
				foreach ( var i in children.ToArray() ) i.Parent = null;
				value.Parent = this;
			}
		}
		new public IReadOnlyList<XrObject> Children {
			get => children.AsReadOnly();
			set {
				foreach ( var i in children.ToArray() ) i.Parent = null;
				foreach ( var i in value ) i.Parent = this;
			}
		}
		new public XrObject Parent {
			get => parent;
			set {
				if ( parent == value ) return;
				
				if ( parent is Container con ) {
					parent.children.Remove( this );
					con.Remove( this );

					parent.onChildRemoved( this );
					foreach ( var i in GetAllChildrenInHiererchy() ) parent.onChildRemovedFromHierarchy( i.parent, i );
				}
				parent = value;
				if ( parent is Container con2 ) {
					parent.children.Add( this );
					con2.Add( this );

					parent.onChildAdded( this );
					foreach ( var i in GetAllChildrenInHiererchy() ) parent.onChildAddedToHierarchy( i.parent, i );
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
		public void Remove ( XrObject child ) {
			child.Parent = null;
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

		public Vector3 Offset { 
			get => Transform.Offset; 
			set {
				AutoOffsetAxes = Axes3D.None;
				Transform.Offset = value;
			} 
		}
		public float OffsetX { 
			get => Transform.OffsetX; 
			set {
				AutoOffsetAxes &= ~Axes3D.X;
				Transform.OffsetX = value;
			}
		}
		public float OffsetY { 
			get => Transform.OffsetY; 
			set {
				AutoOffsetAxes &= ~Axes3D.Y;
				Transform.OffsetY = value;
			}
		}
		public float OffsetZ { 
			get => Transform.OffsetZ; 
			set {
				AutoOffsetAxes &= ~Axes3D.Z;
				Transform.OffsetZ = value;
			}
		}

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

		protected virtual Vector3 RequiredParentSizeToFit => ChildSize;
		/// <summary>
		/// The size nescessary to fit all children
		/// </summary>
		new public Vector3 ChildSize { get; private set; } // ISSUE the "no separation between composite and regular xrobjects" makes this iffy bc theres "children size" and "self size"
		new protected Axes3D AutoSizeAxes = Axes3D.All; // TODO invalidation mechanism for this
		public Axes3D AutoOffsetAxes = Axes3D.None;
		new public Axes3D BypassAutoSizeAxes = Axes3D.None;
		private Vector3 autoOffsetAnchor;
		public Vector3 AutoOffsetAnchor {
			get => autoOffsetAnchor;
			set {
				AutoOffsetAxes = Axes3D.All;
				autoOffsetAnchor = value;
			}
		}
		public float AutoOffsetAnchorX {
			get => autoOffsetAnchor.X;
			set {
				AutoOffsetAxes |= Axes3D.X;
				autoOffsetAnchor.X = value;
			}
		}
		public float AutoOffsetAnchorY {
			get => autoOffsetAnchor.Y;
			set {
				AutoOffsetAxes |= Axes3D.Y;
				autoOffsetAnchor.Y = value;
			}
		}
		public float AutoOffsetAnchorZ {
			get => autoOffsetAnchor.Z;
			set {
				AutoOffsetAxes |= Axes3D.Z;
				autoOffsetAnchor.Z = value;
			}
		}
		private Vector3 autoOffsetOrigin;
		public Vector3 AutoOffsetOrigin {
			get => autoOffsetOrigin;
			set {
				AutoOffsetAxes = Axes3D.All;
				autoOffsetOrigin = value;
			}
		}
		public float AutoOffsetOriginX {
			get => autoOffsetOrigin.X;
			set {
				AutoOffsetAxes |= Axes3D.X;
				autoOffsetOrigin.X = value;
			}
		}
		public float AutoOffsetOriginY {
			get => autoOffsetOrigin.Y;
			set {
				AutoOffsetAxes |= Axes3D.Y;
				autoOffsetOrigin.Y = value;
			}
		}
		public float AutoOffsetOriginZ {
			get => autoOffsetOrigin.Z;
			set {
				AutoOffsetAxes |= Axes3D.Z;
				autoOffsetOrigin.Z = value;
			}
		}

		/// <summary>
		/// The centre of the object in local coordinates.
		/// </summary>
		public virtual Vector3 Centre => children.Any() ? children.Average( x => x.Position + x.Centre ) : Vector3.Zero;
		protected override void Update () {
			base.Update();
			if ( children.Any() ) {
				ChildSize = new Vector3(
					AutoSizeAxes.HasFlag( Axes3D.X ) ? children.Max( c => c.BypassAutoSizeAxes.HasFlag( Axes3D.X ) ? 0 : c.RequiredParentSizeToFit.X ) : 0,
					AutoSizeAxes.HasFlag( Axes3D.Y ) ? children.Max( c => c.BypassAutoSizeAxes.HasFlag( Axes3D.Y ) ? 0 : c.RequiredParentSizeToFit.Y ) : 0,
					AutoSizeAxes.HasFlag( Axes3D.Z ) ? children.Max( c => c.BypassAutoSizeAxes.HasFlag( Axes3D.Z ) ? 0 : c.RequiredParentSizeToFit.Z ) : 0
				);
			}
			else {
				ChildSize = RequiredParentSizeToFit;
			}

			if ( AutoOffsetAxes.HasFlag( Axes3D.X ) ) {
				var parentSize = parent is null ? 0 : parent.ChildSize.X;
				var ownSize = ChildSize.X;
				Transform.OffsetX = autoOffsetAnchor.X * parentSize - autoOffsetOrigin.X * ownSize - Centre.X;
			}
			if ( AutoOffsetAxes.HasFlag( Axes3D.Y ) ) {
				var parentSize = parent is null ? 0 : parent.ChildSize.Y;
				var ownSize = ChildSize.Y;
				Transform.OffsetY = autoOffsetAnchor.Y * parentSize - autoOffsetOrigin.Y * ownSize - Centre.Y;
			}
			if ( AutoOffsetAxes.HasFlag( Axes3D.Z ) ) {
				var parentSize = parent is null ? 0 : parent.ChildSize.Z;
				var ownSize = ChildSize.Z;
				Transform.OffsetZ = autoOffsetAnchor.Z * parentSize - autoOffsetOrigin.Z * ownSize - Centre.Z;
			}
		}

		public void Destroy () {
			Parent = null;
			foreach ( var i in children ) i.Destroy();
			Dispose();
		}

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
	[Flags]
	public enum Axes3D {
		None = 0,

		X = 1,
		Y = 2,
		Z = 4,

		XY = X | Y,
		XZ = X | Z,
		YZ = Y | Z,
		All = X | Y | Z
	}
}

namespace System.Runtime.CompilerServices {
	public class IsExternalInit { }
}