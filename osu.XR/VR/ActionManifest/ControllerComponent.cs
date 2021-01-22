using osu.Framework.Bindables;
using osu.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Valve.VR;

namespace osu.XR.VR.ActionManifest {
	/// <summary>
	/// Represents an input or output of a controller
	/// </summary>
	public abstract class ControllerComponent {
		/// <summary>
		/// A unique name for this component, usually an enum
		/// </summary>
		public object Name { get; init; }
		public ulong Handle { get; init; }

		public abstract void Update ();
	}

	public abstract class ControllerComponent<T> : ControllerComponent {
		public readonly Bindable<T> ValueBindable = new();
		public T Value {
			get => ValueBindable.Value;
			protected set => ValueBindable.Value = value;
		}
	}

	// TODO leftright actions can be restricted to a given side
	public class ControllerButton : ControllerComponent<bool> {
		public override void Update () {
			InputDigitalActionData_t data = default;
			var error = OpenVR.Input.GetDigitalActionData( Handle, ref data, (uint)Marshal.SizeOf<InputDigitalActionData_t>(), OpenVR.k_ulInvalidActionHandle );
			if ( error != EVRInputError.None ) {
				Logger.Error( null, $"Cannot read input: {error}" );
				return;
			}

			Value = data.bState;
		}
	}
}
