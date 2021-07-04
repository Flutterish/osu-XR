using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.XR.Drawables.Containers {
	public class FilterableContainer : Container, IFilterable, IHasFilterableChildren {
		public IEnumerable<string> FilterTerms { get; set; } = Array.Empty<string>();
		public bool MatchingFilter {
			set {
				if ( !CanBeFiltered ) return;

				if ( value ) Show();
				else Hide();
			}
		}

		public override void Hide () {
			this.ScaleTo( new Vector2( 1, 0 ), 100, Easing.Out );
			this.Delay( 50 ).FadeOut( 50 );
		}

		public override void Show () {
			this.ScaleTo( new Vector2( 1, 1 ), 100, Easing.Out );
			this.FadeIn( 50 );
		}

		public bool FilteringActive { get; set; }
		public bool CanBeFiltered = true;

		public IEnumerable<IFilterable> FilterableChildren
			=> CanBeFiltered ? Children.OfType<IFilterable>() : Array.Empty<IFilterable>();
	}
}
