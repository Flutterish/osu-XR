using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osuTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Input.Custom {
	public abstract class CustomInput : CompositeDrawable {
		new public abstract string Name { get; }
		private Drawable settingDrawable;
		public Drawable SettingDrawable => settingDrawable ??= CreateSettingDrawable();

		[Resolved]
		protected OsuGameXr game { get; private set; }

		/// <summary>
		/// Creates a setting subsection for this input. This is created only once per instance.
		/// </summary>
		protected virtual Drawable CreateSettingDrawable () {
			var x = new OsuTextFlowContainer {
				Anchor = Anchor.Centre,
				Origin = Anchor.Centre,
				AutoSizeAxes = Axes.Both,
				TextAnchor = Anchor.Centre,
				Margin = new MarginPadding( 6 )
			};

			x.AddText( Name, x => x.Font = OsuFont.GetFont( weight: FontWeight.Bold ) );
			x.AddText( $" {((Name.EndsWith('s'))?"are":"is")} not configurable" );

			return x;
		}

		/// <summary>
		/// Creates a component which actually manages the binding.
		/// You should create one for backing fields and bind the new one's to them here.
		/// </summary>
		public abstract CustomRulesetInputBindingHandler CreateHandler ();
	}

	public abstract class CustomRulesetInputBindingHandler : CompositeDrawable {
		protected void TriggerPress ( object rulesetAction ) {

		}

		protected void TriggerRelease ( object rulesetAction ) {

		}

		protected void MoveTo ( Vector2 position ) {

		}
	}

	public class RulesetActionBinding {
		public readonly BindableBool IsActive = new();
		public readonly Bindable<object> RulesetAction = new();

		public RulesetActionBinding () {
			IsActive.BindValueChanged( v => {
				if ( RulesetAction.Value == null ) return;

				if ( v.NewValue )
					Press?.Invoke( RulesetAction.Value );
				else
					Release?.Invoke( RulesetAction.Value );
			} );

			RulesetAction.BindValueChanged( v => {
				if ( IsActive.Value ) {
					if ( v.OldValue != null )
						Release?.Invoke( v.OldValue );

					if ( v.NewValue != null )
						Press?.Invoke( v.NewValue );
				}
			} );
		}

		public event Action<object> Press;
		public event Action<object> Release;
	}
}
