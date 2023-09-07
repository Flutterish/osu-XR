using osu.XR.Graphics.Bindings.Editors;
using osu.XR.Input.Handlers;
using osu.XR.IO;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

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

	protected sealed override object CreateSaveData ( BindingsSaveContext context )
		=> CreateSaveData( Children.Where( x => x.ShouldBeSaved ), context );

	protected void LoadChildren ( IEnumerable<object> children, BindingsSaveContext context, Func<JsonElement, BindingsSaveContext, Tchild?> factory )
		=> LoadChildren<JsonElement>( children, context, factory );
	protected void LoadChildren<Tdata> ( IEnumerable<object> children, BindingsSaveContext context, Func<Tdata, BindingsSaveContext, Tchild?> factory, JsonSerializerOptions? options = null ) {
		foreach ( JsonElement i in children ) {
			if ( !context.DeserializeBindingData<Tdata>( i, out var data, options ) )
				continue;

			var child = factory( data, context );
			if ( child != null )
				Add( child );
			else
				context.Error( @"Could not load a binding", data );
		}
	}

	protected abstract object CreateSaveData ( IEnumerable<Tchild> children, BindingsSaveContext context );

	protected object[] CreateSaveDataAsArray ( IEnumerable<Tchild> children, BindingsSaveContext context )
		=> children.Select( x => x.GetSaveData( context ) ).ToArray();
	protected List<object> CreateSaveDataAsList ( IEnumerable<Tchild> children, BindingsSaveContext context )
		=> children.Select( x => x.GetSaveData( context ) ).ToList();

	public override Drawable? CreateEditor () => new CompositeEditor<Tchild>( this );
	public override ActionBindingHandler? CreateHandler () => new CompositeHandler<Tchild>( this );

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

	protected Dictionary<K, object> CreateSaveDataAsDictionary ( IEnumerable<Tchild> children, BindingsSaveContext context )
		=> children.ToDictionary( GetKey, e => e.GetSaveData( context ) );

	protected void LoadChildren ( IDictionary<K, object> children, BindingsSaveContext context, Func<JsonElement, K, BindingsSaveContext, Tchild?> factory )
		=> LoadChildren<JsonElement>( children, context, factory );
	protected void LoadChildren<Tdata> ( IDictionary<K, object> children, BindingsSaveContext context, Func<Tdata, K, BindingsSaveContext, Tchild?> factory, JsonSerializerOptions? options = null ) {
		foreach ( (K key, object i) in children ) {
			if ( !context.DeserializeBindingData<Tdata>( (JsonElement)i, out var data, options ) )
				continue;

			var child = factory( data, key, context );
			if ( child != null ) {
				if ( !GetKey( child ).Equals( key ) ) {
					context.Warning( @"Mismatched binding identifiers", data );
				}
				if ( !Add( child ) ) {
					context.Error( @"Duplicate binding found", data );
				}
			}
			else {
				context.Error( @"Could not load a binding", data );
			}
		}
	}
}