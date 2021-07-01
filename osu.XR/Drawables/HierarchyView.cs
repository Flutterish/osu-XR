using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osu.XR.Inspector.Components;
using osuTK.Graphics.ES11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Drawables {
	public abstract class HierarchyView<Tstep,Ttype> : SearchContainer where Tstep : HierarchyStep<Ttype> {
		public event Action<Tstep> StepHovered;
		public event Action<Tstep> StepHoverLost;
		public event Action<Tstep> StepSelected;

		public HierarchyView ( Ttype value ) {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Direction = FillDirection.Vertical;

			top = CreateStep( value );
			parent = CreateParent( top );
			if ( parent is not null ) {
				parent.Margin = new MarginPadding { Left = 10, Right = 15 };
				parent.CanBeExpanded.Value = false;
				Add( parent );

				parent.Selected += parentSelected;
			}
			Add( top );
			top.Selected += childSelected;
			top.Hovered += childHovered;
			top.HoverLost += childHoverLost;
			top.Margin = new MarginPadding { Horizontal = 15 };
		}

		private void childHoverLost ( HierarchyStep<Ttype> obj ) {
			StepHoverLost?.Invoke( (Tstep)obj );
		}

		private void childHovered ( HierarchyStep<Ttype> obj ) {
			StepHovered?.Invoke( (Tstep)obj );
		}

		private void childSelected ( HierarchyStep<Ttype> obj ) {
			if ( obj == top ) return;

			Remove( parent );
			Remove( top );
			top.Selected -= childSelected;
			top.Hovered -= childHovered;
			top.HoverLost -= childHoverLost;

			obj.SplitFromParent();
			top = (Tstep)obj;
			parent = CreateParent( top );
			if ( parent is not null ) {
				parent.Margin = new MarginPadding { Left = 10, Right = 15 };
				parent.CanBeExpanded.Value = false;
				Add( parent );

				parent.Selected += parentSelected;
			}
			Add( top );
			top.Selected += childSelected;
			top.Hovered += childHovered;
			top.HoverLost += childHoverLost;
			top.Margin = new MarginPadding { Horizontal = 15 };
		}

		private void parentSelected ( HierarchyStep<Ttype> obj ) {
			Remove( parent );
			Remove( top );
			top.Selected -= childSelected;
			top.Hovered -= childHovered;
			top.HoverLost -= childHoverLost;

			top = (Tstep)top.WrapInParent();
			top.SplitFromParent();
			parent = CreateParent( top );
			if ( parent is not null ) {
				parent.Margin = new MarginPadding { Left = 10, Right = 15 };
				parent.CanBeExpanded.Value = false;
				Add( parent );

				parent.Selected += parentSelected;
			}
			Add( top );
			top.Selected += childSelected;
			top.Hovered += childHovered;
			top.HoverLost += childHoverLost;
			top.Margin = new MarginPadding { Horizontal = 15 };
		}

		private Tstep top;
		private Tstep parent;
		protected abstract Tstep CreateStep ( Ttype value );
		protected abstract Tstep CreateParent ( Tstep top );
	}

	public abstract class HierarchyStep<Ttype> : SearchContainer {
		public event Action<HierarchyStep<Ttype>> Hovered;
		public event Action<HierarchyStep<Ttype>> HoverLost;
		public event Action<HierarchyStep<Ttype>> Selected;

		public Ttype Value { get; private set; }
		HierarchyStep<Ttype> parent;
		AutoScrollContainerSyncGroup parentSyncGroup = new();
		AutoScrollContainerSyncGroup syncGroup = new();

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
		}

		protected override void LoadComplete () {
			base.LoadComplete();
			Add( button = new HierarchyButton( Title, CreateIcon(), parentSyncGroup ) {
				Margin = new MarginPadding { Left = 5 },
				Hovered = () => Hovered?.Invoke( this ),
				HoverLost = () => HoverLost?.Invoke( this ),
				Action = () => Selected?.Invoke( this )
			} );
			button.Add( toggleButton );

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
					}
				}
				else {
					dropdownChevron.FadeTo( 1f, 100 );
					foreach ( var (k, c) in children ) {
						Remove( c );
					}
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
		CalmOsuAnimatedButton toggleButton;
		SpriteIcon dropdownChevron;
		public readonly BindableBool IsExpanded = new( false );
		private Dictionary<Ttype, HierarchyStep<Ttype>> children = new();
		new public IReadOnlyDictionary<Ttype, HierarchyStep<Ttype>> Children => children;
		private void AddChild ( HierarchyStep<Ttype> child ) {
			if ( children.ContainsKey( child.Value ) ) {
				RemoveChild( child.Value );
			}

			child.Hovered += c => Hovered?.Invoke( c );
			child.HoverLost += c => HoverLost?.Invoke( c );
			child.Selected += c => Selected?.Invoke( c );
			child.parentSyncGroup = syncGroup;
			child.Margin = new MarginPadding { Left = 5 };
			children.Add( child.Value, child );
			if ( IsExpanded.Value ) {
				Add( child );
				needsSorting = true;
			}

			if ( children.Count == 1 && CanBeExpanded.Value ) {
				toggleButton.FadeIn( 120, Easing.Out );
			}
		}
		protected void AddChild ( Ttype value ) {
			if ( children.ContainsKey( value ) ) return;

			var child = CreateChild( value );
			child.parent = this;
			AddChild( child );
		}
		protected void RemoveChild ( Ttype value ) {
			children.Remove( value, out var child );
			if ( IsExpanded.Value ) {
				Remove( child );
			}

			if ( children.Count == 0 && CanBeExpanded.Value ) {
				toggleButton.FadeOut( 120, Easing.Out );
			}
		}
		protected void ChangeChildValue ( Ttype old, Ttype @new ) {
			children.Remove( old, out var child );
			children.Add( @new, child );

			child.Value = @new;
			child.Schedule( () => {
				child.ValueChanged( old, @new );
				needsSorting = true;
			} );
		}
		public HierarchyStep<Ttype> WrapInParent () {
			if ( parent is null ) {
				parent = CreateParent();
				parent.OnLoadComplete += d => {
					parent.IsExpanded.Value = true;
				};
				parent.syncGroup = parentSyncGroup;
			}

			if ( Parent != parent ) {
				if ( Parent != null ) throw new InvalidOperationException( "Cannot wrap in parent if already inside another parent" );

				parent.AddChild( this );
				return parent;
			}
			else {
				parent.AddChild( this );
				return parent;
			}
		}
		public void SplitFromParent () {
			if ( Parent is null ) {
				return;
			}

			if ( parent != Parent ) {
				throw new InvalidOperationException( "Cannot split from a parent because the parent is not a part of the hierarchy" );
			}
			else {
				parent.RemoveChild( Value );
			}
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

		private HierarchyButton button;
		protected OsuTextFlowContainer Label => button.Label;
		protected Drawable Icon {
			get => button.Icon;
			set => button.Icon = value;
		}
		protected virtual void ValueChanged ( Ttype old, Ttype @new ) { Label.Text = Title; }
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
}
