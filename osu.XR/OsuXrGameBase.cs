using OpenVR.NET;
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
	protected BeatSyncSource BeatSync = new();

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

		//Bindings.Value.SettingsChanged += () => {
		//	var opts = new JsonSerializerOptions {
		//		IncludeFields = true,
		//		WriteIndented = true,
		//		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		//	};
		//	opts.Converters.Add( new JsonStringEnumConverter() );
		//	File.WriteAllText( "C:\\Users\\Charm\\AppData\\Roaming\\osu\\XR\\test.json", System.Text.Json.JsonSerializer.Serialize( Bindings.Value.CreateSaveData( new() ), opts ) );
		//};
	}

	Storage storage = null!;
	OsuXrConfigManager config = null!;

	DependencyContainer dependencies = null!;
	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent )
		=> dependencies = new DependencyContainer( base.CreateChildDependencies( parent ) );

	[BackgroundDependencyLoader]
	private void load ( GameHost host ) {
		storage = host.Storage.GetStorageForDirectory( "XR" );
		dependencies.CacheAs( storage );
		config = new();
		dependencies.CacheAs( config );
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
