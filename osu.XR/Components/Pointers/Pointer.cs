using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.XR.Components;
using osu.XR.Physics;
using osuTK.Graphics.ES20;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static osu.XR.Physics.Raycast;

namespace osu.XR.Components.Pointers {
	public abstract class Pointer : MeshedXrObject {
		[Resolved]
		protected PhysicsSystem PhysicsSystem { get; private set; }

		protected RaycastHit RaycastHit;
		private IHasCollider currentHit;
		/// <summary>
		/// The <see cref="IHasCollider"/> this pointer points at. Might be null.
		/// </summary>
		public IHasCollider CurrentHit {
			get => currentHit;
			protected set {
				if ( value != currentHit ) {
					var prev = currentHit;
					currentHit = value;
					if ( currentHit != null ) CurrentFocus = value;
					HitChanged?.Invoke( new( prev, currentHit ) );
				}
				if ( value != null ) NewHit?.Invoke( RaycastHit );
				else NoHit?.Invoke();
			}
		}

		private IHasCollider currentFocus;
		/// <summary>
		/// The last non-null <see cref="CurrentHit"/>.
		/// </summary>
		public IHasCollider CurrentFocus {
			get => currentFocus;
			private set {
				if ( value == currentFocus ) return;
				var prev = currentFocus;
				currentFocus = value;

				FocusChanged?.Invoke( new( prev, currentFocus ) );
			}
		}

		private bool wasActive = false;
		public abstract bool IsActive { get; }
		protected sealed override void Update () {
			base.Update();
			if ( !IsActive ) {
				if ( wasActive ) {
					RaycastHit = default;
					wasActive = false;

					var oldFocus = currentFocus;
					var oldHit = currentHit;
					currentHit = null;
					currentFocus = null;
					FocusChanged?.Invoke( new( oldFocus, null ) );
					HitChanged?.Invoke( new( oldHit, null ) );
					NewHit?.Invoke( default );
				}
				return;
			}

			wasActive = true;
			UpdatePointer();
		}

		protected abstract void UpdatePointer ();

		public event Action<ValueChangedEvent<IHasCollider>> FocusChanged;
		public event Action<ValueChangedEvent<IHasCollider>> HitChanged;

		public delegate void PointerUpdate ( RaycastHit hit );
		public event PointerUpdate NewHit;
		public event Action NoHit;
	}
}
