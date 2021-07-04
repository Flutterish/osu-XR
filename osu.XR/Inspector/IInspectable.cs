﻿using osu.Framework.Graphics;
using System.Collections.Generic;

namespace osu.XR.Inspector {
	/// <summary>
	/// An object whose properties can be inspected.
	/// </summary>
	public interface IInspectable { }

	/// <summary>
	/// An object whose properties can be inspected and that has custom inspector subsections.
	/// </summary>
	public interface IConfigurableInspectable {
		IEnumerable<Drawable> CreateInspectorSubsections ();
		bool AreSettingsPersistent { get; }
	}

	/// <summary>
	/// An object that should never be seen in the inspector. Its children will still be visible.
	/// </summary>
	public interface ISelfNotInspectable { }
	/// <summary>
	/// An object whose children should never be seen in the inspector.
	/// </summary>
	public interface IChildrenNotInspectable { }
	/// <summary>
	/// An object fully invisible to the inspector.
	/// </summary>
	public interface INotInspectable : ISelfNotInspectable, IChildrenNotInspectable { }

	// TODO IHasInspectorVisuals will be able to render things when selected by the inspector.
	public interface IHasInspectorVisuals { }

	/// <summary>
	/// This object is experimental.
	/// </summary>
	public interface IExperimental { }
}
