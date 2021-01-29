using osuTK;
using System;
using System.Collections.Generic;

namespace osu.XR.Maths {
	/// <summary>
	/// Position, rotation and scale of an object with child-parent hiererchy.
	/// </summary>
	public class Transform {
		private readonly object key;
		/// <param name="key">An optional key to lock modifying relationships by non-authorized sources.</param>
		public Transform ( object key = null ) {
			this.key = key;
		}
		private Transform parent;
		private List<Transform> children = new();
		public Transform Parent {
			get => parent;
			set {
				if ( key is not null ) throw new InvalidOperationException( "This transform's relationships are locked." );
				if ( parent == value ) return;
				parent?.children.Remove( this );

				parent = value;
				parent?.children.Add( this );

				invalidateFinal();
			}
		}
		public IReadOnlyList<Transform> Children => children.AsReadOnly();
		public void SetParent ( Transform value, object key = null ) {
			if ( key == this.key ) {
				if ( parent == value ) return;
				parent?.children.Remove( this );

				parent = value;
				parent?.children.Add( this );

				invalidateFinal();
			}
			else throw new InvalidOperationException( "Invalid key." );
		}

		private bool isLocalMatrixInvalidated = true;
		private bool isFinalMatrixInvalidated = true;

		private void invalidateLocal () {
			isLocalMatrixInvalidated = true;
			for ( int i = 0; i < children.Count; i++ )
				children[ i ].invalidateFinal();
		}
		private void invalidateFinal () {
			isFinalMatrixInvalidated = true;
			for ( int i = 0; i < children.Count; i++ )
				children[ i ].invalidateFinal();
		}

		private Vector3 position;
		public Vector3 Position { get => position; set { position = value; invalidateLocal(); } }
		public float X { get => position.X; set { position.X = value; invalidateLocal(); } }
		public float Y { get => position.Y; set { position.Y = value; invalidateLocal(); } }
		public float Z { get => position.Z; set { position.Z = value; invalidateLocal(); } }

		private Vector3 scale = Vector3.One;
		public Vector3 Scale { get => scale; set { scale = value; invalidateLocal(); } }
		public float ScaleX { get => scale.X; set { scale.X = value; invalidateLocal(); } }
		public float ScaleY { get => scale.Y; set { scale.Y = value; invalidateLocal(); } }
		public float ScaleZ { get => scale.Z; set { scale.Z = value; invalidateLocal(); } }

		private Vector3 offset;
		public Vector3 Offset { get => offset; set { offset = value; invalidateLocal(); } }
		public float OffsetX { get => offset.X; set { offset.X = value; invalidateLocal(); } }
		public float OffsetY { get => offset.Y; set { offset.Y = value; invalidateLocal(); } }
		public float OffsetZ { get => offset.Z; set { offset.Z = value; invalidateLocal(); } }

		private Quaternion rotation = Quaternion.Identity;
		public Quaternion Rotation { get => rotation; set { rotation = value; invalidateLocal(); } }
		public Vector3 EulerRotation { get => Rotation.ToEuler(); set { rotation = Quaternion.FromEulerAngles( value ); invalidateLocal(); } }
		public float EulerRotX { get => EulerRotation.X; set { EulerRotation = EulerRotation.With( x: value ); invalidateLocal(); } }
		public float EulerRotY { get => EulerRotation.Y; set { EulerRotation = EulerRotation.With( y: value ); invalidateLocal(); } }
		public float EulerRotZ { get => EulerRotation.Z; set { EulerRotation = EulerRotation.With( z: value ); invalidateLocal(); } }

		public Vector3 Forward => ( Rotation * new Vector4( 0, 0, 1, 1 ) ).Xyz;
		public Vector3 Backward => ( Rotation * new Vector4( 0, 0, -1, 1 ) ).Xyz;
		public Vector3 Left => ( Rotation * new Vector4( -1, 0, 0, 1 ) ).Xyz;
		public Vector3 Right => ( Rotation * new Vector4( 1, 0, 0, 1 ) ).Xyz;
		public Vector3 Up => ( Rotation * new Vector4( 0, 1, 0, 1 ) ).Xyz;
		public Vector3 Down => ( Rotation * new Vector4( 0, -1, 0, 1 ) ).Xyz;

		public Vector3 GlobalPosition => ( Matrix * new Vector4( position, 1 ) ).Xyz;

		private Matrix4x4 localMatrix;
		private Matrix4x4 finalMatrix;

		private void validateLocal () {
			if ( isLocalMatrixInvalidated ) {
				localMatrix = Matrix4x4.CreateTranslation( position ) * Matrix4x4.CreateRotation( rotation ) * Matrix4x4.CreateScale( scale ) * Matrix4x4.CreateTranslation( offset );
				isLocalMatrixInvalidated = false;
				isFinalMatrixInvalidated = true;
			}
		}
		private void validateFinal () {
			if ( isFinalMatrixInvalidated ) {
				if ( parent is null )
					finalMatrix = localMatrix;
				else
					finalMatrix = parent.Matrix * localMatrix;
				isFinalMatrixInvalidated = false;
			}
		}

		/// <summary>
		/// The matrix this <see cref="Transform"/> produces. It is cached.
		/// </summary>
		public Matrix4x4 Matrix {
			get {
				validateLocal();
				validateFinal();
				return finalMatrix;
			}
		}

		/// <summary>
		/// An inverse matrix. This one is not cached.
		/// </summary>
		public Matrix4x4 InverseMatrix
			=> Matrix4x4.CreateTranslation( -offset ) * Matrix4x4.CreateScale( new Vector3( 1 / scale.X, 1 / scale.Y, 1 / scale.Z ) ) * Matrix4x4.CreateRotation( rotation.Inverted() ) * Matrix4x4.CreateTranslation( -position );
	}
}
