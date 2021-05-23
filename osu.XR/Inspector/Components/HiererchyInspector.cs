using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.XR.Components.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Inspector.Components {
	public class HiererchyInspector : FillFlowContainer, IHasName {
		public HiererchyInspector () {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Direction = FillDirection.Vertical;


		}

		public string DisplayName => "Hiererchy";
	}
}
