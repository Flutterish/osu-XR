using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.XR.Drawables.Containers;
using System.Collections.Generic;
using System.Linq;

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


	public static class IConfigurableInspectableExtensions {
		public static IEnumerable<Drawable> CreateInspectorSubsectionsWithWarning ( this IConfigurableInspectable self ) {
			if ( self.AreSettingsPersistent )
				return self.CreateInspectorSubsections();
			else return self.CreateInspectorSubsections().Prepend( new FormatedTextContainer( () => new() { Size = 20 } ) {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Text = "||Warning:|| These settings are **not** persistent\n~~They will not be saved after you close the game~~",
				TextAnchor = Anchor.Centre
			} );
		}
	}
}
