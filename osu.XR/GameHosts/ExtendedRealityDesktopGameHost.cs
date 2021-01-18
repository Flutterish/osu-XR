using osu.Framework;
using osu.Framework.Configuration;
using osu.Framework.Input;
using osu.Framework.Input.Handlers;
using osu.Framework.Input.Handlers.Joystick;
using osu.Framework.Input.Handlers.Keyboard;
using osu.Framework.Input.Handlers.Midi;
using osu.Framework.Input.Handlers.Mouse;
using osu.Framework.Platform;
using osuTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace osu.XR.GameHosts {
	public abstract class ExtendedRealityDesktopGameHost : ExtendedRealityGameHost {
        private TcpIpcProvider ipcProvider;
        private readonly bool bindIPCPort;
        private Thread ipcThread;

        internal bool UseOsuTK { get; }

        protected ExtendedRealityDesktopGameHost ( string gameName = @"", bool bindIPCPort = false, ToolkitOptions toolkitOptions = default, bool portableInstallation = false, bool useOsuTK = false )
            : base( gameName, toolkitOptions ) {
            this.bindIPCPort = bindIPCPort;
            IsPortableInstallation = portableInstallation;
            UseOsuTK = useOsuTK;
        }

        protected sealed override Storage GetDefaultGameStorage () {
            if ( IsPortableInstallation || File.Exists( Path.Combine( RuntimeInfo.StartupDirectory, @"framework.ini" ) ) )
                return GetStorage( RuntimeInfo.StartupDirectory );

            return base.GetDefaultGameStorage();
        }

        public sealed override Storage GetStorage ( string path ) => new NativeStorage( path, this );

        protected override void SetupForRun () {
            if ( bindIPCPort )
                startIPC();

            base.SetupForRun();
        }

        protected override void SetupToolkit () {
            if ( UseOsuTK )
                base.SetupToolkit();
        }

        private void startIPC () {
            Debug.Assert( ipcProvider == null );

            ipcProvider = new TcpIpcProvider();
            IsPrimaryInstance = ipcProvider.Bind();

            if ( IsPrimaryInstance ) {
                ipcProvider.MessageReceived += OnMessageReceived;

                ipcThread = new Thread( () => ipcProvider.StartAsync().Wait() ) {
                    Name = "IPC",
                    IsBackground = true
                };

                ipcThread.Start();
            }
        }

        public bool IsPortableInstallation { get; }

        private void openUsingShellExecute ( string path ) => Process.Start( new ProcessStartInfo {
            FileName = path,
            UseShellExecute = true //see https://github.com/dotnet/corefx/issues/10361
        } );

        public override ITextInputSource GetTextInput () => Window == null ? null : new GameWindowTextInput( Window );

        protected override IEnumerable<InputHandler> CreateAvailableInputHandlers () {
            var defaultEnabled = new InputHandler[]
            {
                new KeyboardHandler(),
                new MouseHandler(),
                new JoystickHandler(),
                new MidiInputHandler()
            };

            var defaultDisabled = new InputHandler[]
            {
                typeof( MouseHandler ).Assembly.CreateInstance( "osu.Framework.Input.Handlers.Mouse.OsuTKRawMouseHandler" ) as InputHandler
            };

            foreach ( var h in defaultDisabled )
                h.Enabled.Value = false;

            return defaultEnabled.Concat( defaultDisabled );
        }

        public override Task SendMessageAsync ( IpcMessage message ) => ipcProvider.SendMessageAsync( message );

        protected override void Dispose ( bool isDisposing ) {
            ipcProvider?.Dispose();
            ipcThread?.Join( 50 );
            base.Dispose( isDisposing );
        }
    }
}
