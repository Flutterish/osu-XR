using NuGet.Packaging;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace osu.XR.Drawables.Containers {
	/// <summary>
	/// A drawable that supports advanced inline text formating.
	/// </summary>
	public abstract class FormatedText : FillFlowContainer {
		protected readonly List<Func<TextSpanFormater>> Formaters = new();

		private readonly BindableWithCurrent<string> current = new();
		public Bindable<string> Current {
			get => current;
			set => current.Current = value;
		}
		public string Text {
			get => Current.Value;
			set => Current.Value = value;
		}

		public FormatedText () {
			Formaters.Add( () => new QuickTextSpanReplaceFormater( @"\n", s => {
				return new NewLine().Yield();
			} ) );
			Current.ValueChanged += v => format( v.NewValue );
		}

		protected override bool ForceNewRow ( Drawable child ) {
			return child is NewLine; // TODO handle newlines and default font creation parameters better
		}

		public override void Add ( Drawable drawable ) {
			drawable.Origin = drawable.Anchor = textAnchor;
			base.Add( drawable );
		}

		private Anchor textAnchor = Anchor.TopLeft;
		public Anchor TextAnchor {
			get => textAnchor;
			set {
				textAnchor = value;
				foreach ( var i in Children )
					i.Anchor = i.Origin = value;
			}
		}

		private void format ( string text ) {
			Clear();

			var begins = new SortedList<Match, (Func<TextSpanFormater> func, TextSpanFormater inst)>( Comparer<Match>.Create( (a,b) => {
				var d = a.Index - b.Index;
				return d != 0 ? d : b.Length - a.Length;
			} ) );
			var ends = new SortedList<Match, (Func<TextSpanFormater> func, TextSpanFormater inst)>( Comparer<Match>.Create( ( a, b ) => {
				var d = a.Index - b.Index;
				return d != 0 ? d : b.Length - a.Length;
			} ) );
			var active = new List<TextSpanFormater>();

			begins.AddRange( Formaters.Select( x => ( x, x() ) ).Select( x => new KeyValuePair<Match, (Func<TextSpanFormater> func, TextSpanFormater inst)>( x.Item2.Begin.Match( text ), x ) ).Where( x => x.Key.Success ) );

			int index = 0;
			void next ( int length ) {
				if ( length <= 0 ) return;

				string substr = text.Substring( index, length );
				index += length;

				if ( active.Any() ) {
					var drawables = active[ 0 ].Format( substr );
					foreach ( var i in active.Skip( 1 ) ) {
						drawables = drawables.SelectMany( x => i.Format( x ) );
					}
					AddRange( drawables );
				}
				else {
					var t = new TextFlowContainer();
					var part = t.AddText( substr );
					part.RecreateDrawablesFor( t );
					var c = part.Drawables.ToArray();
					t.Clear( disposeChildren: false );
					AddRange( c );
				}
			}

			void removePassed () {
				foreach ( var i in begins.TakeWhile( x => x.Key.Index < index ).ToArray() ) {
					begins.Remove( i.Key );
					var match = i.Value.inst.Begin.Match( text, index );
					if ( match.Success )
						begins.Add( match, i.Value );
				}
				foreach ( var i in ends.TakeWhile( x => x.Key.Index < index ).ToArray() ) {
					ends.Remove( i.Key );
					var match = i.Value.inst.End.Match( text, index );
					if ( match.Success )
						ends.Add( match, i.Value );
				}
			}

			while ( index < text.Length ) {
				Match match = null;
				(Func<TextSpanFormater> func, TextSpanFormater inst) format;

				bool? isStart = null;

				if ( begins.Any() ) {
					(match, format) = begins.First();
					isStart = true;

					if ( ends.Any() ) {
						var (ematch, eformat) = ends.First();
						if ( ematch.Index <= match.Index ) {
							(match, format) = (ematch, eformat);
							isStart = false;
						}
					}
				}
				else if ( ends.Any() ) {
					(match, format) = ends.First();
					isStart = false;
				}

				if ( isStart == true ) {
					next( match.Index - index );

					var firsts = begins.TakeWhile( x => x.Key.Index == match.Index );
					var specificity = firsts.Max( x => x.Key.Length );
					var first = firsts.Single( x => x.Key.Length == specificity );

					index += specificity;
					if ( first.Value.inst.IsContentExclusive ) {
						var startMatch = match;
						match = first.Value.inst.End.Match( text, index );
						string t;
						if ( match.Success ) {
							t = text.Substring( index, match.Index - index );
							index = match.Index + match.Length;
						}
						else {
							t = text.Substring( index );
							index = text.Length;
						}

						var drawables = first.Value.inst.Format( startMatch, t, match );
						foreach ( var i in active ) {
							drawables = drawables.SelectMany( x => i.Format( x ) );
						}
						AddRange( drawables );
					}
					else {
						var inst = first.Value.func();
						active.Add( inst );

						match = inst.End.Match( text, index );
						if ( match.Success )
							ends.Add( match, (first.Value.func, inst) );
					}

					removePassed();
				}
				else if ( isStart == false ) {
					next( match.Index - index );

					var firsts = ends.TakeWhile( x => x.Key.Index == match.Index );
					var specificity = firsts.Max( x => x.Key.Length );
					var first = firsts.Single( x => x.Key.Length == specificity );

					ends.Remove( first.Key );
					active.Remove( first.Value.inst );

					index += specificity;
					removePassed();
				}
				else {
					next( text.Length - index );
				}
			}
		}
	}

	public abstract class TextSpanFormater {
		public abstract Regex Begin { get; }
		public virtual Regex End => Begin;

		/// <summary>
		/// Whether other rules can not be matched inside of this block.
		/// </summary>
		public virtual bool IsContentExclusive => false;

		public virtual IEnumerable<Drawable> Format ( Drawable drawable )
			=> drawable.Yield();
		/// <summary>
		/// Applies only when <see cref="IsContentExclusive"/> = <see langword="true"/>.
		/// </summary>
		public virtual IEnumerable<Drawable> Format ( Match beginMatch, string text, Match endMatch )
			=> Format( text );
		public virtual IEnumerable<Drawable> Format ( string text ) {
			var t = new TextFlowContainer();
			var p = t.AddText( text );
			p.RecreateDrawablesFor( t );
			t.Clear( false );
			foreach ( SpriteText i in p.Drawables ) {
				yield return new OsuSpriteText { 
					Text = i.Text, 
					Font = OsuFont.GetFont( size: i.Font.Size )
				};
			}
			t.Dispose();
		}
	}

	public class QuickTextSpanFormater : TextSpanFormater {
		Regex begin;
		Func<Drawable, IEnumerable<Drawable>> format;
		public override Regex Begin => begin;

		public QuickTextSpanFormater ( string regex, Func<Drawable, IEnumerable<Drawable>> format ) {
			begin = new Regex( regex );
			this.format = format;
		}

		public QuickTextSpanFormater ( Regex regex, Func<Drawable, IEnumerable<Drawable>> format ) {
			begin = regex;
			this.format = format;
		}

		public override IEnumerable<Drawable> Format ( string text ) {
			return base.Format( text ).SelectMany( x => Format( x ) );
		}

		public override IEnumerable<Drawable> Format ( Drawable drawable ) {
			return format( drawable );
		}
	}

	public class NewLine : Drawable { }

	public class QuickTextSpanReplaceFormater : TextSpanFormater {
		Regex begin;
		Func<string, IEnumerable<Drawable>> format;
		public override Regex Begin => begin;
		public override Regex End => new Regex( "" );

		public override bool IsContentExclusive => true;

		public QuickTextSpanReplaceFormater ( string regex, Func<string, IEnumerable<Drawable>> format ) {
			begin = new Regex( regex );
			this.format = format;
		}

		public QuickTextSpanReplaceFormater ( Regex regex, Func<string, IEnumerable<Drawable>> format ) {
			begin = regex;
			this.format = format;
		}

		public override IEnumerable<Drawable> Format ( string text ) {
			return format( text );
		}
	}

	public class TextSpanLinkFormater : TextSpanFormater {
		public override Regex Begin => new Regex( @"\[([^]]*)\]\(([^)]*)\)" );
		public override Regex End => new Regex( "" );
		public override bool IsContentExclusive => true;

		public override IEnumerable<Drawable> Format ( Match startMatch, string text, Match endMatch ) {
			return new LinkButton( startMatch.Groups[ 1 ].Value, startMatch.Groups[ 2 ].Value ) {
				AutoSizeAxes = Axes.Both
			}.Yield();
		}

		private class LinkButton : CalmOsuAnimatedButton {
			[Resolved]
			private GameHost host { get; set; }

			public LinkButton ( string name, string url ) {
				TooltipText = url;
				Action = () => {
					host.OpenUrlExternally( url );
				};

				Add( new OsuSpriteText {
					Text = name,
					Font = OsuFont.GetFont( size: 20 ),
					Colour = Colour4.Cyan
				} );

				Add( new Circle {
					RelativeSizeAxes = Axes.X,
					Height = 2.4f,
					Origin = Anchor.BottomCentre,
					Anchor = Anchor.BottomCentre,
					Colour = Color4.Cyan
				} );
			}
		}
	}

	public class IconEmojiFormater : TextSpanFormater {
		public override Regex Begin => new Regex( ":(regular|solid|brand|osu)-([^:]+):", RegexOptions.IgnoreCase );
		public override Regex End => new Regex( "" );

		public override bool IsContentExclusive => true;

		public override IEnumerable<Drawable> Format ( Match beginMatch, string text, Match endMatch ) {
			IconUsage icon = FontAwesome.Regular.QuestionCircle;

			var group = beginMatch.Groups[ 1 ].Value.ToLower();
			var name = beginMatch.Groups[ 2 ].Value.ToLower();

			switch ( group ) {
				case "regular":
					var all = typeof( FontAwesome.Regular ).GetProperties( BindingFlags.Public | BindingFlags.Static );
					var c = all.FirstOrDefault( x => x.Name.ToLower() == name );
					if ( c is not null )
						icon = (IconUsage)c.GetGetMethod().Invoke( null, null );
					break;

				case "solid":
					all = typeof( FontAwesome.Solid ).GetProperties( BindingFlags.Public | BindingFlags.Static );
					c = all.FirstOrDefault( x => x.Name.ToLower() == name );
					if ( c is not null )
						icon = (IconUsage)c.GetGetMethod().Invoke( null, null );
					break;

				case "brand":
					all = typeof( FontAwesome.Brands ).GetProperties( BindingFlags.Public | BindingFlags.Static );
					c = all.FirstOrDefault( x => x.Name.ToLower() == name );
					if ( c is not null )
						icon = (IconUsage)c.GetGetMethod().Invoke( null, null );
					break;

				case "osu":
					all = typeof( OsuIcon ).GetProperties( BindingFlags.Public | BindingFlags.Static );
					c = all.FirstOrDefault( x => x.Name.ToLower() == name );
					if ( c is not null )
						icon = (IconUsage)c.GetGetMethod().Invoke( null, null );
					break;
			}

			return new BeatSyncedFlashingDrawable {
				Child = new SpriteIcon {
					Icon = icon,
					Origin = Anchor.CentreLeft,
					Anchor = Anchor.CentreLeft,
					RelativeSizeAxes = Axes.Both,
					Size = new Vector2( 0.8f )
				},
				Size = new Vector2( 20 )
			}.Yield();
		}
	}

	/// <summary>
	/// A drawable text object that supports advanded formatting with a markdown-like syntax.
	/// 
	/// The implemented tags are:
	/// <list type="bullet">
	/// <item>"^^text^^" to make the text bigger</item>
	/// <item>"*text*" to make the text in italics</item>
	/// <item>"**text**" to make the text bold</item>
	/// <item>"~~text~~" to make the text small and transparent</item>
	/// <item>"||text||" to make the text bold and hot-pink</item>
	/// <item>"<3" to add a beat synced heart!</item>
	/// <item>"[name](url)" to add a link</item>
	/// <item>":(solid|brand|regular|osu)-name:" to add a font awesome or osu icon</item>
	/// </list>
	/// </summary>
	public class BasicFormatedText : FormatedText {
		public BasicFormatedText () {
			Formaters.Add( () => new QuickTextSpanFormater( @"\|\|", d => {
				d.Colour = Color4.HotPink;
				if ( d is SpriteText sp )
					sp.Font = new FontUsage( sp.Font.Family, sp.Font.Size, OsuFont.GetWeightString( sp.Font.Family, FontWeight.Bold ), sp.Font.Italics, sp.Font.FixedWidth );

				return d.Yield();
			} ) );

			Formaters.Add( () => new QuickTextSpanFormater( @"\*\*", d => {
				if ( d is SpriteText sp )
					sp.Font = new FontUsage( sp.Font.Family, sp.Font.Size, OsuFont.GetWeightString( sp.Font.Family, FontWeight.Bold ), sp.Font.Italics, sp.Font.FixedWidth );

				return d.Yield();
			} ) );

			Formaters.Add( () => new QuickTextSpanFormater( @"\^\^", d => {
				if ( d is SpriteText sp )
					sp.Font = new FontUsage( sp.Font.Family, sp.Font.Size * 4 / 3, sp.Font.Weight, sp.Font.Italics, sp.Font.FixedWidth );
				else
					d.Scale *= 4f / 3;

				return d.Yield();
			} ) );


			Formaters.Add( () => new QuickTextSpanFormater( @"\*", d => {
				if ( d is SpriteText sp )
					sp.Font = new FontUsage( sp.Font.Family, sp.Font.Size, sp.Font.Weight, true, sp.Font.FixedWidth );

				return d.Yield();
			} ) );

			Formaters.Add( () => new QuickTextSpanFormater( @"~~", d => {
				d.Alpha /= 2;
				if ( d is SpriteText sp )
					sp.Font = new FontUsage( sp.Font.Family, sp.Font.Size * 3 / 4, sp.Font.Weight, sp.Font.Italics, sp.Font.FixedWidth );
				else
					d.Scale *= 3f / 4;

				return d.Yield();
			} ) );

			Formaters.Add( () => new QuickTextSpanReplaceFormater( @"<3", s => {
				return new BeatSyncedFlashingDrawable {
					Child = new SpriteIcon {
						Icon = FontAwesome.Solid.Heart,
						Origin = Anchor.CentreLeft,
						Anchor = Anchor.CentreLeft,
						RelativeSizeAxes = Axes.Both,
						Size = new Vector2( 0.8f )
					},
					Size = new Vector2( 20 )
				}.Yield();
			} ) );

			Formaters.Add( () => new TextSpanLinkFormater() );
			Formaters.Add( () => new IconEmojiFormater() );
		}
	}
}
