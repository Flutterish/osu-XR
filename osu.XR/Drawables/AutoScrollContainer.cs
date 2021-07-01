using Microsoft.CodeAnalysis.CSharp.Syntax;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Drawables {
	public class AutoScrollContainerSyncGroup : AutoScrollContainerSyncGroup<Drawable> {
		public AutoScrollContainerSyncGroup ( Direction direction = Direction.Horizontal ) { }
	}
	public class AutoScrollContainerSyncGroup<T> : Component where T : Drawable {
		public readonly List<AutoScrollContainer<T>> containers = new();

		public double BeginDelay = 2000;
		public double EndDelay = 2000;
		public double ScrollSpeed = 60;
		public double Current { get; private set; }
		public readonly Direction Direction;

		enum Stage {
			Start,
			WaitingAfterStart,
			Scrolling,
			WaitingBeforeEnd,
			End
		}
		Stage stage = Stage.Start;

		public AutoScrollContainerSyncGroup ( Direction direction = Direction.Horizontal ) {
			Direction = direction;
		}

		protected override void Update () {
			base.Update();

			if ( containers.Count == 0 ) {
				stage = Stage.Start;
				Current = 0;
				return;
			}

			double contentSize = containers.Max( x => x.ContentSize );
			double avaiableSize = containers.Max( x => x.AvaiableSize );
			double maskedSize = contentSize - avaiableSize;

			if ( maskedSize <= 0 ) {
				stage = Stage.Start;
				Current = 0;
				return;
			}

			switch ( stage ) {
				case Stage.Start:
					stage = Stage.WaitingAfterStart;
					this.Delay( BeginDelay ).Then().Schedule( () => {
						stage = Stage.Scrolling;
					} );
					break;
				case Stage.Scrolling:
					Current = Math.Clamp( Current + Time.Elapsed / 1000 * ScrollSpeed, 0, maskedSize );
					if ( Current == maskedSize ) {
						stage = Stage.WaitingBeforeEnd;
						this.Delay( EndDelay ).Then().Schedule( () => {
							stage = Stage.End;
						} );
					}
					break;
				case Stage.WaitingBeforeEnd:
					if ( Current < maskedSize ) {
						FinishTransforms();
						stage = Stage.Scrolling;
					}
					break;
				case Stage.End:
					Current = Math.Clamp( Current + Time.Elapsed / 1000 * ScrollSpeed * -3, 0, maskedSize );
					if ( Current == 0 ) {
						stage = Stage.Start;
					}
					break;
			}
		}
	}

	public class AutoScrollContainer : AutoScrollContainer<Drawable> {
		public AutoScrollContainer ( Direction direction = Direction.Horizontal ) : base( direction ) { }
		public AutoScrollContainer ( AutoScrollContainerSyncGroup<Drawable> syncGroup ) : base( syncGroup ) { }
	}

	public class AutoScrollContainer<T> : Container<T> where T : Drawable {
		public double BeginDelay { get => syncGroup.BeginDelay; set => syncGroup.BeginDelay = value; }
		public double EndDelay { get => syncGroup.EndDelay; set => syncGroup.EndDelay = value; }
		public double ScrollSpeed { get => syncGroup.ScrollSpeed; set => syncGroup.ScrollSpeed = value; }
		public Direction Direction => syncGroup.Direction;

		FillFlowContainer<T> size;
		protected override Container<T> Content => size;
		AutoScrollContainerSyncGroup<T> syncGroup;
		public AutoScrollContainer ( Direction direction = Direction.Horizontal ) : this( new AutoScrollContainerSyncGroup<T>( direction ) ) {
			AddInternal( syncGroup );
		}
		public AutoScrollContainer ( AutoScrollContainerSyncGroup<T> syncGroup ) {
			if ( syncGroup is null ) {
				AddInternal( syncGroup = new AutoScrollContainerSyncGroup<T>( Direction.Horizontal ) );
			}
			this.syncGroup = syncGroup;
			syncGroup?.containers.Add( this );
			AddInternal( size = new FillFlowContainer<T> {
				AutoSizeAxes = Axes.Both,
				Direction = syncGroup.Direction is Direction.Horizontal ? FillDirection.Horizontal : FillDirection.Vertical
			} );
			Masking = true;
		}
		public double ContentSize => syncGroup.Direction is Direction.Horizontal ? size.Size.X : size.Size.Y;
		public double AvaiableSize => syncGroup.Direction is Direction.Horizontal ? DrawSize.X : DrawSize.Y;
		protected override void Update () {
			base.Update();

			double maskedSize = ContentSize - AvaiableSize;
			double current = Math.Max( Math.Min( syncGroup.Current, maskedSize ), 0 );
			if ( syncGroup.Direction is Direction.Horizontal )
				size.X = -(float)current;
			else
				size.Y = -(float)current;
		}
	}
}
