using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Inspector {
	public class InspectorProperty<T> : CompositeDrawable {
		SpriteText label;
		OsuTextBox textBox;
		public InspectorProperty ( object source, string propertyName ) {
			Source = source;
			PropertyName = propertyName;

			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Margin = new MarginPadding { Left = 15 };

			AddInternal( new FillFlowContainer {
				Direction = FillDirection.Vertical,
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Children = new Drawable[] {
					label = new SpriteText {
						Font = OsuFont.Default,
						Height = 16,
						RelativeSizeAxes = Axes.X
					},
					textBox = new OsuTextBox {
						RelativeSizeAxes = Axes.X,
						Height = 30
					}
				}
			} );
		}

		public readonly object Source;

		private string propertyName;
		public string PropertyName {
			get => propertyName;
			set {
				propertyName = value;
				label.Text = value;

				Property = new ReflectedValue<T>( Source, value );
			}
		}

		ReflectedValue<T> Property;
		public Func<string, (bool, T)> Parser;

		private double timer = double.PositiveInfinity;
		private double updateInterval = 1000;
		protected override void Update () {
			base.Update();
			timer += Time.Elapsed;
			if ( timer > updateInterval ) return;

			timer = 0;
			textBox.Text = $"{Property.Value}";
		}
	}
}
