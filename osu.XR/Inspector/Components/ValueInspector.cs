using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.XR.Components.Groups;
using osu.XR.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Inspector.Components {
	public class ValueInspector : FillFlowContainer, IHasName {
		public readonly Bindable<object> InspectedObject = new();
		public object Inspected {
			get => InspectedObject.Value;
			set => InspectedObject.Value = value;
		}

		string title;
		public string Title {
			set {
				title = value;
				Schedule( () => inspect( InspectedObject.Value ) );
			}
			get => title;
		}

		public ValueInspector () {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Direction = FillDirection.Vertical;

			CompositeSection = new() { Margin = new MarginPadding { Horizontal = 5 } };

			InspectedObject.BindValueChanged( v => {
				inspect( v.NewValue );
			}, true );
		}

		protected ExpandableSection CompositeSection;

		void inspect ( object obj ) {
			RemoveAll( x => x == CompositeSection );
			Clear( true );
			CompositeSection.Clear( true );

			if ( obj is null ) {
				Add( new OsuTextFlowContainer {
					Margin = new MarginPadding { Left = 15 },
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y,
					Width = 0.95f,
					Text = $"{title}: Null"
				} );
				return;
			}

			var type = obj.GetType();

			if ( type.IsValueType ) { // primitive, can be just stringified
				Add( new OsuTextFlowContainer {
					Margin = new MarginPadding { Left = 15 },
					RelativeSizeAxes = Axes.X,
					AutoSizeAxes = Axes.Y,
					Width = 0.95f,
					Text = $"{title}: {obj}"
				} );
			}
			else { // composite, needs to be decomposed
				Add( CompositeSection );
				CompositeSection.Title = $"{title} ({type.ReadableName()}): {obj}";
				inspect( obj, type );
			}

			IEnumerable<ReflectedValue<object>> refelct ( object obj, Type type ) {
				foreach ( var i in type.GetProperties( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly ) ) {
					if ( i.GetGetMethod() != null ) yield return new ReflectedValue<object>( obj, i );
				}

				foreach ( var i in type.GetFields( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly ) ) {
					yield return new ReflectedValue<object>( obj, i );
				}
			}

			void inspect ( object obj, Type type ) {
				var values = refelct( obj, type );
				if ( values.Any() ) {
					addSection( type.Name, d => {
						foreach ( var i in values ) {
							addProperty( d, i );
						}
					} );
				}

				if ( type.BaseType != null && type.BaseType != typeof( object ) ) {
					inspect( obj, type.BaseType );
				}
			}
		}

		void addSection ( string name, Action<LazyExpandableSection> populate ) {
			CompositeSection.Add( new LazyExpandableSection( populate ) {
				Title = name,
				Margin = new MarginPadding { Horizontal = 5 }
			} );
		}

		void addProperty ( Container<Drawable> container, ReflectedValue<object> value ) {
			container.Add( new ReflectedValueInspector( value ) );
		}

		public string DisplayName => "Reflections";
	}

	public class ReflectedValueInspector : ValueInspector {
		ReflectedValue<object> value;
		public ReflectedValueInspector ( ReflectedValue<object> value ) {
			this.value = value;
			Title = value.Name;
			updateValue();
		}

		public double UpdateInterval = 50;
		double timer;

		protected override void Update () {
			base.Update();
			timer += Time.Elapsed;
			if ( timer >= UpdateInterval ) {
				timer = 0;
				updateValue();
			}
		}

		void updateValue () {
			try {
				InspectedObject.Value = value.Value;
				var type = InspectedObject.Value?.GetType();
				if ( type != null )
					CompositeSection.Title = $"{Title} ({type.ReadableName()}): {InspectedObject.Value}";
			}
			catch ( Exception e ) { InspectedObject.Value = $"Error: {e.Message}"; }
		}
	}
}
