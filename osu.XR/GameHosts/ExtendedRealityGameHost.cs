using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;
using osu.Game;
using osu.XR.Graphics;
using osu.XR.Maths;
using osu.XR.VR;
using osuTK;
using osuTK.Graphics.ES30;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Threading;
using Valve.VR;
using WindowState = osu.Framework.Platform.WindowState;

namespace osu.XR.GameHosts {
	public abstract class ExtendedRealityGameHost : GameHost {
        protected ExtendedRealityGameHost ( string gameName = "", ToolkitOptions toolkitOptions = null ) : base( gameName, toolkitOptions ) { }

		public override void OpenFileExternally ( string filename ) {
			throw new NotImplementedException( "File dialog panel is not yet implemented" ); // TODO file dialog and browser panels
		}

		public override void OpenUrlExternally ( string url ) {
			throw new NotImplementedException( "Web browser panel is not yet implemented" );
		}

		XrGame runningGame;
		DepthFrameBuffer leftEye = new();
		DepthFrameBuffer rightEye = new();
		public void Run ( XrGame game ) {
			runningGame = game;
			base.Run( game );
		}

		protected override void DrawFrame () {
			base.DrawFrame();
			var vr = runningGame.VrManager;
			if ( vr is null || !vr.VrState.HasFlag( VrState.OK ) ) return;

			if ( leftEye.Size != vr.RenderSize ) {
				leftEye.Size = vr.RenderSize;
				rightEye.Size = vr.RenderSize;
			}

			var lMatrix = vr.CVRSystem.GetProjectionMatrix( EVREye.Eye_Left, 0.01f, 1000 );
			var rMatrix = vr.CVRSystem.GetProjectionMatrix( EVREye.Eye_Right, 0.01f, 1000 );

			var leftEyeMatrix =
				new Matrix4x4( 
					lMatrix.m0, lMatrix.m1, lMatrix.m2, lMatrix.m3, 
					lMatrix.m4, lMatrix.m5, lMatrix.m6, lMatrix.m7,
					lMatrix.m8, lMatrix.m9, -lMatrix.m10, lMatrix.m11, 
					lMatrix.m12, lMatrix.m13, -lMatrix.m14, lMatrix.m15 
				);
			var rightEyeMatrix =
				new Matrix4x4( 
					rMatrix.m0, rMatrix.m1, rMatrix.m2, rMatrix.m3, 
					rMatrix.m4, rMatrix.m5, rMatrix.m6, rMatrix.m7, 
					rMatrix.m8, rMatrix.m9, -rMatrix.m10, rMatrix.m11, 
					rMatrix.m12, rMatrix.m13, -rMatrix.m14, rMatrix.m15 
				);

			runningGame.Scene.Camera.Render( runningGame.Scene, leftEye, new Components.XrObject.XrObjectDrawNode.DrawSettings { WorldToCamera = runningGame.Scene.Camera.WorldCameraMatrix, CameraToClip = leftEyeMatrix } );
			runningGame.Scene.Camera.Render( runningGame.Scene, rightEye, new Components.XrObject.XrObjectDrawNode.DrawSettings { WorldToCamera = runningGame.Scene.Camera.WorldCameraMatrix, CameraToClip = rightEyeMatrix } );

			Texture_t left = new Texture_t { eColorSpace = EColorSpace.Linear, eType = ETextureType.OpenGL, handle = (IntPtr)leftEye.Texture.TextureId };
			Texture_t right = new Texture_t { eColorSpace = EColorSpace.Linear, eType = ETextureType.OpenGL, handle = (IntPtr)rightEye.Texture.TextureId };
			VRTextureBounds_t bounds = new VRTextureBounds_t { uMin = 0, uMax = 1, vMin = 0, vMax = 1 };
			EVRCompositorError[] errors = new EVRCompositorError[ 3 ];
			errors[ 2 ] = OpenVR.Compositor.WaitGetPoses( Array.Empty<TrackedDevicePose_t>(), Array.Empty<TrackedDevicePose_t>() );
			if ( errors[ 2 ] is not EVRCompositorError.None ) {
				Logger.Error( null, $"Pose error: {errors[2]}" );
				return;
			}

			leftEye.Texture.Bind();
			errors[ 0 ] = OpenVR.Compositor.Submit( EVREye.Eye_Left, ref left, ref bounds, EVRSubmitFlags.Submit_Default );
			rightEye.Texture.Bind();
			errors[ 1 ] = OpenVR.Compositor.Submit( EVREye.Eye_Right, ref right, ref bounds, EVRSubmitFlags.Submit_Default );

			if ( errors[ 0 ] is not EVRCompositorError.None || errors[ 1 ] is not EVRCompositorError.None ) {
				Logger.Error( null, $"Frame submit errors: Left eye ({errors[0]}), Right eye ({errors[1]})" );
				return;
			}
		}
	}
}
