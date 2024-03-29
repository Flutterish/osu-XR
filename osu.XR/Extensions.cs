﻿using osu.Framework.Graphics;
using osu.XR.Inspector;
using System;
#nullable enable

namespace osu.XR {
	public static class Extensions {
		public static IInspectable? GetClosestInspectable ( this Drawable self )
			=> ( self.IsInspectable() && self is IInspectable inspectable ) ? inspectable : self.Parent?.GetClosestInspectable();

		public static string GetInspectorName ( this Drawable drawable )
			=> string.IsNullOrWhiteSpace( drawable.Name ) ? drawable.GetType().Name : drawable.Name;

		public static Drawable? GetValidInspectable ( this Drawable self )
			=> self.IsInspectable() ? self : self.Parent?.GetValidInspectable();

		public static bool DoParentsAllowInspection ( this Drawable self )
			=> self.Parent is null ? true : ( self.Parent is IChildrenNotInspectable ? false : self.Parent.DoParentsAllowInspection() );

		public static bool IsInspectable ( this Drawable self )
			=> self is not ISelfNotInspectable && self.DoParentsAllowInspection();

		static readonly string[] SiUnits = new[] { "B", "kiB", "MiB", "GiB", "TiB" };
		public static string HumanizeSiBytes ( this int bytes )
			=> HumanizeSiBytes( (long)bytes );
		public static string HumanizeSiBytes ( this long bytes ) {
			if ( bytes == 0 ) return "0B";
			if ( bytes < 0 ) return '-' + HumanizeSiBytes( -bytes );

			int order = (int)Math.Floor( Math.Log( (double)bytes, 1024 ) );
			order = Math.Clamp( order, 0, SiUnits.Length - 1 );
			double display = bytes / Math.Pow( 1024, order );

			return $"{display:0.##}{SiUnits[order]}";
		}

		public static float CopySign ( this float value, float sign )
			=> Math.Abs( value ) * Math.Sign( sign );

		public static string Pluralize ( this int number, string name, string? plural = null ) {
			if ( number == 1 ) {
				return $"{number} {name}";
			}
			else {
				if ( plural == null ) {
					if ( name.ToLower().EndsWith( "sh" ) ) {
						plural = name + "es";
					}
					else {
						plural = name + "s";
					}
				}
				return $"{number} {plural}";
			}
		}
	}
}
