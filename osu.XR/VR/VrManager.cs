using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace osu.XR.VR {
	[Cached]
	public class VrManager : Container {
		// NOTE platform specific binaries for openVR are there: https://github.com/ValveSoftware/openvr/tree/master/bin
		public VrState VrState { get => VrStateBindable.Value; set => VrStateBindable.Value = value; }
		public readonly Bindable<VrState> VrStateBindable = new( VrState.NotInitialized );
		public CVRSystem CVRSystem { get; private set; }
		public Vector2 RenderSize { get; private set; }

		private double lastInitializationAttempt;
		private double initializationAttemptInterval = 5000;
		protected override void LoadComplete () {
			// TODO log/explain startup errors https://github.com/ValveSoftware/openvr/wiki/API-Documentation
			base.LoadComplete();
		}

		protected override void Update () {
			base.Update();

			if ( VrState.HasFlag( VrState.NotInitialized ) && Clock.CurrentTime >= lastInitializationAttempt + initializationAttemptInterval ) {
				lastInitializationAttempt = Clock.CurrentTime;
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

		void InitializeOpenVR () {

		}

		void ReadVrInput () {
			uint w = 0, h = 0;
			CVRSystem.GetRecommendedRenderTargetSize( ref w, ref h );
			RenderSize = new Vector2( w, h );
		}

		protected override void Dispose ( bool isDisposing ) {
			base.Dispose( isDisposing );
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
}
