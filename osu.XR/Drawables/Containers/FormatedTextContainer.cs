using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.XR.Allocation;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.XR.Drawables.Containers {
	/// <summary>
	/// A drawable text object that supports advanded formatting with a markdown-like syntax.
	/// </summary>
	/// <typeparam name="Tsettings">The type used to keep track of effects to apply to the resulting text</typeparam>
	public class FormatedTextContainer<Tsettings> : CompositeDrawable {
		protected Dictionary<string, Action<Tsettings>> Formattings = new();
		protected Dictionary<string, Func<Drawable>> Replacements = new();

		protected Func<Tsettings> DefaultSettings;
		protected Action<Tsettings, SpriteText> ApplySettingsToText;
		protected Action<Tsettings, Drawable> ApplySettingsToDrawable;

		public Anchor TextAnchor {
			get => textFlow.TextAnchor;
			set {
				textFlow.TextAnchor = value;
				textFlow.Origin = value;
				textFlow.Anchor = value;
			}
		}
		new public Axes AutoSizeAxes {
			get => base.AutoSizeAxes;
			set => base.AutoSizeAxes = value;
		}
		ArbitraryTextFlowContainer textFlow;
		private class ArbitraryTextFlowContainer : TextFlowContainer {
			public void AddArbitraryDrawable ( Drawable d ) => AddInternal( d );
		}
		protected FormatedTextContainer ( Func<Tsettings> defaultSettings, Action<Tsettings, SpriteText> applySettingsToText, Action<Tsettings, Drawable> applySettingsToDrawable ) {
			DefaultSettings = defaultSettings;
			ApplySettingsToText = applySettingsToText;
			ApplySettingsToDrawable = applySettingsToDrawable;

			AddInternal( textFlow = new ArbitraryTextFlowContainer {
				AutoSizeAxes = Axes.Both
			} );

			TextBindable.BindValueChanged( v => RegenerateText( v.NewValue ) );
			DisplayFormattingCharactersBindable.BindValueChanged( v => RegenerateText( Text ) );
		}

		public readonly Bindable<string> TextBindable = new( "" );
		public readonly BindableBool DisplayFormattingCharactersBindable = new( false );

		public bool DisplayFormattingCharacters {
			get => DisplayFormattingCharactersBindable.Value;
			set => DisplayFormattingCharactersBindable.Value = value;
		}
		public string Text {
			get => TextBindable.Value;
			set => TextBindable.Value = value;
		}

		protected void RegenerateText ( string text ) {
			using var splices = ListPool<string>.Shared.Rent();

			string collected = "";

			bool canBeContinued ( string str ) {
				return ( Formattings.Keys.Concat( Replacements.Keys ) ).Any( x => x != str && x.StartsWith( str ) );
			}
			bool hasMatch ( string str ) {
				return (Formattings.Keys.Concat( Replacements.Keys ) ).Contains( str );
			}
			string seek ( int from ) {
				string match = "";
				for ( int i = from; i < text.Length; i++ ) {
					string current = text.Substring( from, i - from + 1 );
					if ( hasMatch( current ) ) match = current;
					if ( !canBeContinued( current ) ) break;
				}
				return match;
			}

			for ( int i = 0; i < text.Length; i++ ) {
				var seeked = seek( i );
				if ( seeked == "" ) {
					collected += text[ i ];
				}
				else {
					if ( collected != "" ) {
						splices.Add( collected );
						collected = "";
					}
					splices.Add( seeked );
					i += seeked.Length - 1;
				}
			}
			if ( collected != "" ) splices.Add( collected );

			using var activeSettings = ListPool<string>.Shared.Rent();

			textFlow.Clear();
			Tsettings currentSettings () {
				Tsettings settings = DefaultSettings();
				foreach ( var i in activeSettings ) {
					Formattings[ i ]( settings );
				}
				return settings;
			}

			foreach ( var i in splices ) {
				void render () {
					textFlow.AddText( i, s => {
						ApplySettingsToText( currentSettings(), s );
					} );
				}

				if ( hasMatch( i ) ) {
					if ( activeSettings.Contains( i ) ) {
						if ( DisplayFormattingCharacters ) render();
						activeSettings.Remove( i );
					}
					else {
						if ( Replacements.TryGetValue( i, out var func ) ) {
							var d = func();
							if ( d is SpriteText t ) {
								ApplySettingsToText( currentSettings(), t );
								textFlow.AddArbitraryDrawable( t );
							}
							else {
								ApplySettingsToDrawable( currentSettings(), d );
								textFlow.AddArbitraryDrawable( d );
							}
						}
						else {
							activeSettings.Add( i );
							if ( DisplayFormattingCharacters ) render();
						}
					}
				}
				else {
					render();
				}
			}
		}
	}

	public class FontSetings {
		public float Size = 16;
		public FontWeight Weight = FontWeight.Regular;
		public Color4 Color = Color4.White;
		public bool UseItalics = false;
		public float Alpha = 1;
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
	/// </list>
	/// </summary>
	public class FormatedTextContainer : FormatedTextContainer<FontSetings> {
		public FormatedTextContainer ( Func<FontSetings> defaultSettings = null ) : base( defaultSettings ?? ( () => new FontSetings() ), ( s, t ) => {
			t.Font = OsuFont.GetFont(
				size: s.Size,
				weight: s.Weight,
				italics: s.UseItalics
			);
			t.Colour = s.Color;
			t.Alpha = s.Alpha;
		}, ( s, d ) => {
			if ( d is not CompositeDrawable c || c.AutoSizeAxes == Axes.None ) d.Size = new Vector2( s.Size );
			d.Colour = s.Color;
			d.Alpha = s.Alpha;
		} ) {
			Formattings = new() {
				[ "^^" ] = s => s.Size *= 4f / 3,
				[ "*" ] = s => s.UseItalics = true,
				[ "**" ] = s => s.Weight = FontWeight.Bold,
				[ "~~" ] = s => {
					s.Size *= 3f / 4;
					s.Alpha /= 2;
				},
				[ "||" ] = s => {
					s.Weight = FontWeight.Bold;
					s.Color = Color4.HotPink;
				}
			};

			Replacements = new() {
				[ "<3" ] = () => {
					return new BeatSyncedFlashingDrawable {
						Child = new SpriteIcon {
							Icon = FontAwesome.Solid.Heart,
							Origin = Anchor.CentreLeft,
							Anchor = Anchor.CentreLeft,
							RelativeSizeAxes = Axes.Both,
							Size = new Vector2( 0.8f )
						}
					};
				},
				[ "Ko-fi" ] = () => {
					return new SpriteText { Text = "Ko-fi" };//return new MarkdownLinkText( "Ko-fi", "https://ko-fi.com/perigee" ) { };
				}
			};
		}
	}
}
