using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.XR.Bindables;
using osu.XR.Configuration;
using osu.XR.Configuration.Presets;

namespace osu.XR.Graphics.Settings;

public partial class ConfigPresetComponent<TLookup, TValue> : CompositeDrawable where TLookup : struct, Enum {
	[Resolved]
	ConfigPresetSource<TLookup> presetSource { get; set; } = null!;
	[Resolved]
	OverlayColourProvider colours { get; set; } = null!;

	SettingsItem<TValue> source;
	TLookup lookup;

	BindableBool isSlideoutEnabled = new( false );
	BindableBool isVisible = new( true );
	BindableProxy<TValue> backing;

	public ConfigPresetComponent ( TLookup lookup, SettingsItem<TValue> source, IConfigManagerWithCurrents<TLookup> config ) {
		this.source = source;
		this.lookup = lookup;

		RelativeSizeAxes = Axes.X;

		var settingBindable = config.GetBindable<TValue>( lookup );
		settingBindable.UnbindAll();
		backing = new( settingBindable );
		source.Current = settingBindable;
		config.BindWith( lookup, backing );
	}

	Container gripArea = null!;
	InteractionArea interactionArea = null!;
	Container all = null!;
	OsuAnimatedButton slideOutButton = null!;

	[BackgroundDependencyLoader]
	private void load () {
		isVisible.BindTo( presetSource.GetIsVisibleBindable( lookup ) );
		isSlideoutEnabled.BindTo( presetSource.IsSlideoutEnabled );

		backing.UpdatedFromDestination += v => {
			presetSource.Set( lookup, v.NewValue );
		};

		gripArea = new Container {
			RelativeSizeAxes = Axes.X,
			Y = -5,
			Child = slideOutButton = new OsuAnimatedButton {
				Width = 16,
				RelativeSizeAxes = Axes.Y,
				X = presetSource.SlideoutDirection == LeftRight.Left ? 2 : -2,
				Origin = presetSource.SlideoutDirection == LeftRight.Left ? Anchor.TopLeft : Anchor.TopRight,
				Anchor = presetSource.SlideoutDirection == LeftRight.Left ? Anchor.TopLeft : Anchor.TopRight,
				Action = slideOut
			}
		};
		slideOutButton.AddRange( new Drawable[] {
			new Box {
				Colour = colours.Background3,
				RelativeSizeAxes = Axes.Both
			},
			new SpriteIcon {
				Colour = colours.Foreground1,
				RelativeSizeAxes = Axes.Y,
				Size = new( 16 * 0.6f, 0.6f ),
				Origin = Anchor.Centre,
				Anchor = Anchor.Centre,
				Icon = presetSource.SlideoutDirection == LeftRight.Left ? FontAwesome.Solid.ChevronLeft : FontAwesome.Solid.ChevronRight
			}
		} );

		AddInternal( all = new() {
			RelativeSizeAxes = Axes.Both,
			Children = new Drawable[] {
				source.With( x => x.Origin = x.Anchor = presetSource.SlideoutDirection == LeftRight.Left ? Anchor.TopRight : Anchor.TopLeft ),
				gripArea
			}
		} );

		AddInternal( interactionArea = new( this, presetSource.SlideoutDirection ) { Y = -5 } );

		isSlideoutEnabled.BindValueChanged( v => {
			if ( v.NewValue ) {
				slideOutButton.ResizeWidthTo( 20, 200, Easing.Out )
					.MoveToX( presetSource.SlideoutDirection == LeftRight.Left ? 4 : -4, 200, Easing.Out );
				source.ResizeWidthTo( presetSource.SlideoutDirection == LeftRight.Left ? 0.93f : 0.95f, 200, Easing.Out );
			}
			else {
				slideOutButton.ResizeWidthTo( 16, 200, Easing.Out )
					.MoveToX( presetSource.SlideoutDirection == LeftRight.Left ? -6 : 6, 200, Easing.Out );
				source.ResizeWidthTo( 1, 200, Easing.Out );
			}

			source.Current.Disabled = v.NewValue;
			interactionArea.Alpha = v.NewValue ? 1 : 0;
			gripArea.FadeTo( interactionArea.Alpha, 200, Easing.Out );
		}, true );

		isVisible.BindValueChanged( v => {
			if ( v.NewValue ) {
				all.MoveToX( presetSource.SlideoutDirection == LeftRight.Left ? 200 : -200 ).MoveToX( 0, 200, Easing.Out );
				this.FadeIn( 200, Easing.Out );
			}
			else {
				all.MoveToX( presetSource.SlideoutDirection == LeftRight.Left ? -1000 : 1000, 400 );
				this.FadeOut( 200, Easing.Out );
			}
		}, true );

		FinishTransforms( true );
	}

	protected override void Update () {
		base.Update();
		interactionArea.Height = gripArea.Height = source.Height + 10;
		Height = source.DrawHeight;
	}

	float startX;
	bool draggingOut;
	protected override bool OnDragStart ( DragStartEvent e ) {
		if ( !isSlideoutEnabled.Value )
			return false;

		startX = all.X;
		draggingOut = false;
		var delta = e.MousePosition - e.MouseDownPosition;
		return Math.Abs( delta.X ) > Math.Abs( delta.Y );
	}
	protected override void OnDrag ( DragEvent e ) {
		e.Target = this;
		var prev = all.X;
		all.X = presetSource.SlideoutDirection == LeftRight.Left
			? MathF.Min( 0, startX + e.MousePosition.X - e.MouseDownPosition.X )
			: MathF.Max( 0, startX + e.MousePosition.X - e.MouseDownPosition.X );
		var delta = all.X - prev;
		draggingOut = presetSource.SlideoutDirection == LeftRight.Left ? delta < 0 : delta > 0;
	}
	protected override void OnDragEnd ( DragEndEvent e ) {
		if ( draggingOut && float.Abs( all.X ) > DrawWidth / 6 ) {
			slideOut();
		}
		else {
			all.MoveToX( 0, 200, Easing.Out );
		}
	}

	void slideOut () {
		if ( presetSource.ViewType == PresetViewType.Preset )
			presetSource.Remove( lookup );
		else if ( presetSource.ViewType == PresetViewType.ItemList )
			presetSource.Set( lookup, source.Current.Value );
	}

	partial class InteractionArea : Drawable {
		Drawable parent;
		LeftRight direction;
		public InteractionArea ( Drawable parent, LeftRight direction ) {
			this.parent = parent;
			RelativeSizeAxes = Axes.X;
			this.direction = direction;
		}

		public override bool ReceivePositionalInputAt ( Vector2 screenSpacePos ) {
			if ( direction == LeftRight.Right ) {
				return base.ReceivePositionalInputAt( screenSpacePos )
					&& ToLocalSpace( screenSpacePos ).X < DrawWidth - 16;
			}
			else {
				return base.ReceivePositionalInputAt( screenSpacePos )
					&& ToLocalSpace( screenSpacePos ).X > 16;
			}
		}

		protected override bool Handle ( UIEvent e ) {
			return parent.TriggerEvent( e );
		}
	}
}

public static class PresetExtensions {
	public static ConfigPresetComponent<TLookup, TValue> PresetComponent<TLookup, TValue> ( this SettingsItem<TValue> self, IConfigManagerWithCurrents<TLookup> config, TLookup lookup )
		where TLookup : struct, Enum {
		return new( lookup, self, config );
	}
}
