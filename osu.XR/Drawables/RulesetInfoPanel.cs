using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osu.XR.Input.Custom;
using osu.XR.Input.Custom.Persistence;
using osu.XR.Settings;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace osu.XR.Drawables {
	public class RulesetInfoPanel : ConfigurationContainer {
		FillFlowContainer container;
		public RulesetInfoPanel () {
			Title = "Ruleset";
			Description = "adjust how you play the ruleset in XR";
		}

		[Resolved]
		private IBindable<RulesetInfo> ruleset { get; set; }

		public BindableList<CustomBinding> GetBindingsForVariant ( int variant )
			=> settings[ ruleset.Value ].GetBindingForVariant( variant );

		Dictionary<RulesetInfo, RulesetXrBindingSection> settings = new();
		[Resolved]
		Storage storage { get; set; }
		const string saveFilePath = "Bindings.json";
		protected override void LoadComplete () {
			base.LoadComplete();
			storage = storage.GetStorageForDirectory( "XR" );

			loadBindings();
			ruleset.BindValueChanged( OnRulesetChanged, true );
		}

		RulesetBindingsFile saveFile;
		private void loadBindings () { // this will do until we have a breaking change
			try {
				if ( storage.Exists( saveFilePath ) ) {
					using var s = storage.GetStream( saveFilePath );
					var reader = new StreamReader( s );
					JToken token = Newtonsoft.Json.JsonConvert.DeserializeObject<object>( reader.ReadToEnd() ) as JToken;
					if ( token is null ) {
						OpenVR.NET.Events.Error( "Could not load Bindings file: Invalid file" );
					}
					saveFile = RulesetBindingsFile.Load( token );

					// create a backup file
					using var ss = storage.GetStream( saveFilePath );
					reader = new StreamReader( ss );
					using var c = storage.GetStream( saveFilePath + "~", FileAccess.Write, mode: FileMode.Create );
					var writer = new StreamWriter( c );
					writer.Write( reader.ReadToEnd() );
					writer.Flush();
				}
			}
			catch ( Exception e ) {
				saveFile = new();
				OpenVR.NET.Events.Exception( e, "Could not load Bindings file" );
			}
		}

		private void saveBindings () {
			using var s = storage.GetStream( saveFilePath, FileAccess.Write, mode: FileMode.Create );
			var writer = new StreamWriter( s );
			writer.Write( Newtonsoft.Json.JsonConvert.SerializeObject( CreateSaveFile(), Newtonsoft.Json.Formatting.Indented ) );
			writer.Flush();
		}

		public RulesetBindingsFile CreateSaveFile () {
			RulesetBindingsFile file = new();

			foreach ( var (ruleset,manager) in settings ) {
				file.Rulesets.Add( manager.CreateSaveData() );
			}

			return file.MergeWith( saveFile );
		}

		double time;
		protected override void Update () {
			base.Update();

			if ( time > 0 ) {
				time -= Clock.ElapsedFrameTime;
				if ( time < 0 ) {
					saveBindings();
				}
			}
		}

		private void OnRulesetChanged ( ValueChangedEvent<RulesetInfo> v ) {
			if ( v.NewValue is null ) return;
			ClearSections( dispose: false );

			if ( !settings.ContainsKey( v.NewValue ) ) {
				var ruleset = v.NewValue.CreateInstance();
				var section = new RulesetXrBindingSection( ruleset );
				section.SettingsChanged += onSettingsChanged;
				settings.Add( v.NewValue, section );
				if ( saveFile.Rulesets.FirstOrDefault( x => x.Name == v.NewValue.ShortName ) is RulesetBindings bindings ) {
					settings[ v.NewValue ].LoadSaveFile( bindings );
				}
			}

			AddSection( settings[ v.NewValue ], name: v.NewValue.Name );
		}

		private void onSettingsChanged () {
			time = 1000;
		}
	}

	public class RulesetXrBindingSection : SettingsSubsection {
		protected override string Header => "Bindings";

		public BindableList<CustomBinding> GetBindingForVariant ( int variant )
			=> getVariant( variant ).ActiveInputs;

		Ruleset ruleset;
		List<int> variants;
		Dictionary<int, RulesetVariantXrBindingsSubsection> variantSettings = new();
		RulesetVariantXrBindingsSubsection active;
		public RulesetXrBindingSection ( Ruleset ruleset ) {
			this.ruleset = ruleset;
			variants = ruleset.AvailableVariants.ToList();
			if ( variants.Count > 1 ) {
				SettingsDropdown<string> settings;
				Add( settings = new SettingsDropdown<string> {
					LabelText = "Variant",
					Items = variants.Select( x => ruleset.GetVariantName( x ) ),
					Current = new Bindable<string>( ruleset.GetVariantName( variants.FirstOrDefault() ) )
				} );

				settings.Current.BindValueChanged( v => {
					setVariant( variants.FirstOrDefault( x => ruleset.GetVariantName( x ) == v.NewValue ) );
				}, true );
			}
			else {
				setVariant( variants.FirstOrDefault() );
			}
		}

		public void LoadSaveFile ( RulesetBindings saveFile ) {
			clearWarnings();
			if ( saveFile.VariantNames.Count != variants.Count || !variants.All( x => saveFile.VariantNames.ContainsKey( x ) && saveFile.VariantNames[ x ] == ruleset.GetVariantName( x ) ) ) {
				createWarning( "Variants for this ruleset defined in the save file do not match the detected variants. Your bindings might be corrupted." );
			}

			foreach ( var (variant,data) in saveFile.Variants ) {
				var settings = getVariant( variant );
				if ( settings is null ) {
					createWarning( $"Could not load variant defined as \"{saveFile.VariantNames[variant]}\"" );
				}
				else {
					settings.LoadSaveFile( data );
				}
			}
		}
		void clearWarnings () {

		}
		void createWarning ( string message ) {
			OpenVR.NET.Events.Error( message );
		}

		RulesetVariantXrBindingsSubsection getVariant ( int variant ) {
			if ( !variants.Contains( variant ) ) return null;

			if ( !variantSettings.ContainsKey( variant ) ) {
				var section = new RulesetVariantXrBindingsSubsection( ruleset, variant );
				section.SettingsChanged += onSettingsChanged;
				variantSettings.Add( variant, section );
			}

			return variantSettings[ variant ];
		}

		public event Action SettingsChanged;
		private void onSettingsChanged () {
			SettingsChanged?.Invoke();
		}

		void setVariant ( int variant ) {
			if ( active != null ) Remove( active );
			Add( active = getVariant( variant ) );
		}

		public RulesetBindings CreateSaveData () {
			RulesetBindings data = new() {
				Name = ruleset.RulesetInfo.ShortName
			};

			foreach ( var variant in variants ) {
				data.VariantNames.Add( variant, ruleset.GetVariantName( variant ) );
			}

			foreach ( var (variant,manager) in variantSettings ) {
				data.Variants.Add( variant, manager.CreateSaveData() );
			}

			return data;
		}
	}

	public class RulesetVariantXrBindingsSubsection : SettingsSubsection {
		protected override string Header => ruleset.GetVariantName( Variant );

		public readonly int Variant;
		Ruleset ruleset;
		[Cached]
		BindableList<object> sharedSettings = new(); 
		[Cached]
		public readonly List<object> RulesetActions = new();

		static Dictionary<string, Func<CustomBinding>> avaiableInputs = new() {
			[ "Clap" ] = () => new ClapBinding(),
			[ "Left Joystick" ] = () => new JoystickBinding( Hand.Left ),
			[ "Right Joystick" ] = () => new JoystickBinding( Hand.Right ),
			[ "Left Buttons" ] = () => new ButtonBinding( Hand.Left ),
			[ "Right Buttons" ] = () => new ButtonBinding( Hand.Right )
		};

		Dictionary<string, CustomBinding> removedInputs = new();
		public readonly BindableList<CustomBinding> ActiveInputs = new();
		Dictionary<string, CustomBinding> selectedInputs = new();
		Dictionary<string, Drawable> inputDrawables = new();
		FillFlowContainer container;
		OsuButton addButton;
		SettingsDropdown<string> dropdown;

		public RulesetVariantXrBindingsSubsection ( Ruleset ruleset, int variant ) {
			this.ruleset = ruleset;
			this.Variant = variant;
			RulesetActions = ruleset.GetDefaultKeyBindings( variant ).Select( x => x.Action ).Distinct().ToList();

			Add( container = new FillFlowContainer {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Direction = FillDirection.Vertical
			} );

			container.Add( addButton = new OsuButton {
				Height = 25,
				Width = 120,
				Margin = new MarginPadding { Left = 16 },
				Text = "Add",
				Action = () => {
					addCustomInput( dropdown.Current.Value );
				}
			} );

			container.Add( dropdown = new SettingsDropdown<string> {
				Current = new Bindable<string>( "Select type" )
			} );

			dropdown.Current.BindValueChanged( v => {
				addButton.Enabled.Value = v.NewValue != dropdown.Current.Default;
			}, true );

			updateDropdown();

			ActiveInputs.CollectionChanged += (_,_) => onSettingsChanged();
		}

		protected override void Update () {
			foreach ( var i in inputDrawables.Values ) {
				i.Width = container.DrawWidth - 32;
			}
			base.Update();
		}

		void updateDropdown () {
			dropdown.Items = avaiableInputs.Keys.Except( selectedInputs.Keys ).Prepend( dropdown.Current.Default );
			dropdown.Current.SetDefault();
		}

		CustomBinding addCustomInput ( string ID ) {
			CustomBinding input;
			if ( removedInputs.ContainsKey( ID ) ) {
				removedInputs.Remove( ID, out input );
				selectedInputs.Add( ID, input );
				ActiveInputs.Add( input );

				container.Add( inputDrawables[ ID ] );
				updateDropdown();
				return input;
			}
			
			input = avaiableInputs[ ID ]();
			input.SettingsChanged += onSettingsChanged;

			selectedInputs.Add( ID, input );
			ActiveInputs.Add( input );

			var handler = input.CreateHandler();
			inputDrawables.Add( ID, new Container {
				Masking = true,
				CornerRadius = 5,
				AutoSizeAxes = Axes.Y,
				Margin = new MarginPadding { Left = 16, Right = 16, Bottom = 4 },
				Children = new Drawable[] {
					new Box {
						RelativeSizeAxes = Axes.Both,
						Colour = OsuColour.Gray( 0.1f )
					},
					new FillFlowContainer {
						RelativeSizeAxes = Axes.X,
						AutoSizeAxes = Axes.Y,
						Direction = FillDirection.Vertical,
						Children = new Drawable[] {
							new Container {
								RelativeSizeAxes = Axes.X,
								AutoSizeAxes = Axes.Y,
								Margin = new MarginPadding { Bottom = 8 },
								Children = new Drawable[] {
									new OsuTextFlowContainer( x => x.Font = OsuFont.GetFont( size: 24 ) ) {
										Text = ID,
										Margin = new MarginPadding { Bottom = 4, Left = 6, Top = 4 },
										RelativeSizeAxes = Axes.X,
										AutoSizeAxes = Axes.Y
									},
									new OsuButton {
										Anchor = Anchor.CentreRight,
										Origin = Anchor.CentreRight,
										Text = "X",
										BackgroundColour = Color4.HotPink,
										Action = () => removeCustomInput( ID ),
										Width = 25,
										Height = 25
									}
								}
							},
							new Container {
								RelativeSizeAxes = Axes.X,
								AutoSizeAxes = Axes.Y,
								Children = new Drawable[] {
									handler,
									handler.CreateSettingsDrawable()
								}
							}
						}
					}
				}
			} );
			container.Add( inputDrawables[ ID ] );
			updateDropdown();

			return input;
		}

		public event Action SettingsChanged;
		private void onSettingsChanged () {
			SettingsChanged?.Invoke();
		}

		void removeCustomInput ( string ID ) {
			container.Remove( inputDrawables[ ID ] );
			selectedInputs.Remove( ID, out var input );
			ActiveInputs.Remove( input );
			removedInputs.Add( ID, input );
			updateDropdown();
		}

		public RulesetVariantBindings CreateSaveData () {
			RulesetVariantBindings data = new() {
				Name = ruleset.GetVariantName( Variant )
			};

			foreach ( var (action,i) in RulesetActions.Zip( Enumerable.Range(0, RulesetActions.Count) ) ) {
				data.Actions.Add( i, action.GetDescription() );
			}

			SaveDataContext context = new( this );
			foreach ( var (ID,binding) in selectedInputs ) {
				data.Bindings.Add( new BindingData {
					Type = ID,
					Data = binding.CreateSaveData( context )
				} );
			}

			return data;
		}

		public void LoadSaveFile ( RulesetVariantBindings data ) {
			clearWarnings();
			if ( data.Actions.Count != RulesetActions.Count || !data.Actions.All( x => x.Key == RulesetActions.FindIndex( y => y.GetDescription() == x.Value ) ) ) {
				createWarning( "Actions for this variant defined in the save file do not match the detected actions. Your bindings might be corrupted." );
			}

			foreach ( var i in selectedInputs.Keys.ToArray() ) {
				removeCustomInput( i );
			}

			inputDrawables.Clear();
			removedInputs.Clear();

			var context = new SaveDataContext( this );
			foreach ( var binding in data.Bindings ) {
				if ( avaiableInputs.ContainsKey( binding.Type ) ) {
					addCustomInput( binding.Type ).Load( binding.Data as JToken, context );
				}
				else {
					createWarning( $"Could not add binding defined as \"{binding.Type}\"" );
				}
			}
		}
		void clearWarnings () {

		}
		void createWarning ( string message ) {
			OpenVR.NET.Events.Error( message );
		}
	}
}
