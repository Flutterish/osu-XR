using osu.Framework.Graphics;
using osuTK.Graphics;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace osu.XR.Graphics {
	public static class NeonColors {
		public static Color4 NextRandom ( Random random ) {
			return ParseHex( hexCodes[ random.Next( 0, hexCodes.Count ) ] );
		}

		private static Colour4 ParseHex ( string hex ) {
			if ( Colour4.TryParseHex( hex, out var color ) ) return color;
			else throw new InvalidOperationException( $"Invalid hex color `{hex}`" );
		}

		private static List<string> hexCodes = new() {
			"#dfff11",
			"#66ff00",
			"#ff08e8",
			"#fe01b1",
			"#be03fd",
			"#ff000d",
			"#ffcf09",
			"#fc0e34",
			"#01f9c6",
			"#ff003f",
			"#0ff0fc",
			"#fc74fd",
			"#21fc0d",
			"#6600ff",
			"#ccff00",
			"#ff3503",
			"#ff0490",
			"#bf00ff",
			"#e60000",
			"#55ffff",
			"#8f00f1",
			"#fffc00",
			"#08ff08",
			"#ffcf00",
			"#fe1493",
			"#ff5555",
			"#fc8427",
			"#00fdff",
			"#ccff02",
			"#ff11ff",
			"#04d9ff",
			"#ff9933",
			"#fe4164",
			"#39ff14",
			"#fe019a",
			"#bc13fe",
			"#ff073a",
			"#cfff04",
			"#ff0055"
		};

		public static readonly Color4 BrightChartreuse = ParseHex( "#dfff11" );
		public static readonly Color4 BrightGreen = ParseHex( "#66ff00" );
		public static readonly Color4 BrightMagenta = ParseHex( "#ff08e8" );
		public static readonly Color4 BrightPink = ParseHex( "#fe01b1" );
		public static readonly Color4 BrightPurple = ParseHex( "#be03fd" );
		public static readonly Color4 BrightRed = ParseHex( "#ff000d" );
		public static readonly Color4 BrightSaffron = ParseHex( "#ffcf09" );
		public static readonly Color4 BrightScarlet = ParseHex( "#fc0e34" );
		public static readonly Color4 BrightTeal = ParseHex( "#01f9c6" );
		public static readonly Color4 ElectricCrimson = ParseHex( "#ff003f" );
		public static readonly Color4 ElectricCyan = ParseHex( "#0ff0fc" );
		public static readonly Color4 ElectricFlamingo = ParseHex( "#fc74fd" );
		public static readonly Color4 ElectricGreen = ParseHex( "#21fc0d" );
		public static readonly Color4 ElectricIndigo = ParseHex( "#6600ff" );
		public static readonly Color4 ElectricLime = ParseHex( "#ccff00" );
		public static readonly Color4 ElectricOrange = ParseHex( "#ff3503" );
		public static readonly Color4 ElectricPink = ParseHex( "#ff0490" );
		public static readonly Color4 ElectricPurple = ParseHex( "#bf00ff" );
		public static readonly Color4 ElectricRed = ParseHex( "#e60000" );
		public static readonly Color4 ElectricSheep = ParseHex( "#55ffff" );
		public static readonly Color4 ElectricViolet = ParseHex( "#8f00f1" );
		public static readonly Color4 ElectricYellow = ParseHex( "#fffc00" );
		public static readonly Color4 FluorescentGreen = ParseHex( "#08ff08" );
		public static readonly Color4 FluorescentOrange = ParseHex( "#ffcf00" );
		public static readonly Color4 FluorescentPink = ParseHex( "#fe1493" );
		public static readonly Color4 FluorescentRed = ParseHex( "#ff5555" );
		public static readonly Color4 FluorescentRedOrange = ParseHex( "#fc8427" );
		public static readonly Color4 FluorescentTurquoise = ParseHex( "#00fdff" );
		public static readonly Color4 FluorescentYellow = ParseHex( "#ccff02" );
		public static readonly Color4 LightNeonPink = ParseHex( "#ff11ff" );
		public static readonly Color4 NeonBlue = ParseHex( "#04d9ff" );
		public static readonly Color4 NeonCarrot = ParseHex( "#ff9933" );
		public static readonly Color4 NeonFuchsia = ParseHex( "#fe4164" );
		public static readonly Color4 NeonGreen = ParseHex( "#39ff14" );
		public static readonly Color4 NeonPink = ParseHex( "#fe019a" );
		public static readonly Color4 NeonPurple = ParseHex( "#bc13fe" );
		public static readonly Color4 NeonRed = ParseHex( "#ff073a" );
		public static readonly Color4 NeonYellow = ParseHex( "#cfff04" );
		public static readonly Color4 PinkishRedNeon = ParseHex( "#ff0055" );
	}
}
