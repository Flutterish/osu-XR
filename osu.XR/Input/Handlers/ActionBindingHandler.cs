using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.XR.VirtualReality;
using osu.Framework.XR.VirtualReality.Devices;
using osu.XR.Input.Actions;

namespace osu.XR.Input.Handlers;

public partial class ActionBindingHandler : CompositeComponent {
	[Resolved]
	protected OsuXrGameBase Game { get; private set; } = null!;
	[Resolved]
	protected VrCompositor VR { get; private set; } = null!;
	[Resolved]
	protected VrInput Input { get; private set; } = null!;
	[Resolved(canBeNull: true)]
	protected InjectedInput? Target { get; private set; }
	public readonly IActionBinding Source;
	new public virtual LocalisableString Name => Source.Name;
	public ActionBindingHandler ( IActionBinding source ) {
		Source = source;
	}

	List<(Bindable<object?> action, Bindable<bool> state)> boundActions = new();
	protected void RegisterAction ( RulesetAction action, Bindable<bool> state ) {
		Bindable<object?> ownAction = new() { BindTarget = action };
		boundActions.Add( (ownAction, state) );

		void press ( object action ) {
			Logger.Log( $@"Trigerred {action} with {Name}", "osu!xr-input", LogLevel.Debug );
			ActionPressed?.Invoke( action );
		}
		void release ( object action ) {
			Logger.Log( $@"Released {action} with {Name}", "osu!xr-input", LogLevel.Debug );
			ActionReleased?.Invoke( action );
		}

		ownAction.BindValueChanged( v => {
			if ( !state.Value )
				return;

			if ( v.OldValue != null )
				release( v.OldValue );
			if ( v.NewValue != null )
				press( v.NewValue );
		} );
		state.BindValueChanged( v => {
			if ( ownAction.Value is null )
				return;

			if ( v.NewValue )
				press( ownAction.Value );
			else
				release( ownAction.Value );
		} );
	}
	public event Action<object>? ActionPressed;
	public event Action<object>? ActionReleased;
	public IEnumerable<object> PressedActions => boundActions.Where( x => x.state.Value && x.action.Value != null ).Select( x => x.action.Value! );

	protected override void LoadComplete () {
		base.LoadComplete();
		if ( Target is null )
			return;

		foreach ( var i in PressedActions ) {
			Target.TriggerPressed( i );
		}
		ActionPressed += Target.TriggerPressed;
		ActionReleased += Target.TriggerReleased;
	}

	List<Action<VrDevice>> disposeActions = new();

	protected void GetDevice ( Func<VrDevice, bool> validator ) {
		void perform () {
			foreach ( var i in VR.TrackedDevices ) {
				if ( validator( i ) )
					return;
			}

			void onDeviceDetected ( VrDevice device ) {
				if ( validator( device ) ) {
					VR.DeviceDetected -= onDeviceDetected;
					disposeActions.Remove( onDeviceDetected );
				}
			}
			
			VR.DeviceDetected += onDeviceDetected;
			disposeActions.Add( onDeviceDetected );
		}

		if ( IsLoaded )
			perform();
		else
			OnLoadComplete += _ => perform();
	}

	protected void GetController ( Hand? hand, Action<Controller?> action ) {
		GetDevice( d => {
			if ( d is Controller controller && controller.Role == (hand == Hand.Left ? Valve.VR.ETrackedControllerRole.LeftHand : Valve.VR.ETrackedControllerRole.RightHand) ) {
				action( controller );
				return true;
			}
			return false;
		} );
	}

	protected override void Dispose ( bool isDisposing ) {
		if ( !IsDisposed ) {
			foreach ( var i in disposeActions ) {
				VR.DeviceDetected -= i;
			}
		}

		base.Dispose( isDisposing );
	}
}