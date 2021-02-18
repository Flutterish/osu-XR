﻿using osu.XR.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components {
	public class Collider : MeshedXrObject, IHasCollider {
		public bool IsColliderEnabled { get; set; } = true;
	}
}