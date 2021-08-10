using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.XR.Drawables.Containers;
using osu.XR.Settings.Sections;
using osuTK;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace osu.XR.Inspector {
	/// <summary>
	/// An object whose properties can be inspected.
	/// </summary>
	public interface IInspectable { }

	/// <summary>
	/// An object whose properties can be inspected and that has custom inspector subsections.
	/// </summary>
	public interface IConfigurableInspectable {
		Drawable CreateInspectorSubsection ();
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
		public static Drawable? CreateWarnings ( this IConfigurableInspectable self ) {
			if ( !self.AreSettingsPersistent ) {
				return new FormatedTextContainer( () => new() { Size = 20 } ) {
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y,
					Text = "||Warning:|| Some or all of these settings are **not** persistent\n~~They will not be saved after you close the game~~",
					TextAnchor = Anchor.Centre
				};
			}
			else return null;
		}
	}

	public class InspectorSubsectionWithCurrent : SettingsSectionContainer {
		BindableWithCurrent<Drawable> current = new();
		public readonly BindableBool ShowWarningsBindable = new( false );
		public Bindable<Drawable> Current {
			get => current;
			set => current.Current = value;
		}
		public bool ShowWarnings {
			get => ShowWarningsBindable.Value;
			set => ShowWarningsBindable.Value = value;
		}

		private Drawable? subsection;
		private Drawable? warning;
		public InspectorSubsectionWithCurrent () {

		}

		protected override void LoadComplete () {
			base.LoadComplete();

			current.BindValueChanged( v => {
				Remove( subsection );
				Remove( warning );
				subsection = null;
				this.warning = null;
				if ( v.NewValue is IConfigurableInspectable config ) {
					if ( config.CreateWarnings() is Drawable warning ) {
						Add( this.warning = warning );
						if ( !ShowWarnings ) warning.Scale = Vector2.UnitX;
					}
					Add( subsection = config.CreateInspectorSubsection() );
				}
			} );

			ShowWarningsBindable.BindValueChanged( v => {
				if ( warning is not null ) warning.ScaleTo( v.NewValue ? Vector2.One : Vector2.UnitX, 100 );
			} );
		}
	}
}
