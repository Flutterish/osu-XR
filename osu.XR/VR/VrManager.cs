using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Testing.Drawables;
using osu.Framework.Timing;
using osu.Game.Screens.OnlinePlay.Components;
using osu.XR.Graphics;
using osu.XR.Maths;
using osu.XR.VR.ActionManifest;
using osuTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Valve.VR;

namespace osu.XR.VR {
	public static class VrManager {
		static event System.Action onUpdateThread;

		// NOTE platform specific binaries for openVR are there: https://github.com/ValveSoftware/openvr/tree/master/bin
		public static VrState VrState { get => VrStateBindable.Value; set => VrStateBindable.Value = value; }
		public static readonly Bindable<VrState> VrStateBindable = new( VrState.NotInitialized );
		public static CVRSystem CVRSystem { get; private set; }
		public static Vector2 RenderSize { get; private set; }

		private static double lastInitializationAttempt;
		private static double initializationAttemptInterval = 5000;
		public static readonly VrInput Current = new();
		internal static void UpdateDraw ( IClock clock ) {
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

			if ( ActionManifest is not null ) SetManifest();
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

			for ( int i = 0; i < trackedRenderDevices.Length + trackedGameDevices.Length; i++ ) {
				int index = i < trackedRenderDevices.Length ? i : ( i - trackedRenderDevices.Length );
				var device = i < trackedRenderDevices.Length ? trackedRenderDevices[ index ] : trackedGameDevices[ index ];
				if ( device.bPoseIsValid && device.bDeviceIsConnected ) {
					switch ( OpenVR.System.GetTrackedDeviceClass( (uint)i ) ) {
						case ETrackedDeviceClass.HMD:
							Current.Headset.Position = device.mDeviceToAbsoluteTracking.ExtractPosition();
							Current.Headset.Rotation = device.mDeviceToAbsoluteTracking.ExtractRotation();
							break;

						case ETrackedDeviceClass.Controller:
							if ( !Current.Controllers.ContainsKey( i ) ) {
								var sb = new StringBuilder( 256 );
								var propError = ETrackedPropertyError.TrackedProp_Success;
								CVRSystem.GetStringTrackedDeviceProperty( (uint)index, ETrackedDeviceProperty.Prop_RenderModelName_String, sb, 256, ref propError );
								Mesh mesh = new();
								Controller controller;
								if ( propError != ETrackedPropertyError.TrackedProp_Success ) {
									controller = new() { ID = i, ModelName = DEFAULT_CONTROLLER_MODEL, Mesh = mesh };
									Logger.Error( null, $"Couldn't find a model for controller with id {index}. Using Valve Index Controller (Left)" );
								}
								else {
									controller = new() { ID = i, ModelName = sb.ToString(), Mesh = mesh };
								}
								Current.Controllers.Add( i, controller );
								_ = LoadModelAsync( controller.ModelName, mesh );
							}
							Current.Controllers[ i ].Position = device.mDeviceToAbsoluteTracking.ExtractPosition();
							Current.Controllers[ i ].Rotation = device.mDeviceToAbsoluteTracking.ExtractRotation();
							break;
					}
				}
			}
		}

		public static void Update () {
			onUpdateThread?.Invoke();
			onUpdateThread = null;

			if ( !VrState.HasFlag( VrState.OK ) ) return;
			if ( ActionManifest is null || !AreComponentsLoaded ) return;

			var vrEvents = new List<VREvent_t>();
			var vrEvent = new VREvent_t();
			try {
				while ( OpenVR.System.PollNextEvent( ref vrEvent, (uint)Marshal.SizeOf<VREvent_t>() ) ) {
					vrEvents.Add( vrEvent );
				}
			}
			catch ( Exception e ) {
				Logger.Error( e, "Could not get events" );
			}

			// Printing events
			foreach ( var e in vrEvents ) {
				var pid = e.data.process.pid;
				if ( (EVREventType)vrEvent.eventType != EVREventType.VREvent_None ) {
					var name = Enum.GetName( typeof( EVREventType ), e.eventType );
					var message = $"[{pid}] {name}";
					if ( pid == 0 ) Logger.Log( message );
					else if ( name == null ) Logger.Log( message );
					else if ( name.ToLower().Contains( "fail" ) ) 
						Logger.Error( null, message );
					else if ( name.ToLower().Contains( "error" ) ) 
						Logger.Error( null, message );
					else if ( name.ToLower().Contains( "success" ) ) 
						Logger.Log( message );
					else Logger.Log( message );
				}
			}

			OpenVR.Input.UpdateActionState( actionSets, (uint)System.Runtime.InteropServices.Marshal.SizeOf<VRActiveActionSet_t>() );
			foreach ( var i in components ) {
				i.Value.Update();
			}
		}

		static async Task LoadModelAsync ( string modelName, Mesh target ) {
			target.IsReady = false;
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
					target.IsReady = true;
					return;
				}
				else {
					Logger.Error( null, $"Model `{modelName}` could not be loaded." );
					target.IsReady = true;
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

		public static Manifest ActionManifest { get; private set; }
		public static void SetManifest ( Manifest manifest ) {
			ActionManifest = manifest;
			if ( VrState.HasFlag( VrState.OK ) ) SetManifest();
		}

		public const string ACTION_MANIFEST_NAME = "openVR_action_manifest.json";
		public const string VR_MANIFEST_NAME = "openVR_vrmanifest.vrmanifest";
		public static bool AreComponentsLoaded { get; private set; }
		static Dictionary<object, ControllerComponent> components = new();
		static VRActiveActionSet_t[] actionSets;
		static void SetManifest () {
			var raw = RawManifest.Parse( ActionManifest );
			var actionManifestPath = Path.Combine( Directory.GetCurrentDirectory(), ACTION_MANIFEST_NAME );
			File.WriteAllText( actionManifestPath, System.Text.Json.JsonSerializer.Serialize( raw.actionManifest, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true } ) );
			var vrManifestPath = Path.Combine( Directory.GetCurrentDirectory(), VR_MANIFEST_NAME );
			File.WriteAllText( vrManifestPath, System.Text.Json.JsonSerializer.Serialize( raw.vrManifest, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true } ) );
			var error = OpenVR.Applications.AddApplicationManifest( vrManifestPath, true );
			if ( error != EVRApplicationError.None ) {
				Logger.Error( null, $"Couldn't set application vr manifest: {error}" ); // TODO our own VR error panel
			}
			var error2 = OpenVR.Input.SetActionManifestPath( actionManifestPath );
			if ( error2 != EVRInputError.None ) {
				Logger.Error( null, $"Couldn't set application action manifest: {error2}" );
			}

			foreach ( var group in ActionManifest.EnumerateGroups() ) {
				foreach ( var action in group.EnumerateActions() ) {
					ulong handle = 0;
					OpenVR.Input.GetActionHandle( action.FullPath, ref handle );
					var comp = action.CreateComponent( handle );
					components.Add( comp.Name, comp );
				}
			}

			actionSets = ActionManifest.EnumerateGroups().Select( x => {
				ulong handle = 0;
				var error = OpenVR.Input.GetActionSetHandle( x.FullPath, ref handle );
				if ( error != EVRInputError.None ) {
					Logger.Error( null, error.ToString() );
				}

				return new VRActiveActionSet_t { ulActionSet = handle };
			} ).ToArray();

			AreComponentsLoaded = true;
		}

		public static T GetControllerComponent<T> ( object name ) where T : ControllerComponent {
			if ( components.TryGetValue( name, out var comp ) ) {
				return comp as T;
			}
			else return null;
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
