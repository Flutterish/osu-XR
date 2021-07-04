using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.XR.Drawables.Containers;
using System.Collections.Generic;
using System.Linq;

namespace osu.XR.Inspector.Reflections {
	public class ReflectionsInspectorComposite : ReflectionsInspectorComponent {
		public ReflectionsInspectorComposite ( ReflectionsInspector source ) : base( source ) {
			UpdateValue();
		}

		public override void UpdateValue () {
			ClearInternal( true );
			var sections = Source.TargetValue.GetDeclaredSections();
			LazyExpandableSection section;
			AddInternal( section = new LazyExpandableSection( v => {
				var addNames = sections.Count() > 1;

				void addSection ( IEnumerable<ReflectedValue<object>> values, Container container ) {
					foreach ( var i in values.OrderBy( v => v.DeclaredType.IsSimpleType() ? 1 : 2 ).ThenBy( v => v.DeclaredName ) ) {
						container.Add( new ReflectionsInspector( i ) );
					}
				}

				foreach ( var (type, values) in sections ) {
					if ( addNames ) {
						v.Add( new LazyExpandableSection( d => {
							addSection( values, d );
						} ) {
							Title = type.ReadableName()
						} );
					}
					else {
						addSection( values, v );
					}
				}
			} ) {
				Title = $"{Source.TargetName} [{Source.TargetType.ReadableName()}]: {StringifiedValue}",
				Margin = new MarginPadding { Horizontal = 5 }
			} );

			section.OnUpdate += _ => {
				section.Title = $"{Source.TargetName} [{Source.TargetType.ReadableName()}]: {StringifiedValue}";
			};
		}
	}
}
