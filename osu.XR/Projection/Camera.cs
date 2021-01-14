using osu.XR.Components;
using osuTK;
using System;

namespace osu.XR.Projection {
    public class Camera : XrObject {
        public Camera () {
            Fov = new Vector2( MathF.PI * 2 - 4 * MathF.Atan( 16f / 9 ), MathF.PI );
        }

        /// <summary>
        /// Field of view in radians.
        /// </summary>
        public Vector2 Fov {
            get => fov;
            set {
                fov = value;
                XSlope = 1 / MathF.Tan( ( MathF.PI - Fov.X / 2 ) / 2 );
                YSlope = 1 / MathF.Tan( ( MathF.PI - Fov.Y / 2 ) / 2 );
                AspectRatio = XSlope / YSlope;
            }
        }
        private Vector2 fov;
        public float AspectRatio { get; private set; }
        public float XSlope { get; private set; }
        public float YSlope { get; private set; }

        /// <summary>
        /// Projects a given point to <-1;1><-1;1>. Returns false if the point is behind the camera.
        /// </summary>
        public bool Project ( Vector3 pos, out Vector2 proj ) {
            var p = Rotation.Inverted() * new Vector4( ( pos - Position ), 1 );
            proj = new Vector2(
                p.X / p.Z / XSlope,
                p.Y / p.Z / YSlope
            );

            if ( p.Z <= 0 ) return false;
            return true;
        }

        public void Render ( XrObject xrObject ) {

        }
    }
}
