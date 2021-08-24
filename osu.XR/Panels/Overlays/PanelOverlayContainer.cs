using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System.Linq;

namespace osu.XR.Panels.Overlays {
	[Cached]
	public class PanelOverlayContainer : Container {
		private PanelOverlay activeOverlay;

		private Container overlays;
		private Container content;
		protected override Container<Drawable> Content => content;

		public PanelOverlayContainer () {
			AddInternal( content = new Container {
				RelativeSizeAxes = Axes.Both
			} );
			AddInternal( overlays = new Container {
				RelativeSizeAxes = Axes.Both
			} );
			Masking = true;
		}

		public T RequestOverlay<T> () where T : PanelOverlay, new() {
			activeOverlay?.Hide();

			var find = overlays.OfType<T>().FirstOrDefault();
			if ( find is null ) {
				find = new T();
				overlays.Add( find );
			}
			activeOverlay = find;
			activeOverlay.Show();
			return find;
		}
	}
}
