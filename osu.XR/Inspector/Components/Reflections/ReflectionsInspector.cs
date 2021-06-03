using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.XR.Components.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Inspector.Components.Reflections {
	public class ReflectionsInspector : CompositeDrawable, IHasName {
		public Func<object> ValueGetter { get; private set; }
		public object TargetValue {
			get {
				try {
					return ValueGetter();
				}
				catch ( Exception e ) {
					ShouldUpdate = false;
					return e;
				}
			}
		}
		public Action<object> ValueSetter { get; private set; }
		public Func<object, Type> TypeGetter { get; private set; }
		public Type TargetType => TypeGetter( TargetValue );
		public Func<object, string> NameGetter;
		public string TargetName => NameGetter( TargetValue );
		public bool IsValueEditable { get; private set; }

		public ReflectionsInspector ( object value = null, string name = null ) {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;

			IsValueEditable = false;
			SetValue( value, name );
		}
		public ReflectionsInspector ( ReflectedValue<object> reflectedValue ) {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;

			IsValueEditable = !reflectedValue.IsReadonly;
			SetSource( reflectedValue );
		}

		public void SetValue ( object value, string name = null ) {
			ValueGetter = () => value;
			ValueSetter = null;
			TypeGetter = v => v?.GetType() ?? typeof( object );
			NameGetter = _ => name ?? "Value";
			ShouldUpdate = true;
			updateValue();
		}

		public void SetSource ( ReflectedValue<object> reflectedValue ) {
			ValueGetter = reflectedValue.Getter;
			ValueSetter = reflectedValue.Setter;
			TypeGetter = _ => reflectedValue.DeclaredType;
			NameGetter = _ => reflectedValue.DeclaredName;
			ShouldUpdate = true;
			updateValue();
		}

		public double UpdateInterval = 100;
		public bool ShouldUpdate = true;
		double time;

		protected override void Update () {
			base.Update();
			if ( ShouldUpdate ) {
				time += Time.Elapsed;
				if ( time >= UpdateInterval ) {
					time = 0;
					updateValue();
				}
			}
		}

		ReflectionsInspectorComponent current {
			get => ( InternalChildren.Any() ? InternalChild : null ) as ReflectionsInspectorComponent;
			set => InternalChild = value;
		}

		object previousValue = null;
		void updateValue () {
			var value = TargetValue;
			if ( current is not null && ( value?.Equals( previousValue ) == true || previousValue == value ) ) return;
			previousValue = value;

			if ( value is null or Exception ) {
				if ( current is not ReflectionsInspectorPrimitive )
					current = new ReflectionsInspectorPrimitive( this );
				else current.UpdateValue();
				return;
			}

			var type = value.GetType();

			if ( type.IsSimpleType() || ReflectionsInspectorPrimitive.CanEdit( type ) ) {
				if ( current is not ReflectionsInspectorPrimitive )
					current = new ReflectionsInspectorPrimitive( this );
				else current.UpdateValue();
				return;
			}

			if ( current is not ReflectionsInspectorComposite )
				current = new ReflectionsInspectorComposite( this );
			else current.UpdateValue();
			return;
		}

		public string DisplayName => "Reflections";
	}
}
