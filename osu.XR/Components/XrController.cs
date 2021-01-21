using osu.XR.VR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components {
	public class XrController : MeshedXrObject {
		public readonly Controller Controller;
		public XrController ( Controller controller ) {
			Controller = controller;
			Mesh = controller.Mesh;
		}

		protected override void Update () {
			base.Update();
			Position = Controller.Position;
			Rotation = Controller.Rotation;
		}
	}
}
