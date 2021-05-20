using Newtonsoft.Json.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.XR.Input.Custom.Persistence;
using osuTK;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#nullable enable

namespace osu.XR.Input.Custom {
	public abstract class CustomBinding {
		public abstract string Name { get; }
		public abstract CustomBindingHandler CreateHandler ();

		public abstract object CreateSaveData ( SaveDataContext context );
		public abstract void Load ( JToken data, SaveDataContext context );

		public event Action? SettingsChanged;
		protected void OnSettingsChanged () {
			SettingsChanged?.Invoke();
		}
	}
	public abstract class CustomBindingHandler : CompositeDrawable {
		public readonly CustomBinding Backing;

		public CustomBindingHandler ( CustomBinding backing ) {
			Backing = backing;
		}
		public abstract CustomBindingDrawable CreateSettingsDrawable ();

		protected virtual InjectedInput? Input => Parent as InjectedInput ?? ( Parent as CustomBindingHandler )?.Input;
		protected void TriggerPress ( object action ) {
			Input?.TriggerPress( action, this );
		}
		protected void TriggerRelease ( object action ) {
			Input?.TriggerRelease( action, this );
		}
		protected void MoveTo ( Vector2 position, bool isNormalized = false ) {
			Input?.MoveTo( position, isNormalized );
		}
		protected void MoveBy ( Vector2 position, bool isNormalized = false ) {
			Input?.MoveBy( position, isNormalized );
		}
	}
	public abstract class CustomBindingDrawable : CompositeDrawable {
		public readonly CustomBindingHandler Handler;
		public CustomBinding Backing => Handler.Backing;

		public CustomBindingDrawable ( CustomBindingHandler handler ) {
			Handler = handler;
		}
	}

	public abstract class CompositeCustomBinding : CustomBinding {
		public CompositeCustomBinding () {
			Children.BindCollectionChanged( (_,e) => {
				if ( e.Action == NotifyCollectionChangedAction.Add ) {
					if ( e.NewItems is null ) return;
					foreach ( CustomBinding i in e.NewItems ) {
						i.SettingsChanged += OnSettingsChanged;
					}
				}
				OnSettingsChanged();
			} );
		}

		public readonly BindableList<CustomBinding> Children = new();
		public override abstract CompositeCustomBindingHandler CreateHandler ();
		public sealed override object CreateSaveData ( SaveDataContext context )
			=> CreateSaveData( Children.ToDictionary( x => x, x => x.CreateSaveData( context ) ) );
		protected abstract object CreateSaveData ( Dictionary<CustomBinding,object> childrenData );
	}
	public abstract class CompositeCustomBindingHandler : CustomBindingHandler {
		new public CompositeCustomBinding Backing => (CompositeCustomBinding)base.Backing;
		public readonly BindableList<CustomBinding> Sources = new();
		public readonly Dictionary<CustomBinding, CustomBindingHandler> ChildrenMap = new();
		public readonly BindableList<CustomBindingHandler> Children = new();

		public CompositeCustomBindingHandler ( CompositeCustomBinding backing ) : base( backing ) {
			Sources.BindTo( backing.Children );
		}

		protected override void LoadComplete () {
			base.LoadComplete();

			Sources.BindCollectionChanged( ( _, e ) => {
				if ( e.Action == NotifyCollectionChangedAction.Add ) {
					if ( e.NewItems is null ) return;
					foreach ( CustomBinding i in e.NewItems ) {
						var handler = i.CreateHandler();
						AddInternal( handler );
						ChildrenMap.Add( i, handler );
						Children.Add( handler );
						OnChildAdded( handler );
					}
				}
				else {
					if ( e.OldItems is null ) return;
					foreach ( CustomBinding i in e.OldItems ) {
						ChildrenMap.Remove( i, out var handler );
						Children.Remove( handler! );
						RemoveInternal( handler! );
						handler!.Dispose();
						OnChildRemoved( handler );
					}
				}
			}, true );
		}

		public override abstract CompositeCustomBindingDrawable CreateSettingsDrawable ();
		protected virtual void OnChildAdded ( CustomBindingHandler child ) { }
		protected virtual void OnChildRemoved ( CustomBindingHandler child ) { }
	}
	public abstract class CompositeCustomBindingDrawable : CustomBindingDrawable {
		new public CompositeCustomBindingHandler Handler => (CompositeCustomBindingHandler)base.Handler;
		new public CompositeCustomBinding Backing => (CompositeCustomBinding)base.Backing;

		public readonly Dictionary<CustomBindingHandler, Drawable> ChildrenMap = new();
		public readonly BindableList<CustomBindingHandler> HandlerChildren = new();

		protected CompositeCustomBindingDrawable ( CompositeCustomBindingHandler handelr ) : base( handelr ) {
			HandlerChildren.BindTo( Handler.Children );
		}

		protected override void LoadComplete () {
			base.LoadComplete();

			HandlerChildren.BindCollectionChanged( ( _, e ) => {
				if ( e.Action == NotifyCollectionChangedAction.Add ) {
					if ( e.NewItems is null ) return;
					foreach ( CustomBindingHandler i in e.NewItems ) {
						var settingDrawable = i.CreateSettingsDrawable();
						var drawable = CreateDrawable( settingDrawable );
						ChildrenMap.Add( i, drawable );
						AddDrawable( drawable, i );
					}
				}
				else {
					if ( e.OldItems is null ) return;
					foreach ( CustomBindingHandler i in e.OldItems ) {
						ChildrenMap.Remove( i, out var drawable );
						RemoveDrawable( drawable!, i );
					}
				}
			}, true );
		}

		protected virtual Drawable CreateDrawable ( CustomBindingDrawable settingDrawable ) => settingDrawable;
		protected abstract void AddDrawable ( Drawable drawable, CustomBindingHandler source );
		protected abstract void RemoveDrawable ( Drawable drawable, CustomBindingHandler source );
	}
}
