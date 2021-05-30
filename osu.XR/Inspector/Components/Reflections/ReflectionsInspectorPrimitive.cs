using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Inspector.Components.Reflections {
	public class ReflectionsInspectorPrimitive : ReflectionsInspectorComponent {
		OsuTextFlowContainer text;
		Circle background;

		public ReflectionsInspectorPrimitive ( ReflectionsInspector source ) : base( source ) {
			AddInternal( background = new Circle {
				RelativeSizeAxes = Axes.Y,
				AlwaysPresent = true,
				Colour = Color4.Transparent,
				Margin = new MarginPadding { Left = 10 }
			} );

			AddInternal( text = new OsuTextFlowContainer {
				Margin = new MarginPadding { Horizontal = 15 },
				AutoSizeAxes = Axes.Y
			} );

			UpdateValue();
		}

		protected override void Update () {
			base.Update();

			text.Width = DrawWidth - 30;
		}

		public override void UpdateValue () {
			text.Clear( true );
			text.AddText( $"{Source.TargetName} ", v => v.Colour = Color4.GreenYellow );
			text.AddText( $"[{Source.TargetType.ReadableName()}]: ", v => v.Colour = Color4.LimeGreen );
			text.AddText( StringifiedValue, v => v.Colour = Source.TargetValue is Exception ? Color4.Red : Color4.White );

			background.FlashColour( Color4.HotPink.Opacity( 0.6f ), 500, Easing.In );
			background.Width = DrawWidth - 20;
		}
	}
}
