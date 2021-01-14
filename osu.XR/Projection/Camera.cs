using osuTK;
using System;

namespace osu.XR.Projection {
	public class Camera
    {
        public Camera ()
        {
            Fov = new Vector2(MathF.PI * 2 - 4 * MathF.Atan(16f / 9), MathF.PI);
        }
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Position;
        public float X { get => Position.X; set => Position.X = value; }
        public float Y { get => Position.Y; set => Position.Y = value; }
        public float Z { get => Position.Z; set => Position.Z = value; }
        /// <summary>
        /// Field of view in radians.
        /// </summary>
        public Vector2 Fov {
            get => fov;
            set
            {
                fov = value;
                XSlope = 1 / MathF.Tan(( MathF.PI - Fov.X / 2 ) / 2);
                YSlope = 1 / MathF.Tan(( MathF.PI - Fov.Y / 2 ) / 2);
                AspectRatio = XSlope / YSlope;
            }
        }
        private Vector2 fov;
        public float AspectRatio { get; private set; }
        public float XSlope { get; private set; }
        public float YSlope { get; private set; }

        public bool Project ( Vector3 pos, out Vector2 proj )
        {
            var p = Rotation.Inverted() * new Vector4(( pos - Position ), 1);
            float widthAtZ = p.Z * 2 * XSlope;
            float heightAtZ = p.Z * 2 * YSlope;
            proj = new Vector2(
                p.X / widthAtZ,
                p.Y / heightAtZ
            );

            if ( p.Z <= 0 ) return false;
            return true;
        }
    }
}
