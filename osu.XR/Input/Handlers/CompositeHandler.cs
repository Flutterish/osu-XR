namespace osu.XR.Input.Handlers;

public partial class CompositeHandler<Tchild> : ActionBindingHandler where Tchild : IActionBinding {
	BindableList<Tchild> children = new();
	Dictionary<Tchild, ActionBindingHandler> childHandlers = new();
	public CompositeHandler ( CompositeActionBinding<Tchild> source ) {
		children.BindTo( source.Children );
		children.BindCollectionChanged( (_, e) => {
			if ( e.OldItems != null ) {
				foreach ( Tchild i in e.OldItems ) {
					if ( childHandlers.Remove( i, out var handler ) ) {
						RemoveInternal( handler, disposeImmediately: true );
					}
				}
			}
			if ( e.NewItems != null ) {
				foreach ( Tchild i in e.NewItems ) {
					var handler = i.CreateHandler();
					if ( handler != null ) {
						childHandlers.Add( i, handler );
						AddInternal( handler );
					}
				}
			}
		}, true );
	}
}
