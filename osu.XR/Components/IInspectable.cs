using osu.Framework.XR.Physics;
using osu.Game.Overlays.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Components {
	/// <summary>
	/// An object whose properties can be inspected
	/// </summary>
	public interface IInspectable : IHasCollider {
		IEnumerable<SettingsSubsection> CreateInspectorSubsections ();
	}
}
