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

			InspectedObject.BindValueChanged( v => {
				inspect( v.NewValue );
			}, true );
		}

		protected ExpandableSection CompositeSection;
		protected OsuTextFlowContainer PrimitiveSection;

		void inspect ( object obj ) {
			Clear( true );

			if ( obj is null ) {
				makePrimitive();
				return;
			}

			var type = obj.GetType();

			if ( isSimpleType( type ) ) { // primitive, can be just stringified
				makePrimitive();
				// TODO editors for simple types
			}
			else { // composite, needs to be decomposed
				if ( type.IsValueType ) {
					makePrimitive();
					return;
				}

				var sections = getDeclaredSections( obj );
				var count = sections.Count();
				if ( count == 0 ) {
					makePrimitive();
				}
				else {
					makeComposite( sections, count );
				}
			}
		}

		static readonly Type[] simpleTypes = new Type[] {
			typeof(string),
			typeof(decimal),
			typeof(DateTime),
			typeof(DateTimeOffset),
			typeof(TimeSpan),
			typeof(Guid)
		};
		bool isSimpleType ( Type type ) {
			return type.IsPrimitive
				|| type.IsEnum
				|| simpleTypes.Contains( type )
				|| ( type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Nullable<> ) && isSimpleType( type.GetGenericArguments()[ 0 ] ) );
		}

		IEnumerable<ReflectedValue<object>> getDeclaredValues ( object obj, Type type ) {
			foreach ( var i in type.GetProperties( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly ) ) {
				if ( i.GetGetMethod() != null ) yield return new ReflectedValue<object>( obj, i );
			}

			foreach ( var i in type.GetFields( BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly ) ) {
				yield return new ReflectedValue<object>( obj, i );
			}
		}

		IEnumerable<(Type type, IEnumerable<ReflectedValue<object>> values)> getDeclaredSections ( object obj ) {
			Type type = obj.GetType();

			while ( type != null && type != typeof( object ) ) {
				var declared = getDeclaredValues( obj, type );
				if ( declared.Any() ) yield return (type, declared);

				type = type.BaseType;
			}
		}

		void addSection ( string name, Action<LazyExpandableSection> populate ) {
			CompositeSection.Add( new LazyExpandableSection( populate ) {
				Title = name,
				Margin = new MarginPadding { Horizontal = 0 }
			} );
		}

		void addProperty ( Container<Drawable> container, ReflectedValue<object> value ) {
			container.Add( new ReflectedValueInspector( value ) );
		}

		void makePrimitive () {

			Clear( true );

			Add( PrimitiveSection = new OsuTextFlowContainer {
				Margin = new MarginPadding { Left = 15 },
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Width = 0.95f,
				//Text = $"[{type.ReadableName()}] {title}: {value}"
			} );
		}

		void makeComposite ( IEnumerable<(Type type, IEnumerable<ReflectedValue<object>> values)> sections, int count ) {
			Add( CompositeSection = new LazyExpandableSection( d => {
				if ( count == 1 ) {
					foreach ( var i in sections.Single().values ) {
						addProperty( d, i );
					}
				}
				else {
					foreach ( var (t, values) in sections ) {
						addSection( t.Name, d => {
							foreach ( var i in values ) {
								addProperty( d, i );
							}
						} );
					}
				}
			} ) {
				Title = $"[{InspectedObject.Value?.GetType().ReadableName()}] {title}: {InspectedObject.Value}",
				Margin = new MarginPadding { Horizontal = 5 }
			} );
		}

		protected virtual string GetDisplayType ()
			=> $"{InspectedObject.Value?.GetType()}";

		protected virtual string GetDisplayValue ()
			=> $"{InspectedObject.Value}";

		public string DisplayName => "Reflections";
	}

	public class ReflectedValueInspector : ValueInspector {
		ReflectedValue<object> value;
		public ReflectedValueInspector ( ReflectedValue<object> value ) {
			this.value = value;
			Title = value.DeclaredName;
			updateValue();
		}

		public double UpdateInterval = 50;
		double timer;

		protected override void Update () {
			base.Update();
			if ( !erroredOut ) timer += Time.Elapsed;
			if ( timer >= UpdateInterval ) {
				timer = 0;
				updateValue();
			}
		}

		bool erroredOut = false;
		void updateValue () {
			try {
				InspectedObject.Value = value.Value;
				if ( CompositeSection != null ) CompositeSection.Title = $"[{value.DeclaredType.ReadableName()}] {Title}: {InspectedObject.Value}";
			}
			catch ( Exception e ) {
				InspectedObject.Value = $"Error: {e.Message}";
				erroredOut = true;
			}
		}
	}
}
