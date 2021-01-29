using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Maths {
	public static class Curves {
		/// <summary>
		/// https://www.desmos.com/calculator/jns5zfdvkm
		/// </summary>
		public static double Logistic ( double from, double to, double t, double @base = Math.E )
			=> ( to - from ) / ( 1 + Math.Pow( @base, t ) ) + from;
	}
}
