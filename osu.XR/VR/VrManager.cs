using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Timing;
using osu.XR.Graphics;
using osu.XR.Maths;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace osu.XR.VR {
	public static class VrManager {
		// NOTE platform specific binaries for openVR are there: https://github.com/ValveSoftware/openvr/tree/master/bin
		public static VrState VrState { get => VrStateBindable.Value; set => VrStateBindable.Value = value; }
		public static readonly Bindable<VrState> VrStateBindable = new( VrState.NotInitialized );
		public static CVRSystem CVRSystem { get; private set; }
		public static Vector2 RenderSize { get; private set; }

		private static double lastInitializationAttempt;
		private static double initializationAttemptInterval = 5000;
		public static readonly VrInput Current = new();
		internal static void Update ( IClock clock ) {
			if ( VrState.HasFlag( VrState.NotInitialized ) && clock.CurrentTime >= lastInitializationAttempt + initializationAttemptInterval ) {
				lastInitializationAttempt = clock.CurrentTime;
				EVRInitError error = EVRInitError.None;
				CVRSystem = OpenVR.Init( ref error );
				if ( error == EVRInitError.None ) {
					VrStateBindable.Value = VrState.OK;
					Logger.Log( "OpenVR initialzed succesfuly" );
					InitializeOpenVR();
				}
				else {
					Logger.Error( null, $"OpenVR could not be initialized: {error.GetReadableDescription()}" );
				}
			}

			if ( VrState.HasFlag( VrState.OK ) ) {
				// TODO check if rig is still alive
				ReadVrInput();
			}
		}

		static void InitializeOpenVR () {
			// TODO log/explain startup errors https://github.com/ValveSoftware/openvr/wiki/API-Documentation
			uint w = 0, h = 0;
			CVRSystem.GetRecommendedRenderTargetSize( ref w, ref h );
			RenderSize = new Vector2( w, h );
		}

		const string DEFAULT_CONTROLLER_MODEL = "{indexcontroller}valve_controller_knu_1_0_left";
		private static readonly TrackedDevicePose_t[] trackedRenderDevices = new TrackedDevicePose_t[ OpenVR.k_unMaxTrackedDeviceCount ];
		private static readonly TrackedDevicePose_t[] trackedGameDevices = new TrackedDevicePose_t[ OpenVR.k_unMaxTrackedDeviceCount ];
		static void ReadVrInput () {
			// this blocks on the draw thread but it needs to be there ( it limits fps to headset framerate so its fine )
			var error = OpenVR.Compositor.WaitGetPoses( trackedRenderDevices, trackedGameDevices );
			if ( error != EVRCompositorError.None ) {
				Logger.Error( null, $"Pose error: {error}" );
				return;
			}
			TrackedDevicePose_t? headset = default;
			for ( int i = 0; i < trackedRenderDevices.Length + trackedGameDevices.Length; i++ ) {
				int index = i < trackedRenderDevices.Length ? i : ( i - trackedRenderDevices.Length );
				var device = i < trackedRenderDevices.Length ? trackedRenderDevices[ index ] : trackedGameDevices[ index ];
				if ( device.bPoseIsValid && device.bDeviceIsConnected ) {
					switch ( OpenVR.System.GetTrackedDeviceClass( (uint)i ) ) {
						case ETrackedDeviceClass.HMD:
							headset = device;
							break;

						case ETrackedDeviceClass.Controller:
							if ( !Current.Controllers.ContainsKey( i ) ) {
								StringBuilder sb = new(256);
								var propError = ETrackedPropertyError.TrackedProp_Success;
								CVRSystem.GetStringTrackedDeviceProperty( (uint)index, ETrackedDeviceProperty.Prop_RenderModelName_String, sb, 256, ref propError );
								Mesh mesh = new();
								if ( propError != ETrackedPropertyError.TrackedProp_Success ) {
									Current.Controllers.Add( i, new() { ID = i, ModelName = DEFAULT_CONTROLLER_MODEL, Mesh = mesh } );
									Logger.Error( null, $"Couldn't find a model for controller with id {index}. Using Valve Index Controller (Left)" );
								}
								else {
									Current.Controllers.Add( i, new() { ID = i, ModelName = sb.ToString(), Mesh = mesh } );
								}
								_ = LoadModelAsync( Current.Controllers[ i ].ModelName, mesh );
							}
							Current.Controllers[ i ].Position = device.mDeviceToAbsoluteTracking.ExtractPosition();
							Current.Controllers[ i ].Rotation = device.mDeviceToAbsoluteTracking.ExtractRotation();
							break;
					}
				}
			}

			if ( headset is not null ) {
				Current.Headset.Position = headset.Value.mDeviceToAbsoluteTracking.ExtractPosition();
				Current.Headset.Rotation = headset.Value.mDeviceToAbsoluteTracking.ExtractRotation();
			}
		}

		static async Task LoadModelAsync ( string modelName, Mesh target ) {
			IntPtr ptr = IntPtr.Zero;
			while ( true ) {
				var error = OpenVR.RenderModels.LoadRenderModel_Async( modelName, ref ptr );
				if ( error == EVRRenderModelError.Loading ) {
					await Task.Delay( 100 );
				}
				else if ( error == EVRRenderModelError.None ) {
					RenderModel_t model = new RenderModel_t();

					if ( ( System.Environment.OSVersion.Platform == System.PlatformID.MacOSX ) || ( System.Environment.OSVersion.Platform == System.PlatformID.Unix ) ) {
						var packedModel = (RenderModel_t_Packed)Marshal.PtrToStructure( ptr, typeof( RenderModel_t_Packed ) );
						packedModel.Unpack( ref model );
					}
					else {
						model = (RenderModel_t)Marshal.PtrToStructure( ptr, typeof( RenderModel_t ) );
					}

					var type = typeof( RenderModel_Vertex_t );
					for ( int iVert = 0; iVert < model.unVertexCount; iVert++ ) {
						var ptr2 = new System.IntPtr( model.rVertexData.ToInt64() + iVert * Marshal.SizeOf( type ) );
						var vert = (RenderModel_Vertex_t)Marshal.PtrToStructure( ptr2, type );

						target.Vertices.Add( new Vector3( vert.vPosition.v0, vert.vPosition.v1, -vert.vPosition.v2 ) );
						target.TextureCoordinates.Add( new Vector2( 0, 0 ) );
					}

					int indexCount = (int)model.unTriangleCount * 3;
					var indices = new short[ indexCount ];
					Marshal.Copy( model.rIndexData, indices, 0, indices.Length );

					for ( int iTri = 0; iTri < model.unTriangleCount; iTri++ ) {
						target.Tris.Add( new IndexedFace(
							(uint)indices[ iTri * 3 + 2 ],
							(uint)indices[ iTri * 3 + 1 ],
							(uint)indices[ iTri * 3 + 0 ]
						) );
					}
					// TODO load textures
					// https://github.com/ValveSoftware/steamvr_unity_plugin/blob/9cc1a76226648d8deb7f3900ab277dfaaa80d60c/Assets/SteamVR/Scripts/SteamVR_RenderModel.cs#L377
					return;
				}
				else {
					Logger.Error( null, $"Model `{modelName}` could not be loaded." );
					return;
				}
			}
		}

		internal static void Exit () {
			if ( CVRSystem is not null ) {
				OpenVR.Shutdown();
				CVRSystem = null;
			}
		}
	}

	public static class EVRInitErrorExtensions {
		private static Dictionary<EVRInitError, string> descriptions = new() {
			[EVRInitError.Init_HmdNotFound] = "Headset not found. This can be a USB issue, or your VR rig might just not be turned on.",
			[EVRInitError.Init_HmdNotFoundPresenceFailed] = "Headset not found. This can be a USB issue, or your VR rig might just not be turned on."
		};

		public static string GetReadableDescription ( this EVRInitError error ) {
			if ( descriptions.ContainsKey( error ) ) return $"{descriptions[ error ]} ({error})";
			else return $"{error}";
		}
	}

	public class VrInput {
		public readonly Headset Headset = new();
		public readonly Dictionary<int, Controller> Controllers = new();
	}

	public class Headset {
		public Vector3 Position;
		public Quaternion Rotation;
	}

	public class Controller {
		public Vector3 Position;
		public Quaternion Rotation;
		public int ID { get; init; }
		public string ModelName { get; init; }
		public Mesh Mesh { get; init; }
	}
}
