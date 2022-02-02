using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.XR.Materials;
using osu.Game.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Tests {
	public class OsuTestBrowser3D : OsuTestBrowser {
		[NotNull, MaybeNull]
		private DependencyContainer dependencies;
		[NotNull, MaybeNull]
		private MaterialManager MaterialManager;

		[BackgroundDependencyLoader]
		private void load () {
			Resources.AddStore( new DllResourceStore( osu.Framework.XR.Resources.ResourceAssembly ) );

			var resources = new ResourceStore<byte[]>();
			resources.AddStore( new NamespacedResourceStore<byte[]>( Resources, @"Shaders" ) );
			resources.AddStore( new NamespacedResourceStore<byte[]>( Resources, @"Shaders/Materials" ) );
			MaterialManager = new MaterialManager( resources );
			dependencies.CacheAs( MaterialManager );

			Child = new SafeAreaContainer {
				RelativeSizeAxes = Axes.Both,
				Child = new DrawSizePreservingFillContainer {
					Children = new Drawable[]
					{
						new TestBrowser(),
						new CursorContainer(),
					},
				}
			};
		}

		public override void SetHost ( GameHost host ) {
			base.SetHost( host );
			host.Window.CursorState |= CursorState.Hidden;
		}

		protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) =>
			dependencies = new DependencyContainer( base.CreateChildDependencies( parent ) );
	}
}
