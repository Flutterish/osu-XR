using osu.XR.Graphics.Containers;
using osu.XR.Input;

namespace osu.XR.Graphics.Bindings.Editors;

public partial class CompositeEditor<TChild> : FillFlowContainer where TChild : IActionBinding {
	public CompositeEditor ( CompositeActionBinding<TChild> source ) {
		AutoSizeAxes = Axes.Y;
		RelativeSizeAxes = Axes.X;
		Direction = FillDirection.Vertical;

		children.BindTo( source.Children );
		children.BindCollectionChanged( ( _, e ) => {
			if ( e.OldItems != null ) {
				foreach ( TChild child in e.OldItems ) {
					if ( childEditors.Remove( child, out var drawable ) )
						Remove( drawable, disposeImmediately: true );
				}
			}
			if ( e.NewItems != null ) {
				foreach ( TChild child in e.NewItems ) {
					CollapsibleSection section = null!;
					Add( section = new CollapsibleSection {
						Header = child.Name,
						Child = child.CreateEditor(),
						Expanded = child.ShouldBeSaved
					} );
					childEditors.Add( child, section );
				}
			}
		}, true );
	}

	BindableList<TChild> children = new();
	Dictionary<TChild, Drawable> childEditors = new();
}
