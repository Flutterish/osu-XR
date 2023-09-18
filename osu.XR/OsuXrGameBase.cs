using OpenVR.NET;
using osu.Framework.Configuration;
using osu.Framework.Development;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.XR.Testing.VirtualReality;
using osu.Framework.XR.VirtualReality;
using osu.XR.Configuration;
using osu.XR.IO;
using osu.XR.Timing;
using osu.XR.VirtualReality;

namespace osu.XR;

[Cached]
public partial class OsuXrGameBase : Framework.Game {
	[Cached]
	VR vr = new();

	[Cached( typeof( VrCompositor ) )]
	public readonly VrCompositor Compositor;

	[Cached( typeof( VrInput ) )]
	public readonly VrInput VrInput;

	[Cached]
	VrResourceStore vrResourceStore = new();

	[Cached]
	protected readonly BeatSyncSource BeatSync = new();

	[Cached]
	public readonly Bindable<BindingsFile> Bindings = new( new() );

	public readonly Bindable<Hand> ActiveHand = new( Hand.Right );

	public OsuXrGameBase ( bool useSimulatedVR = true ) {
		Compositor = useSimulatedVR ? new TestingVrCompositor() : new VrCompositor();
		VrInput = Compositor.Input;

		Compositor.Input.DominantHandBindable.BindValueChanged( v => {
			ActiveHand.Value = v.NewValue;
		} );

		Add( Compositor );
		Compositor.Input.SetActionManifest( new OsuXrActionManifest() );
		Compositor.Input.BindManifestLoaded( vr => {
			vr.InstallApp( new OsuXrAppManifest() );
		} );
	}

	Storage storage = null!;
	OsuXrConfigManager config = null!;

	DependencyContainer dependencies = null!;
	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent )
		=> dependencies = new DependencyContainer( base.CreateChildDependencies( parent ) );

	protected override IDictionary<FrameworkSetting, object> GetFrameworkConfigDefaults () {
		Logging.LogAsOsuXr();
		return base.GetFrameworkConfigDefaults();
	}

	protected virtual void OnBindingsLoadMessage ( BindingsSaveContext.Message message ) { }

	[BackgroundDependencyLoader]
	private void load ( GameHost host, FrameworkConfigManager frameworkConfig ) {
		Resources.AddStore( new NamespacedResourceStore<byte[]>( new DllResourceStore( typeof( OsuXrGameBase ).Assembly ), "Resources" ) );
		storage = host.Storage.GetStorageForDirectory( "XR" );
		dependencies.CacheAs( storage );
		config = new( storage );
		dependencies.CacheAs( config );
		config.Load();
		BindingsSaveContext saveContext = new();
		saveContext.Messages.BindCollectionChanged( (_,e) => {
			if ( e.NewItems != null ) {
				foreach ( BindingsSaveContext.Message i in e.NewItems ) {
					OnBindingsLoadMessage( i );
				}
			}
		} );
		Bindings.Value = BindingsFile.LoadFromStorage( storage, "Bindings.json", saveContext );

		var renderer = frameworkConfig.GetBindable<RendererType>( FrameworkSetting.Renderer );
		renderer.Disabled = true;

		var mode = frameworkConfig.GetBindable<ExecutionMode>( FrameworkSetting.ExecutionMode );
		mode.Disabled = false;
		mode.Value = ExecutionMode.MultiThreaded;
		mode.Disabled = true;

		var framerate = frameworkConfig.GetBindable<FrameSync>( FrameworkSetting.FrameSync );
		framerate.Disabled = false;
		framerate.Value = FrameSync.Unlimited;
		framerate.Disabled = true;

		foreach ( var thread in host.Threads ) {
			var isActive = (Bindable<bool>)thread.IsActive;
			
			isActive.UnbindFrom( host.IsActive );
			isActive.Value = true;
		}
	}

	protected override bool OnExiting () {
		if ( DebugUtils.IsDebugBuild )
			return base.OnExiting();
		
		if ( !Bindings.IsDefault ) {
			Bindings.Value.SaveToStorage( storage, "Bindings.json", new() );
		}

		config.Save();

		return base.OnExiting();
	}

	protected override void Dispose ( bool isDisposing ) {
		if ( !IsDisposed ) {
			Task.Run( async () => {
				await Task.Delay( 1000 );
				vr.Exit();
				vrResourceStore.Dispose();
			} );
		}

		base.Dispose( isDisposing );
	}
}
