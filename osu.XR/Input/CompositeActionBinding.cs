using osu.XR.Input.Handlers;
using System.Diagnostics.CodeAnalysis;

namespace osu.XR.Input;

public abstract class CompositeActionBinding : CompositeActionBinding<ActionBinding> { }
/// <summary>
/// An <see cref="ActionBinding"/> which is composed of other <see cref="ActionBinding"/>s
/// </summary>
public abstract class CompositeActionBinding<Tchild> : ActionBinding where Tchild : IActionBinding {
	public override bool ShouldBeSaved => Children.Any( x => x.ShouldBeSaved );

	public CompositeActionBinding () {
		Children.BindCollectionChanged( ( _, e ) => {
			if ( e.OldItems != null ) {
				foreach ( Tchild i in e.OldItems ) {
					i.SettingsChanged -= OnSettingsChanged;
				}
			}
			if ( e.NewItems != null ) {
				foreach ( Tchild i in e.NewItems ) {
					i.SettingsChanged += OnSettingsChanged;
				}
			}
			OnSettingsChanged();
		} );
	}

	public override Drawable? CreateEditor () => null;
	public override ActionBindingHandler? CreateHandler () => null;

	public virtual bool Add ( Tchild action ) {
		Children.Add( action );
		return true;
	}
	public virtual bool Remove ( Tchild action )
		=> Children.Remove( action );

	public readonly BindableList<Tchild> Children = new();
}

public abstract class UniqueCompositeActionBinding<K> : UniqueCompositeActionBinding<ActionBinding, K> where K : notnull { }
public abstract class UniqueCompositeActionBinding<Tchild, K> : CompositeActionBinding<Tchild> where K : notnull where Tchild : IActionBinding {
	Dictionary<K, Tchild> childKeys = new();
	protected abstract K GetKey ( Tchild action );

	public Tchild GetOrAdd ( Tchild action ) {
		var key = GetKey( action );
		if ( childKeys.TryAdd( key, action ) ) {
			base.Add( action );
			return action;
		}
		else {
			return childKeys[key];
		}
	}
	public override bool Add ( Tchild action ) {
		return childKeys.TryAdd( GetKey( action ), action ) && base.Add( action );
	}

	public bool Remove ( K key ) {
		return childKeys.Remove( key, out var child ) && base.Remove( child );
	}
	public override bool Remove ( Tchild action ) {
		return childKeys.Remove( GetKey( action ) ) && base.Remove( action );
	}

	public bool TryGetChild ( K key, [NotNullWhen(true)] out Tchild? child ) {
		return childKeys.TryGetValue( key, out child );
	}

	public bool Contains ( Tchild action )
		=> childKeys.ContainsKey( GetKey( action ) );
}