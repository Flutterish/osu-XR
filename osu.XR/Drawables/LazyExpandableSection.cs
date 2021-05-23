using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Drawables {
	public class LazyExpandableSection : ExpandableSection {
		Action<LazyExpandableSection> onFirstExpand;
		public LazyExpandableSection ( Action<LazyExpandableSection> onFirstExpand ) {
			this.onFirstExpand = onFirstExpand;

			IsExpanded.ValueChanged += onExpanded;
		}

		private void onExpanded ( Framework.Bindables.ValueChangedEvent<bool> obj ) {
			if ( !obj.NewValue ) return;

			IsExpanded.ValueChanged -= onExpanded;
			onFirstExpand?.Invoke( this );
		}
	}
}
