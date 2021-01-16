using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Buffers;
using osu.Framework.Graphics.Shaders;
using osu.XR.Components;
using osu.XR.Maths;
using osuTK;
using osuTK.Graphics.OpenGL4;
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
                CameraClipMatrix = Matrix4x4.CreatePerspectiveProjection( XSlope, YSlope, 0.01f, 100000 );
            }
        }
        private Vector2 fov;
        public float AspectRatio { get; private set; }
        public float XSlope { get; private set; }
        public float YSlope { get; private set; }
        public Matrix4x4 WorldCameraMatrix => Transform.InverseMatrix;
        public Matrix4x4 CameraClipMatrix { get; private set; }
        public Matrix4x4 WorldClipMatrix => CameraClipMatrix * WorldCameraMatrix;

        /// <summary>
        /// Projects a given point to <-1;1><-1;1>. Returns false if the point is behind the camera.
        /// </summary>
        public bool Project ( Vector3 pos, out Vector2 proj ) {
            var p = WorldClipMatrix * new Vector4( pos, 1 );
            p /= p.W;
            proj = p.Xy;

            if ( p.Z is <= 0 or >= 1 ) return false;
            return true;
        }

        public void Render ( XrObject xrObject, float width, float height ) {
            Vector2 scale;
            if ( width / height > AspectRatio ) {
                scale = new( AspectRatio * AspectRatio, 1 ); // TODO why square?
            }
            else {
                scale = new( 1, 1 / AspectRatio / AspectRatio );
            }

            var settings = new XrObjectDrawNode.DrawSettings(
                Matrix4x4.CreateScale( scale.X, scale.Y ) * WorldCameraMatrix,
                CameraClipMatrix,
                this
            );

            //GLWrapper.PushDepthInfo( new DepthInfo( true, true, osuTK.Graphics.ES30.DepthFunction.Less ) );
            //GL.Enable( EnableCap.DepthTest );
            //GL.DepthMask( true );
            //GL.DepthFunc( DepthFunction.Less );
            //GL.DepthRange( 0.0, 1.0 );
            //GL.ClearColor( 0, 0, 0, 0 );
            //GL.ClearDepth( 1 );
            //GL.Clear( ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit );
            // TODO render to any frame buffer ( this will require to correct global scale to fit the aspect ratio )
            void Draw ( XrObject xrObject ) {
                xrObject.BeforeDraw( settings );
                xrObject.DrawNode?.Draw( settings );
                foreach ( var i in xrObject.Children ) {
                    Draw( i );
				}
			}

            Draw( xrObject );
            //GLWrapper.PopDepthInfo();
        }
    }
}
