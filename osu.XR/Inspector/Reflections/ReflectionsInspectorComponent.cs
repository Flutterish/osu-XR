using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Reflection;

namespace osu.XR.Inspector.Reflections {
	public abstract class ReflectionsInspectorComponent : CompositeDrawable {
		protected readonly ReflectionsInspector Source;
		public ReflectionsInspectorComponent ( ReflectionsInspector source ) {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Source = source;
		}

		public abstract void UpdateValue ();

		protected string StringifiedValue {
			get {
				var value = Source.TargetValue;

				if ( value is Exception e ) {
					if ( e is TargetInvocationException )
						return e.InnerException.Message;
					else
						return e.Message;
				}
				else if ( value is null )
					return "Null";
				else
					return value.ToString();
			}
		}
	}
}
