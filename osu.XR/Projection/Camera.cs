using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.Primitives;
using osu.XR.Components;
using osu.XR.Graphics;
using osu.XR.Maths;
using osuTK;
using osuTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace osu.XR.Projection {
	public class Camera : XrObject {
        public Camera () {
            Fov = new Vector2( MathF.PI * 2 - 4 * MathF.Atan( 16f / 9 ), MathF.PI );
            FovBindable.BindValueChanged( v => {
                XSlope = 1 / MathF.Tan( ( MathF.PI - v.NewValue.X / 2 ) / 2 );
                YSlope = 1 / MathF.Tan( ( MathF.PI - v.NewValue.Y / 2 ) / 2 );
                AspectRatio = XSlope / YSlope;
                CameraClipMatrix = Matrix4x4.CreatePerspectiveProjection( XSlope, YSlope, 0.01f, 1000 );
            }, true );
        }

        List<XrObject> depthTestedRenderTargets = new();
        List<XrObject> renderTargets = new();
        private bool shoudBeDepthTested ( XrObject target ) {
            return target is not IBehindEverything;
		}
        private void addRenderTarget ( XrObject parent, XrObject child ) {
            if ( shoudBeDepthTested( child ) )
                depthTestedRenderTargets.Add( child );
            else
                renderTargets.Add( child );
        }
        private void removeRenderTarget ( XrObject parent, XrObject child ) {
            if ( shoudBeDepthTested( child ) )
                depthTestedRenderTargets.Remove( child );
            else
                renderTargets.Remove( child );
        }

        [BackgroundDependencyLoader]
        private void load () {
            Root.BindHierarchyChange( addRenderTarget, removeRenderTarget, true );
        }

		/// <summary>
		/// Field of view in radians.
		/// </summary>
		public Vector2 Fov {
            get => FovBindable.Value;
            set {
                FovBindable.Value = value;
            }
        }
        public readonly Bindable<Vector2> FovBindable = new();
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

            return p.Z > 0;
        }

        public void Render ( XrObject xrObject, DepthFrameBuffer depthBuffer ) {
            Vector2 scale;
            if ( depthBuffer.Size.X / depthBuffer.Size.Y > AspectRatio ) {
                scale = new( AspectRatio * AspectRatio, 1 ); // TODO why square?
            }
            else {
                scale = new( 1, 1 / AspectRatio / AspectRatio );
            }

            var settings = new XrObjectDrawNode.DrawSettings {
                Camera = this,
                CameraToClip = CameraClipMatrix,
                WorldToCamera = Matrix4x4.CreateScale( scale.X, scale.Y ) * WorldCameraMatrix
            };

            Render( xrObject, depthBuffer, settings );
        }

        public void Render ( XrObject xrObject, DepthFrameBuffer depthBuffer, XrObjectDrawNode.DrawSettings settings ) {
            settings = settings with { Camera = this };

            GLWrapper.PushViewport( new RectangleI( 0, 0, (int)depthBuffer.Size.X, (int)depthBuffer.Size.Y ) );
            GLWrapper.PushScissor( new RectangleI( 0, 0, (int)depthBuffer.Size.X, (int)depthBuffer.Size.Y ) );
            GLWrapper.PushScissorOffset( Vector2I.Zero );
            depthBuffer.Bind();
            GLWrapper.PushDepthInfo( new DepthInfo( false, false, osuTK.Graphics.ES30.DepthFunction.Less ) );
            GL.Clear( ClearBufferMask.ColorBufferBit );
            foreach ( var i in renderTargets ) {
                i.BeforeDraw( settings );
                i.DrawNode?.Draw( settings );
            }

            GLWrapper.PushDepthInfo( new DepthInfo( true, true, osuTK.Graphics.ES30.DepthFunction.Less ) );
            GL.Clear( ClearBufferMask.DepthBufferBit );
            foreach ( var i in depthTestedRenderTargets ) {
                i.BeforeDraw( settings );
                i.DrawNode?.Draw( settings );
            }

            GLWrapper.PopDepthInfo();
            GLWrapper.PopDepthInfo();
            depthBuffer.Unbind();
            GLWrapper.PopScissorOffset();
            GLWrapper.PopScissor();
            GLWrapper.PopViewport();
        }

        protected override void Dispose ( bool isDisposing ) {
			base.Dispose( isDisposing );
            var root = Root;
            root.ChildAddedToHierarchy -= addRenderTarget;
            root.ChildRemovedFromHierarchy -= removeRenderTarget;
        }
	}
}
