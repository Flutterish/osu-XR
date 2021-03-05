using Humanizer;
using NuGet.Protocol.Core.Types;
using OpenVR.NET;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;
using osu.Framework.XR.Maths;
using osu.Game;
using osu.XR.Graphics;
using osuTK;
using osuTK.Graphics.ES30;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Threading;
using Valve.VR;
using WindowState = osu.Framework.Platform.WindowState;

namespace osu.XR.GameHosts {
	public abstract class ExtendedRealityGameHost : GameHost {
        protected ExtendedRealityGameHost ( string gameName = "", ToolkitOptions toolkitOptions = null ) : base( gameName, toolkitOptions ) { }


		public XrTextInput TextInput { get; } = new XrTextInput();
		public override ITextInputSource GetTextInput ()
			=> TextInput;

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
			VR.Exit();
		}

		static EVRCompositorError[] errors = new EVRCompositorError[ 2 ];
		protected override void DrawFrame () {
			base.DrawFrame();
			VR.UpdateDraw( SceneGraphClock.CurrentTime );
			if ( !VR.VrState.HasFlag( VrState.OK ) ) return;

			var size = new Vector2( VR.RenderSize.X, VR.RenderSize.Y );
			if ( leftEye.Size != size ) {
				leftEye.Size = size;
				rightEye.Size = size;
			}

			var lMatrix = VR.CVRSystem.GetProjectionMatrix( EVREye.Eye_Left, 0.01f, 1000 );
			var rMatrix = VR.CVRSystem.GetProjectionMatrix( EVREye.Eye_Right, 0.01f, 1000 );

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

			runningGame.Scene.Camera.Position = new Vector3( VR.Current.Headset.Position.X, VR.Current.Headset.Position.Y, VR.Current.Headset.Position.Z );
			runningGame.Scene.Camera.Rotation = new Quaternion( VR.Current.Headset.Rotation.X, VR.Current.Headset.Rotation.Y, VR.Current.Headset.Rotation.Z, VR.Current.Headset.Rotation.W );

			var el = VR.CVRSystem.GetEyeToHeadTransform( EVREye.Eye_Left );
			var er = VR.CVRSystem.GetEyeToHeadTransform( EVREye.Eye_Right );

			Matrix4x4 headToLeftEye = new Matrix4x4(
				el.m0, el.m1, el.m2, el.m3,
				el.m4, el.m5, el.m6, el.m7,
				el.m8, el.m9, el.m10, el.m11,
				0, 0, 0, 1
			);

			Matrix4x4 headToRightEye = new Matrix4x4(
				er.m0, er.m1, er.m2, er.m3,
				er.m4, er.m5, er.m6, er.m7,
				er.m8, er.m9, er.m10, er.m11,
				0, 0, 0, 1
			);

			runningGame.Scene.Camera.Render( runningGame.Scene, leftEye, new Components.XrObject.XrObjectDrawNode.DrawSettings { WorldToCamera = headToLeftEye * runningGame.Scene.Camera.WorldCameraMatrix, CameraToClip = leftEyeMatrix } );
			runningGame.Scene.Camera.Render( runningGame.Scene, rightEye, new Components.XrObject.XrObjectDrawNode.DrawSettings { WorldToCamera = headToRightEye * runningGame.Scene.Camera.WorldCameraMatrix, CameraToClip = rightEyeMatrix } );

			Texture_t left = new Texture_t { eColorSpace = EColorSpace.Linear, eType = ETextureType.OpenGL, handle = (IntPtr)leftEye.Texture.TextureId };
			Texture_t right = new Texture_t { eColorSpace = EColorSpace.Linear, eType = ETextureType.OpenGL, handle = (IntPtr)rightEye.Texture.TextureId };
			VR.SubmitFrame( EVREye.Eye_Right, left );
			VR.SubmitFrame( EVREye.Eye_Left, right );
		}

		protected override void UpdateFrame () {
			VR.Update();
			base.UpdateFrame();
		}
	}
}
