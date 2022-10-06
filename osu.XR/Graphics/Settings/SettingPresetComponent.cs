using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.XR;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.XR.Configuration;

namespace osu.XR.Graphics.Settings;

public interface ISettingPresetComponent<Tlookup> {
	Tlookup Lookup { get; }
}

public class SettingPresetComponent<Tlookup, Tvalue> : CompositeDrawable, ISettingPresetComponent<Tlookup> where Tlookup : struct, Enum {
	[Resolved]
	OverlayColourProvider colours { get; set; } = null!;

	[Resolved]
	SettingPresetContainer<Tlookup> presetContainer { get; set; } = null!;
	public readonly IHasCurrentValue<Tvalue> Source;
	Drawable sourceDrawable;
	public Tlookup Lookup { get; }

	BindableBool isEditingBindable = new( false );
	BindableBool isPreviewBindable = new( false );
	Bindable<ConfigurationPreset<Tlookup>?> selectedPresetBindable = new();
	BindableList<Tlookup> presetKeys = new();

	public SettingPresetComponent ( Tlookup lookup, IHasCurrentValue<Tvalue> source ) {
		Source = source;
		Lookup = lookup;
		sourceDrawable = (Drawable)source;
		RelativeSizeAxes = Axes.X;
	}

	Container gripArea = null!;
	InteractionArea interactionArea = null!;
	Container all = null!;
	OsuAnimatedButton slideOutButton = null!;
	protected override void LoadComplete () {
		AddInternal( all = new() {
			RelativeSizeAxes = Axes.Both
		} );
		all.Add( sourceDrawable );
		all.Add( gripArea = new Container {
			RelativeSizeAxes = Axes.X,
			Y = -5,
			Children = new Drawable[] {
				slideOutButton = new OsuAnimatedButton {
					Width = 16,
					RelativeSizeAxes = Axes.Y,
					X = -2,
					Origin = Anchor.TopRight,
					Anchor = Anchor.TopRight,
					Action = slideOut
				}
			}
		} );

		slideOutButton.AddRange( new Drawable[] {
			new Box {
				Colour = colours.Background3,
				RelativeSizeAxes = Axes.Both,
				Depth = 2
			},
			new SpriteIcon {
				Colour = colours.Foreground1,
				RelativeSizeAxes = Axes.Y,
				Size = new( 16 * 0.6f, 0.6f ),
				Origin = Anchor.Centre,
				Anchor = Anchor.Centre,
				Icon = FontAwesome.Solid.ChevronRight,
				Depth = 1
			}
		} );

		AddInternal( interactionArea = new( this ) { Y = -5 } );

		isEditingBindable.BindTo( presetContainer.IsEditingBindable );
		isPreviewBindable.BindTo( presetContainer.IsPreviewBindable );
		selectedPresetBindable.BindTo( presetContainer.SelectedPresetBindable );

		presetKeys.BindCollectionChanged( (_, e) => {
			if ( e.OldItems?.Contains( Lookup ) == true || e.NewItems?.Contains( Lookup ) == true )
				updateVisibility();
		} );
		selectedPresetBindable.BindValueChanged( v => {
			if ( v.OldValue?.Keys is BindableList<Tlookup> old )
				presetKeys.UnbindFrom( old );

			if ( v.NewValue?.Keys is BindableList<Tlookup> @new )
				presetKeys.BindTo( @new );
			else
				presetKeys.Clear();
		}, true );
		(isEditingBindable, isPreviewBindable).BindValuesChanged( updateVisibility, true );

		FinishTransforms( true );
	}

	void updateVisibility () {
		FinishTransforms( true );
		var editing = isEditingBindable.Value;

		interactionArea.Alpha = editing ? 1 : 0;
		gripArea.FadeTo( interactionArea.Alpha, 200, Easing.Out );

		if ( presetContainer.IsVisible( this ) ) {
			all.X = -200;
			all.MoveToX( 0, 200, Easing.Out );
			this.FadeIn( 200, Easing.Out );
		}
		else {
			all.MoveToX( 1000, 400 );
			this.FadeOut( 200, Easing.Out );
		}

		if ( editing ) {
			slideOutButton.ResizeWidthTo( 20, 200, Easing.Out )
				.MoveToX( -4, 200, Easing.Out );
			sourceDrawable.ResizeWidthTo( 0.95f, 200, Easing.Out );
		}
		else {
			slideOutButton.ResizeWidthTo( 16, 200, Easing.Out )
				.MoveToX( 6, 200, Easing.Out );
			sourceDrawable.ResizeWidthTo( 1, 200, Easing.Out );
		}
	}

	protected override void Update () {
		base.Update();
		interactionArea.Height = gripArea.Height = sourceDrawable.Height + 10;
		Height = sourceDrawable.DrawHeight;
	}

	float startX;
	bool draggingOut;
	protected override bool OnDragStart ( DragStartEvent e ) {
		if ( !isEditingBindable.Value )
			return false;

		startX = all.X;
		draggingOut = false;
		var delta = e.MousePosition - e.MouseDownPosition;
		return Math.Abs(delta.X) > Math.Abs(delta.Y);
	}
	protected override void OnDrag ( DragEvent e ) {
		e.Target = this;
		var prev = all.X;
		all.X = MathF.Max( 0, startX + e.MousePosition.X - e.MouseDownPosition.X );
		var delta = all.X - prev;
		draggingOut = delta > 0;
	}
	protected override void OnDragEnd ( DragEndEvent e ) {
		if ( draggingOut && all.X > DrawWidth / 6 ) {
			slideOut();
		}
		else {
			all.MoveToX( 0, 200, Easing.Out );
		}
	}

	void slideOut () {
		if ( isPreviewBindable.Value )
			presetContainer.Remove( this );
		else
			presetContainer.Set( this, Source.Current.Value );
	}

	class InteractionArea : Drawable {
		SettingPresetComponent<Tlookup, Tvalue> parent;
		public InteractionArea ( SettingPresetComponent<Tlookup, Tvalue> parent ) {
			this.parent = parent;
			RelativeSizeAxes = Axes.X;
		}

		public override bool ReceivePositionalInputAt ( Vector2 screenSpacePos ) {
			return base.ReceivePositionalInputAt( screenSpacePos ) 
				&& ToLocalSpace( screenSpacePos ).X < DrawWidth - 16;
		}

		protected override bool Handle ( UIEvent e ) {
			return parent.TriggerEvent( e );
		}
	}
}