using osu.Framework.Bindables;
using osu.XR.Settings.Sections;

namespace osu.XR.Editor {
	/// <summary>
	/// Given you want to edit a component that does not implement its own inspector setting subsection, you can use this class instead
	/// </summary>
	public abstract class SceneComponentEditor<T> : SettingsSection {
		public readonly Bindable<T> EditedElementBindable = new();
		public T EditedElement {
			get => EditedElementBindable.Value;
			set => EditedElementBindable.Value = value;
		}

		public SceneComponentEditor ( T editedElement = default ) {
			EditedElementBindable.ValueChanged += v => OnEditedElementChanged( v.OldValue, v.NewValue );
			EditedElementBindable.Value = editedElement;
		}

		protected abstract void OnEditedElementChanged ( T previous, T next );
	}
}
