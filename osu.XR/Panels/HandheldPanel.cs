﻿using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.XR.Components.Groups;
using osu.XR.Components.Panels;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Panels {
	/// <summary>
	/// A panel with a standard size
	/// </summary>
	public abstract class HandheldPanel : FlatPanel, IHasName, IHasIcon {
		public Drawable Content { get; private set; }
		public readonly Bindable<bool> IsVisibleBindable = new();
		public HandheldPanel () {
			Content = CreateContent();
			Content.Size = new Vector2( 400, 500 );

			PanelAutoScaleAxes = Axes.X;
			AutosizeBoth();
			PanelHeight = 0.5;
			Source.Add( Content );
		}

		protected override void Update () {
			base.Update();
			IsVisibleBindable.Value = IsVisible = Content.IsPresent;
		}

		protected abstract Drawable CreateContent ();

		public abstract string DisplayName { get; }
		public abstract Drawable CreateIcon ();
	}

	public abstract class HandheldPanel<T> : HandheldPanel where T : Drawable {
		new public T Content => (T)base.Content;
		protected override abstract T CreateContent ();
	}
}