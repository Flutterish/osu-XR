using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.VR.ActionManifest {
	/// <summary>
	/// A controller binding. Contains what type of input it uses and what enum name it has.
	/// </summary>
	public class Action<T> : Action where T : Enum {
		public T Name { get; set; }
		public override string GetReadableName () => Name.ToString();
		public override ControllerComponent CreateComponent ( ulong handle ) {
			switch ( Type ) {
				case ActionType.Boolean:
					return new ControllerButton { Handle = handle, Name = Name };
				case ActionType.Vector1:
					return null;
				case ActionType.Vector2:
					return null;
				case ActionType.Vector3:
					return null;
				case ActionType.Vibration:
					return null;
				case ActionType.Skeleton:
					return null;
				case ActionType.Pose:
					return null;
				default:
					throw new InvalidOperationException( $"No controller component exists for {Type}" );
			}
		}
	}

	/// <summary>
	/// Base type of <see cref="Action{T}"/> as Enums are not covariant. Don't use this.
	/// </summary>
	public abstract class Action {
		public abstract string GetReadableName ();
		public ActionType Type { get; set; }
		public Requirement Requirement { get; set; }
		public Dictionary<string, string> Localizations = new();

		/// <summary>
		/// The full action name. This is usually set by <see cref="VrManager"/>
		/// </summary>
		public string FullPath { get; set; }

		public abstract ControllerComponent CreateComponent ( ulong handle );
	}
}
