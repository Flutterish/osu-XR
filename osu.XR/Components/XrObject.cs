using osuTK;

namespace osu.XR.Components {
	public abstract class XrObject {
		public Transform Transform { get; } = new Transform();
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
	}
}
