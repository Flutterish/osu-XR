﻿using OpenVR.NET;
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

	public readonly Bindable<Hand> DominantHand = new( Hand.Right );
	Bindable<HandSetting> dominantHandSetting = new( HandSetting.Right );

	public OsuXrGameBase ( bool useSimulatedVR = true ) {
		Compositor = useSimulatedVR ? new TestingVrCompositor() : new VrCompositor();
		VrInput = Compositor.Input;

		Compositor.Input.DominantHandBindable.BindValueChanged( v => {
			if ( dominantHandSetting.Value is HandSetting.Auto )
				DominantHand.Value = v.NewValue;
		} );

		dominantHandSetting.BindValueChanged( v => {
			if ( v.NewValue is HandSetting.Auto ) {
				DominantHand.Value = Compositor.Input.DominantHandBindable.Value;
			}
			else {
				DominantHand.Value = v.NewValue == HandSetting.Right ? Hand.Right : Hand.Left;
			}
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

		var forcedActiveWindowBindable = (Bindable<bool>)Window.IsActive;
		forcedActiveWindowBindable.BindValueChanged( v => {
			if ( !v.NewValue ) {
				forcedActiveWindowBindable.Value = true;
			}
		} );
	}

	protected override bool OnExiting () {
		if ( DebugUtils.IsDebugBuild )
			return base.OnExiting();
		
		if ( !Bindings.IsDefault ) {
			if ( storage.Exists( "Bindings.json~" ) )
				storage.Delete( "Bindings.json~" );
			if ( storage.Exists( "Bindings.json" ) )
				storage.Move( "Bindings.json", "Bindings.json~" );

			Bindings.Value.SaveToStorage( storage, "Bindings.json", new() );
		}

		config.Save();

		return base.OnExiting();
	}

	protected override void LoadComplete () {
		base.LoadComplete();

		config.BindWith( OsuXrSetting.DominantHand, dominantHandSetting );
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
