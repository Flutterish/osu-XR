using System.Diagnostics.CodeAnalysis;

namespace osu.XR.Bindables;

/// <summary>
/// A bindable list which holds a reference to a bound target, allowing switching between targets and handling unbind/rebind.
/// </summary>
public class BindableListWithCurrent<T> : BindableList<T> {
	private BindableList<T>? currentBound;

	[AllowNull]
	public BindableList<T> Current {
		get => this;
		set {
			if ( currentBound != null ) 
				UnbindFrom( currentBound );
			if ( (currentBound = value) != null )
				BindTo( currentBound );
		}
	}

	public BindableListWithCurrent ( IEnumerable<T>? items = null )
		: base( items ) {
	}

	protected override BindableList<T> CreateInstance () => new BindableListWithCurrent<T>();
}
