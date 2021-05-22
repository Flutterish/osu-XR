using OpenVR.NET;
using OpenVR.NET.Manifests;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using System;

namespace osu.XR.Input.Custom.Components {
	public class BoundComponent<T,Tvalue> : Drawable where T : ControllerInputComponent<Tvalue> where Tvalue : struct, IEquatable<Tvalue> {
		event System.Action onDispose;
		protected override void Dispose ( bool isDisposing ) {
			base.Dispose( isDisposing );

			onDispose?.Invoke();
			onDispose = null;
		}

		public readonly Bindable<Tvalue> Current = new();

		public BoundComponent ( XrAction action, Func<Controller,bool> predicate ) {
			bool found = false;

			void lookForValidController ( Controller controller ) {
				if ( found ) return;
				if ( !predicate( controller ) ) return;
				found = true;

				var comp = VR.GetControllerComponent<T>( action, controller );
				System.Action<ValueUpdatedEvent<Tvalue>> listener = v => {
					Current.Value = v.NewValue;
				};
				comp.BindValueChangedDetailed( listener, true );
				onDispose += () => comp.ValueChanged -= listener;
				VR.NewControllerAdded -= lookForValidController;
			}

			VR.BindComponentsLoaded( () => {
				VR.BindNewControllerAdded( lookForValidController, true );
				lookForValidController( null );
			} );
		}
	}

	public class BoundComponent<T, Tvalue, Tfinal> : Drawable where T : ControllerInputComponent<Tvalue> where Tvalue : struct, IEquatable<Tvalue> {
		event System.Action onDispose;
		protected override void Dispose ( bool isDisposing ) {
			base.Dispose( isDisposing );

			onDispose?.Invoke();
			onDispose = null;
		}

		public readonly Bindable<Tfinal> Current = new();

		public BoundComponent ( XrAction action, Func<Controller, bool> predicate, Func<Tvalue,Tfinal> converter ) {
			bool found = false;

			void lookForValidController ( Controller controller ) {
				if ( found ) return;
				if ( !predicate( controller ) ) return;
				found = true;

				var comp = VR.GetControllerComponent<T>( action, controller );
				System.Action<ValueUpdatedEvent<Tvalue>> listener = v => {
					Current.Value = converter( v.NewValue );
				};
				comp.BindValueChangedDetailed( listener, true );
				onDispose += () => comp.ValueChanged -= listener;
				VR.NewControllerAdded -= lookForValidController;
			}

			VR.BindComponentsLoaded( () => {
				VR.BindNewControllerAdded( lookForValidController, true );
				lookForValidController( null );
			} );
		}
	}
}
