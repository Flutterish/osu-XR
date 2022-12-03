using osu.Framework.XR.Allocation;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Containers;
using osu.Framework.XR.Graphics.Panels;

namespace osu.XR.Graphics.Panels;

public abstract partial class PanelStack<T> : Container3D<T> where T : Panel {
	public virtual IEnumerable<Drawable3D> FlowingChildren => AliveInternalChildren.OrderBy( d => layoutChildren[d] );

	Dictionary<Drawable3D, float> layoutChildren = new();
	bool isLayoutValid = false;
	float frontIndex = -1;

	protected override void AddInternal ( Drawable3D child ) {
		layoutChildren.Add( child, 0 );
		base.AddInternal( child );
		isLayoutValid = false;
	}

	protected override void RemoveInternal ( Drawable3D child, bool disposeImmediately ) {
		layoutChildren.Remove( child );
		base.RemoveInternal( child, disposeImmediately );
		isLayoutValid = false;
	}

	public void FocusPanel ( T panel ) {
		if ( layoutChildren.ContainsKey( panel ) ) {
			layoutChildren[panel] = frontIndex--;
			isLayoutValid = false;
		}
	}

	bool firstLayoutPerfromed = false;
	protected override void Update () {
		base.Update();
		if ( !isLayoutValid ) {
			using ( MemoryPool<T>.Shared.Rent( InternalChildren.Count, out var span ) ) {
				int index = 0;
				foreach ( var i in FlowingChildren.OfType<T>() ) {
					span[index++] = i;
				}
				PerformLayout( span.Slice( 0, index ) );
			}
			isLayoutValid = true;
			if ( !firstLayoutPerfromed ) {
				firstLayoutPerfromed = true;
				FinishTransforms( true );
			}
		}
	}

	protected abstract void PerformLayout ( ReadOnlySpan<T> children );
}
