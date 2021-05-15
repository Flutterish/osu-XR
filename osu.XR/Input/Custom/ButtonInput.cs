using osu.XR.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Input.Custom {
	public class ButtonInput : CustomInput {
		public Hand Hand = Hand.Auto;
		public override string Name => $"{Hand} Buttons";
	}
}
