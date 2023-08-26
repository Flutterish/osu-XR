using osu.Framework;
using osu.Framework.Development;
using osu.Framework.Logging;
using osu.Framework.XR;

namespace osu.XR {
	public static class Program {
		[STAThread]
		public static int Main () {
			Environment.SetEnvironmentVariable( "OSU_FRAME_STATISTICS_VIA_TOUCH", "0", EnvironmentVariableTarget.Process );
			using var host = HostXR.GetSuitableDesktopHost( "osu", new HostOptions { BindIPC = true } );
			host.ExceptionThrown += handleException;
			host.Run( new OsuXrGame() );
			return 0;
		}

		private static int allowableExceptions = DebugUtils.IsDebugBuild ? 0 : 1;

		/// <summary>
		/// Allow a maximum of one unhandled exception, per second of execution.
		/// </summary>
		/// <param name="arg"></param>
		/// <returns></returns>
		private static bool handleException ( Exception arg ) {
			bool continueExecution = Interlocked.Decrement( ref allowableExceptions ) >= 0;

			Logger.Log( $"Unhandled exception has been {( continueExecution ? $"allowed with {allowableExceptions} more allowable exceptions" : "denied" )} ." );

			// restore the stock of allowable exceptions after a short delay.
			Task.Delay( 1000 ).ContinueWith( _ => Interlocked.Increment( ref allowableExceptions ) );

			return continueExecution;
		}
	}
}