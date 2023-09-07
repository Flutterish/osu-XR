namespace osu.XR.Input.Handlers;

public abstract partial class HandlerMod : ActionBindingHandler {
	protected HandlerMod ( IActionBinding source ) : base( source ) { }

	protected override void LoadComplete () {
		base.LoadComplete();

		HandlerMods.Add( this );
	}

	protected override void Dispose ( bool isDisposing ) {
		HandlerMods.Remove( this );

		base.Dispose( isDisposing );
	}
}
