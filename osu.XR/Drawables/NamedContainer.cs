using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.XR.Components.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Drawables {
	public class NamedContainer : FillFlowContainer, IHasName {
		public NamedContainer () {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Direction = FillDirection.Vertical;

			DisplayName = GetType().ReadableName();
		}

		public string DisplayName { get; set; }
	}
}
