using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.VR.ActionManifest {
	public enum Requirement {
		/// <summary>
		/// The user will be warned if this is not bound
		/// </summary>
		Suggested,
		/// <summary>
		/// The bindings cannot be saved witohut this bound
		/// </summary>
		Mandatory,
		/// <summary>
		/// No warning are produced if this is not bound
		/// </summary>
		Optional
	}
}
