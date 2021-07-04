using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.XR.Drawables.Containers;
using osuTK;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace osu.XR.Drawables {
	public abstract class HierarchyView<Tstep,Ttype> : FillFlowContainer, IHasFilterableChildren where Tstep : HierarchyStep<Ttype> {
		public event Action<Tstep> StepHovered;
		public event Action<Tstep> StepHoverLost;
		public event Action<Tstep> StepSelected;
		public event Action SearchTermsModified;

		FillFlowContainer hierarchy;

		public IEnumerable<Tstep> MultiselectSelection => multiselectSelection.OfType<Tstep>();
		readonly BindableList<HierarchyStep<Ttype>> multiselectSelection = new();
		public readonly BindableBool IsMultiselectBindable = new( false );
		public bool IsMultiselect {
			get => IsMultiselectBindable.Value;
			set => IsMultiselectBindable.Value = value;
		}

		/// <summary>
		/// Should clicking a step focus the view on it?
		/// </summary>
		public bool SelectionNavigates = true;

		public HierarchyView ( Ttype value ) {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Direction = FillDirection.Vertical;

			Add( hierarchy = new FillFlowContainer {
				AutoSizeAxes = Axes.Y,
				RelativeSizeAxes = Axes.X,
				Direction = FillDirection.Vertical
			} );

			setTop( CreateTop( value ) );
			top.IsMultiselect.BindTo( IsMultiselectBindable );
			top.MultiselectSelection.BindTo( multiselectSelection );
		}

		void setTop ( Tstep newTop ) {
			if ( top == newTop ) return;

			if ( top is not null ) {
				if ( hierarchy.Contains( top ) ) hierarchy.Remove( top );
				top.Selected -= childSelected;
				top.Hovered -= childHovered;
				top.HoverLost -= childHoverLost;
				top.SearchTermsModified -= onSearchTermsModified;
			}

			top = newTop;
			hierarchy.Insert( 1, top );
			top.Selected += childSelected;
			top.Hovered += childHovered;
			top.HoverLost += childHoverLost;
			top.SearchTermsModified += onSearchTermsModified;
			top.Margin = new MarginPadding { Horizontal = 15 };

			setParent();
		}
		void setParent () { // TODO react when the parent changes
			if ( parent is not null ) hierarchy.Remove( parent );

			parent = CreateParent( top );
			if ( parent is not null ) {
				parent.Margin = new MarginPadding { Left = 10, Right = 15 };
				parent.CanBeExpanded.Value = false;
				hierarchy.Insert( -1, parent );

				parent.Selected += parentSelected;
			}
		}

		public void FocusOn ( Ttype value ) {
			var step = top.FindStep( value );
			if ( step is null ) {
				setTop( CreateTop( value ) );
				top.IsMultiselect.BindTo( IsMultiselectBindable );
				top.MultiselectSelection.BindTo( multiselectSelection );
			}
			else {
				focusOn( (Tstep)step );
			}
		}
		void focusOn ( Tstep step ) {
			if ( step == top ) return;
			step.SplitFromParent();
			setTop( (Tstep)step );
			top.IsExpanded.Value = true;

			SearchTermsModified?.Invoke();
		}

		private void onSearchTermsModified () {
			SearchTermsModified?.Invoke();
		}
		private void childHoverLost ( HierarchyStep<Ttype> obj ) {
			StepHoverLost?.Invoke( (Tstep)obj );
		}
		private void childHovered ( HierarchyStep<Ttype> obj ) {
			StepHovered?.Invoke( (Tstep)obj );
		}

		private void childSelected ( HierarchyStep<Ttype> obj ) {
			if ( !SelectionNavigates ) {
				StepSelected?.Invoke( (Tstep)obj );
				return;
			}
			if ( obj == top ) return;
			obj.SplitFromParent();
			setTop( (Tstep)obj );
			top.IsExpanded.Value = true;

			StepSelected?.Invoke( top );
			SearchTermsModified?.Invoke();
		}

		private void parentSelected ( HierarchyStep<Ttype> obj ) {
			if ( !SelectionNavigates ) return;

			hierarchy.Remove( top );
			setTop( (Tstep)top.WrapInParent().SplitFromParent() );

			StepSelected?.Invoke( top );
			SearchTermsModified?.Invoke();
		}

		private Tstep top;
		private Tstep parent;
		protected abstract Tstep CreateTop ( Ttype value );
		protected abstract Tstep CreateParent ( Tstep top );

		public IEnumerable<IFilterable> FilterableChildren => top.Yield();
		public bool MatchingFilter { set { } }
		public bool FilteringActive { set { } }
		public IEnumerable<string> FilterTerms => Array.Empty<string>();
	}

	public abstract class HierarchyStep<Ttype> : FillFlowContainer, IHasFilterableChildren { // TODO view filters
		public event Action<HierarchyStep<Ttype>> Hovered;
		public event Action<HierarchyStep<Ttype>> HoverLost;
		public event Action<HierarchyStep<Ttype>> Selected;
		public event Action SearchTermsModified;

		protected void InvokeSearchTermsModified () {
			SearchTermsModified?.Invoke();
		}

		public readonly BindableBool IsMultiselect = new( false );
		public readonly BindableBool IsMultiselected = new( false );
		public readonly BindableList<HierarchyStep<Ttype>> MultiselectSelection = new();

		public Ttype Value { get; private set; }
		HierarchyStep<Ttype> parent;
		AutoScrollContainerSyncGroup parentSyncGroup = new();
		AutoScrollContainerSyncGroup syncGroup = new();

		FillFlowContainer header;
		SelectionNub selection;
		HierarchyButton button;
		CalmOsuAnimatedButton toggleButton;
		SpriteIcon dropdownChevron;
		protected HierarchyStep ( Ttype value ) {
			Value = value;
			AutoSizeAxes = Axes.Y;
			Direction = FillDirection.Vertical;

			toggleButton = new CalmOsuAnimatedButton {
				Origin = Anchor.CentreRight,
				Anchor = Anchor.CentreRight,
				Action = IsExpanded.Toggle,
				RelativeSizeAxes = Axes.Y,
				Width = 80,
				Alpha = 0
			};
			AddInternal( syncGroup );

			bool ignoreListAdd = false;
			IsMultiselected.BindValueChanged( v => {
				if ( ignoreListAdd ) return;

				if ( v.NewValue ) {
					MultiselectSelection.Add( this );
				}
				else {
					MultiselectSelection.Remove( this );
				}
			} );

			MultiselectSelection.BindCollectionChanged( (e,o) => {
				if ( o.Action == NotifyCollectionChangedAction.Add ) {
					if ( o.NewItems == null ) return;
					if ( o.NewItems.Contains( this ) ) {
						ignoreListAdd = true;
						IsMultiselected.Value = true;
						ignoreListAdd = false;
					}
				}
				else {
					if ( o.OldItems == null ) return;
					if ( o.OldItems.Contains( this ) ) IsMultiselected.Value = false;
				}
			} );
		}

		protected virtual void OnSelected () {
			Selected?.Invoke( this );
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			Add( header = new FillFlowContainer {
				RelativeSizeAxes = Axes.X,
				AutoSizeAxes = Axes.Y,
				Direction = FillDirection.Horizontal
			} );
			header.Add( selection = new SelectionNub {
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft,
				Current = IsMultiselected
			} );
			header.Add( button = new HierarchyButton( Title, CreateIcon(), parentSyncGroup ) {
				Margin = new MarginPadding { Left = 5 },
				Hovered = () => Hovered?.Invoke( this ),
				HoverLost = () => HoverLost?.Invoke( this ),
				Action = OnSelected
			} );
			button.Add( toggleButton );
			button.OnUpdate += d => d.Margin = new MarginPadding { Left = 5, Right = selection.LayoutSize.X };
			IsMultiselect.BindValueChanged( v => {
				if ( v.NewValue ) {
					selection.ScaleTo( 1, 100, Easing.Out );
				}
				else {
					selection.ScaleTo( new Vector2( 0, 1 ), 100, Easing.Out );
				}
			}, true );
			FinishTransforms( true );

			toggleButton.Add( new Container {
				RelativeSizeAxes = Axes.X,
				Height = 15,
				Anchor = Anchor.Centre,
				Origin = Anchor.Centre,
				Child = dropdownChevron = new SpriteIcon {
					RelativeSizeAxes = Axes.Both,
					Icon = FontAwesome.Solid.ChevronDown,
					FillMode = FillMode.Fit,
					Anchor = Anchor.Centre,
					Origin = Anchor.Centre
				}
			} );

			IsExpanded.BindValueChanged( v => {
				if ( v.NewValue ) {
					if ( CanBeExpanded.Value ) {
						dropdownChevron.FadeTo( 0.3f, 100 );
						foreach ( var (k, c) in children ) {
							Add( c );
						}
						needsSorting = true;
						InvokeSearchTermsModified();
					}
				}
				else {
					dropdownChevron.FadeTo( 1f, 100 );
					foreach ( var (k, c) in children ) {
						Remove( c );
					}
					InvokeSearchTermsModified();
				}
			} );

			CanBeExpanded.BindValueChanged( v => {
				if ( v.NewValue ) {
					if ( children.Any() ) {
						toggleButton.FadeIn( 120, Easing.Out );
					}
				}
				else {
					IsExpanded.Value = false;
				}
			} );
		}

		protected virtual int Sort ( HierarchyStep<Ttype> a, HierarchyStep<Ttype> b ) => 0;
		void sortChildren () {
			var order = children.Values.ToList();
			order.Sort( Sort );

			for ( int i = 0; i < order.Count; i++ ) {
				SetLayoutPosition( order[ i ], i );
			}
			InvalidateLayout();
		}

		public readonly BindableBool CanBeExpanded = new( true );
		public readonly BindableBool IsExpanded = new( false );
		private Dictionary<Ttype, HierarchyStep<Ttype>> children = new();
		new public IReadOnlyDictionary<Ttype, HierarchyStep<Ttype>> Children => children;
		private void AddChild ( HierarchyStep<Ttype> child ) {
			if ( children.ContainsKey( child.Value ) ) {
				RemoveChild( child.Value );
			}

			child.parentSyncGroup = syncGroup;
			child.Margin = new MarginPadding { Left = 5 };
			children.Add( child.Value, child );
			if ( IsExpanded.Value ) {
				Add( child );
				needsSorting = true;
				InvokeSearchTermsModified();
			}

			if ( children.Count == 1 && CanBeExpanded.Value ) {
				toggleButton.FadeIn( 120, Easing.Out );
			}

			OnChildAdded( child );
		}
		/// <summary>
		/// Happens when a child is added. This childs bindables should already be bound here.
		/// You must call the base method.
		/// </summary>
		protected virtual void OnChildAdded ( HierarchyStep<Ttype> child ) {
			
		}
		/// <summary>
		/// Happens when a child is created. This is a good place to bind childs bindables to the parents bindables.
		/// You must call the base method.
		/// </summary>
		protected virtual void OnChildCreated ( HierarchyStep<Ttype> child ) {
			child.IsMultiselect.BindTo( IsMultiselect );
			child.MultiselectSelection.BindTo( MultiselectSelection );
			child.Hovered += c => Hovered?.Invoke( c );
			child.HoverLost += c => HoverLost?.Invoke( c );
			child.Selected += c => Selected?.Invoke( c );
			child.SearchTermsModified += () => SearchTermsModified?.Invoke();
		}
		protected void AddChild ( Ttype value ) {
			if ( children.ContainsKey( value ) ) return;

			var child = CreateChild( value );
			child.parent = this;
			OnChildCreated( child );
			AddChild( child );
		}
		protected void RemoveChild ( Ttype value ) {
			children.Remove( value, out var child );
			if ( IsExpanded.Value ) {
				Remove( child );
				InvokeSearchTermsModified();
			}

			if ( children.Count == 0 && CanBeExpanded.Value ) {
				toggleButton.FadeOut( 120, Easing.Out );
			}

			OnChildRemoved( child );
		}
		/// <summary>
		/// Happens when a child is removed. You should not unbind parent bindables from the child because it might rejoin later.
		/// You must call the base method.
		/// </summary>
		protected virtual void OnChildRemoved ( HierarchyStep<Ttype> child ) {
			
		}
		protected void ChangeChildValue ( Ttype old, Ttype @new ) {
			children.Remove( old, out var child );
			children.Add( @new, child );

			child.Value = @new;
			child.Schedule( () => {
				child.ValueChanged( old, @new );
				needsSorting = true;
			} );

			SearchTermsModified?.Invoke();
		}
		public HierarchyStep<Ttype> WrapInParent () {
			if ( parent is null ) {
				parent = CreateParent();
				OnParentCreated( parent );
				parent.OnLoadComplete += d => {
					parent.IsExpanded.Value = true;
				};
				parent.syncGroup = parentSyncGroup;
			}

			if ( Parent != parent && Parent != null )
				throw new InvalidOperationException( "Cannot wrap in parent if already inside another parent" );

			parent.AddChild( this );
			return parent;
		}
		/// <summary>
		/// Happens when a parent is added above this step. This is a good place to bind parents bindables to the childs bindables.
		/// You must call the base method.
		/// </summary>
		protected virtual void OnParentCreated ( HierarchyStep<Ttype> parent ) {
			parent.IsMultiselect.BindTo( IsMultiselect );
			parent.MultiselectSelection.BindTo( MultiselectSelection );
			Hovered += c => parent.Hovered?.Invoke( c );
			HoverLost += c => parent.HoverLost?.Invoke( c );
			Selected += c => parent.Selected?.Invoke( c );
			SearchTermsModified += () => parent.SearchTermsModified?.Invoke();
		}
		public HierarchyStep<Ttype> SplitFromParent () {
			if ( Parent is null ) {
				return this;
			}

			if ( parent != Parent )
				throw new InvalidOperationException( "Cannot split from a parent because the parent is not a part of the hierarchy" );

			parent.RemoveChild( Value );
			return this;
		}
		protected abstract HierarchyStep<Ttype> CreateChild ( Ttype value );
		protected abstract HierarchyStep<Ttype> CreateParent ();

		bool needsSorting = true;
		protected override void Update () {
			base.Update();
			Width = Parent.DrawWidth - Margin.Right - Margin.Left;
		}

		protected override void UpdateAfterChildren () {
			base.UpdateAfterChildren();
			if ( IsExpanded.Value && needsSorting ) {
				needsSorting = false;
				sortChildren();
			}
		}

		public abstract string Title { get; }
		public virtual Drawable CreateIcon () => null;
		protected OsuTextFlowContainer Label => button.Label;
		protected Drawable Icon {
			get => button.Icon;
			set => button.Icon = value;
		}
		protected virtual void ValueChanged ( Ttype old, Ttype @new ) { Label.Text = Title; }

		public IEnumerable<IFilterable> FilterableChildren => IsExpanded.Value ? children.Values : Array.Empty<IFilterable>();

		bool matchingFilter = false;
		public bool MatchingFilter {
			set {
				matchingFilter = value;
				updateVisibility();
			}
		}
		bool filteringActive = false;
		public bool FilteringActive {
			set {
				filteringActive = value;
				updateVisibility();
			}
		}

		bool visible = true;
		void updateVisibility () {
			if ( filteringActive ) {
				if ( matchingFilter ) {
					if ( !visible ) show();
				}
				else {
					if ( visible ) hide();
				}
			}
			else {
				if ( !visible ) show();
			}
		}
		void show () {
			visible = true;
			this.ScaleTo( new Vector2( 1, 1 ), 100, Easing.Out );
		}
		void hide () {
			visible = false;
			this.ScaleTo( new Vector2( 1, 0 ), 100, Easing.Out );
		}

		public HierarchyStep<Ttype> FindStep ( Ttype value ) {
			if ( !typeof( Ttype ).IsValueType ) {
				return findStepByref( value );
			}
			else if ( value is IEquatable<Ttype> equatable ) {
				return findStepEquatable( value );
			}
			else {
				throw new InvalidOperationException( "The given step type is not equatable and cant be compared by refernece" );
			}
		}

		HierarchyStep<Ttype> findStepEquatable ( Ttype value ) {
			if ( (value as IEquatable<Ttype>).Equals( Value ) ) return this;
			if ( !IsExpanded.Value ) return null;
			foreach ( var i in Children.Values ) {
				var val = i.findStepByref( value );
				if ( val is not null ) return val;
			}
			return null;
		}

		HierarchyStep<Ttype> findStepByref ( Ttype value ) {
			if ( object.ReferenceEquals( value, Value ) ) return this;
			if ( !IsExpanded.Value ) return null;
			foreach ( var i in Children.Values ) {
				var val = i.findStepByref( value );
				if ( val is not null ) return val;
			}
			return null;
		}

		public virtual IEnumerable<string> FilterTerms => Title.Yield();
	}

	public class HierarchyButton : CalmOsuAnimatedButton {
		public readonly OsuTextFlowContainer Label;
		public Drawable Icon {
			get {
				return iconContainer.Children.FirstOrDefault();
			}
			set {
				iconContainer.Clear( false );
				if ( value is null ) return;
				iconContainer.Add( value );
			}
		}

		private Container iconContainer;
		public HierarchyButton ( string title, Drawable icon, AutoScrollContainerSyncGroup syncGroup ) {
			AutoSizeAxes = Axes.Y;

			FillFlowContainer content;
			Add( content = new FillFlowContainer {
				AutoSizeAxes = Axes.Y,
				Direction = FillDirection.Horizontal,
				Margin = new MarginPadding { Left = 4 }
			} );
			content.OnUpdate += d => d.Width = this.DrawWidth - 90;

			content.Add( iconContainer = new Container {
				Height = 16,
				AutoSizeAxes = Axes.X
			} );

			Icon = icon;

			AutoScrollContainer scroll;
			content.Add( scroll = new AutoScrollContainer( syncGroup ) {
				Height = 16
			} );
			scroll.OnUpdate += d => d.Width = content.DrawWidth - ( icon?.DrawWidth ?? 0 );

			scroll.Add( Label = new OsuTextFlowContainer {
				AutoSizeAxes = Axes.Both,
				Anchor = Anchor.CentreLeft,
				Origin = Anchor.CentreLeft,
				TextAnchor = Anchor.CentreLeft,
				Margin = new MarginPadding { Left = 4 }
			} );
			Label.AddText( title );
		}

		protected override void Update () {
			base.Update();
			Width = Parent.DrawWidth - Margin.Right - Margin.Left;
		}
	}

	public class SelectionNub : Nub {
		public SelectionNub () {
			AddInternal( new HoverSounds() );
		}

		protected override bool OnHover ( HoverEvent e ) {
			Glowing = true;
			Expanded = true;
			return base.OnHover( e );
		}

		protected override void OnHoverLost ( HoverLostEvent e ) {
			base.OnHoverLost( e );
			Expanded = false;
			Glowing = false;
		}

		protected override bool OnClick ( ClickEvent e ) {
			Current.Value = !Current.Value;
			if ( Current.Value )
				sampleChecked.Play();
			else
				sampleUnchecked.Play();
			return base.OnClick( e );
		}

		Sample sampleChecked;
		Sample sampleUnchecked;
		[BackgroundDependencyLoader]
		private void load ( AudioManager audio ) {
			sampleChecked = audio.Samples.Get( @"UI/check-on" );
			sampleUnchecked = audio.Samples.Get( @"UI/check-off" );
		}
	}
}
