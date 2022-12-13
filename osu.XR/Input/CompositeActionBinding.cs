namespace osu.XR.Input;

/// <summary>
/// An <see cref="ActionBinding"/> which is composed of other <see cref="ActionBinding"/>s
/// </summary>
public abstract class CompositeActionBinding : ActionBinding {
	public override bool ShouldBeSaved => Children.Any( x => x.ShouldBeSaved );

	public CompositeActionBinding () {
		Children.BindCollectionChanged( ( _, e ) => {
			if ( e.OldItems != null ) {
				foreach ( ActionBinding i in e.OldItems ) {
					i.SettingsChanged -= OnSettingsChanged;
				}
			}
			if ( e.NewItems != null ) {
				foreach ( ActionBinding i in e.NewItems ) {
					i.SettingsChanged += OnSettingsChanged;
				}
			}
			OnSettingsChanged();
		} );
	}

	public virtual void Add ( ActionBinding action )
		=> Children.Add( action );
	public virtual void Remove ( ActionBinding action )
		=> Children.Remove( action );

	public readonly BindableList<ActionBinding> Children = new();
}

public abstract class UniqueCompositeActionBinding<K> : CompositeActionBinding where K : notnull {
	HashSet<K> childKeys = new();
	protected abstract K? GetKey ( ActionBinding action );

	public override void Add ( ActionBinding action ) {
		var key = GetKey( action );
		if ( key != null && childKeys.Add( key ) ) {
			base.Add( action );
		}
	}

	public override void Remove ( ActionBinding action ) {
		base.Remove( action );
		var key = GetKey( action );
		if ( key != null )
			childKeys.Remove( key );
	}
}