using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Handlers;
using osu.Framework.Input.Handlers.Joystick;
using osu.Framework.Input.Handlers.Keyboard;
using osu.Framework.Input.Handlers.Midi;
using osu.Framework.Input.Handlers.Mouse;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;
using osuTK;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace osu.XR.GameHosts {
	public class ExtendedRealityWindowsGameHost : ExtendedRealityDesktopGameHost { // this is a copy of DesktopGameHost and WindowsGameHost
		private TimePeriod timePeriod;

		public override string UserStoragePath => Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );

#if NET5_0
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
#endif
		public override bool CapsLockEnabled => Console.CapsLock;

		internal ExtendedRealityWindowsGameHost ( string gameName, bool bindIPC = false, ToolkitOptions toolkitOptions = default, bool portableInstallation = false, bool useOsuTK = false )
			: base( gameName, bindIPC, toolkitOptions, portableInstallation, useOsuTK ) {
		}

		public override void OpenFileExternally ( string filename ) {
			if ( Directory.Exists( filename ) ) {
				Process.Start( "explorer.exe", filename );
				return;
			}

			base.OpenFileExternally( filename );
		}

		protected override void SetupForRun () {
			base.SetupForRun();

			// OnActivate / OnDeactivate may not fire, so the initial activity state may be unknown here.
			// In order to be certain we have the correct activity state we are querying the Windows API here.

			timePeriod = new TimePeriod( 1 ) { Active = true };
		}

		protected override IWindow CreateWindow () => new WindowsWindow();

		public override IEnumerable<KeyBinding> PlatformKeyBindings => base.PlatformKeyBindings.Concat( new[]
		{
			new KeyBinding(new KeyCombination(InputKey.Alt, InputKey.F4), new PlatformAction(PlatformActionType.Exit))
		} ).ToList();

		protected override void Dispose ( bool isDisposing ) {
			timePeriod?.Dispose();
			base.Dispose( isDisposing );
		}

		protected override void OnActivated () {
			timePeriod.Active = true;

			Execution.SetThreadExecutionState( Execution.ExecutionState.Continuous | Execution.ExecutionState.SystemRequired | Execution.ExecutionState.DisplayRequired );
			base.OnActivated();
		}

		protected override void OnDeactivated () {
			timePeriod.Active = false;

			Execution.SetThreadExecutionState( Execution.ExecutionState.Continuous );
			base.OnDeactivated();
		}
	}

	internal class TimePeriod : IDisposable {
		private static readonly TimeCaps time_capabilities;

		private readonly int period;

		[DllImport( @"winmm.dll", ExactSpelling = true )]
		private static extern int timeGetDevCaps ( ref TimeCaps ptc, int cbtc );

		[DllImport( @"winmm.dll", ExactSpelling = true )]
		private static extern int timeBeginPeriod ( int uPeriod );

		[DllImport( @"winmm.dll", ExactSpelling = true )]
		private static extern int timeEndPeriod ( int uPeriod );

		internal static int MinimumPeriod => time_capabilities.wPeriodMin;
		internal static int MaximumPeriod => time_capabilities.wPeriodMax;

		private bool canAdjust = MaximumPeriod > 0;

		static TimePeriod () {
			timeGetDevCaps( ref time_capabilities, Marshal.SizeOf( typeof( TimeCaps ) ) );
		}

		internal TimePeriod ( int period ) {
			this.period = period;
		}

		private bool active;

		internal bool Active {
			get => active;
			set {
				if ( value == active || !canAdjust ) return;

				active = value;

				try {
					if ( active ) {
						canAdjust &= 0 == timeBeginPeriod( Math.Clamp( period, MinimumPeriod, MaximumPeriod ) );
					}
					else {
						timeEndPeriod( period );
					}
				}
				catch {
				}
			}
		}

		#region IDisposable Support

		private bool disposedValue; // To detect redundant calls

		protected virtual void Dispose ( bool disposing ) {
			if ( !disposedValue ) {
				Active = false;
				disposedValue = true;
			}
		}

		~TimePeriod () {
			Dispose( false );
		}

		public void Dispose () {
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		#endregion

		[StructLayout( LayoutKind.Sequential )]
		private readonly struct TimeCaps {
			internal readonly int wPeriodMin;
			internal readonly int wPeriodMax;
		}
	}

	internal static class Execution {
		[DllImport( "kernel32.dll" )]
		internal static extern uint SetThreadExecutionState ( ExecutionState state );

		[Flags]
		internal enum ExecutionState : uint {
			AwaymodeRequired = 0x00000040,
			Continuous = 0x80000000,
			DisplayRequired = 0x00000002,
			SystemRequired = 0x00000001,
			UserPresent = 0x00000004,
		}
	}
}
