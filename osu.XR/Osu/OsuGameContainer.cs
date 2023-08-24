using osu.Framework;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Framework.XR.Graphics.Panels;
using osu.Game;
using osu.XR.Graphics.VirtualReality;

namespace osu.XR.Osu;

/// <summary>
/// A container for <see cref="OsuGame"/> capable of capturing cached dependencies and reloading the game
/// </summary>
public partial class OsuGameContainer : CompositeDrawable {
	public readonly OsuDependencies OsuDependencies = new();
	public VirtualOsuGameHost VirtualGameHost { get; private set; } = null!;
	OsuGame osu = new();

	protected override IReadOnlyDependencyContainer CreateChildDependencies ( IReadOnlyDependencyContainer parent ) {
		var deps = new DependencyContainer( parent );
		parent.TryGet<VrKeyboardInputSource>( out var keyboard );
		VirtualGameHost = new( parent.Get<GameHost>(), keyboard );
		deps.CacheAs<GameHost>( VirtualGameHost );
		deps.CacheAs<Framework.Game>( osu );
		deps.CacheAs<OsuGameBase>( osu );
		deps.CacheAs<OsuGame>( osu );
		deps.CacheAs<TextInputSource>( VirtualGameHost.PublicCreateTextInput() );
		return base.CreateChildDependencies( deps );
	}

	[BackgroundDependencyLoader]
	private void load () {
		RelativeSizeAxes = Axes.Both;
		OsuDependencies.OsuGame.Value = osu;

		osu.SetHost( VirtualGameHost );
		AddInternal( osu );
	}
}

public class VirtualOsuGameHost : VirtualGameHost {
	public VirtualOsuGameHost ( GameHost parent, VrKeyboardInputSource? keyboard, string? name = null, HostOptions? options = null ) : base( parent, name, options ) {
		keyboardSource = keyboard;
	}

	TextInputSource baseSource = null!;
	VrKeyboardInputSource? keyboardSource;
	public TextInputSource PublicCreateTextInput () {
		return CreateTextInput();
	}

	protected override TextInputSource CreateTextInput () {
		baseSource = base.CreateTextInput();
		return keyboardSource == null ? baseSource : new MergedTextInputSource( new[] { keyboardSource, baseSource } );
	}


	public override Clipboard GetClipboard () {
		return base.GetClipboard();
	}

	class MergedTextInputSource : TextInputSource {
		TextInputSource[] sources;
		public MergedTextInputSource ( IEnumerable<TextInputSource> sources ) {
			this.sources = sources.ToArray();
			foreach ( var i in sources ) {
				i.OnTextInput += TriggerTextInput;
				i.OnImeResult += TriggerImeResult;
				i.OnImeComposition += TriggerImeComposition;
			}
		}

		protected override void ActivateTextInput ( bool allowIme ) {
			foreach ( var i in sources ) {
				i.Activate( allowIme );
			}
		}

		protected override void DeactivateTextInput () {
			foreach ( var i in sources ) {
				i.Deactivate();
			}
		}

		protected override void EnsureTextInputActivated ( bool allowIme ) {
			foreach ( var i in sources ) {
				i.EnsureActivated( allowIme );
			}
		}

		public override void SetImeRectangle ( RectangleF rectangle ) {
			foreach ( var i in sources ) {
				i.SetImeRectangle( rectangle );
			}
		}

		public override void ResetIme () {
			foreach ( var i in sources ) {
				i.ResetIme();
			}
		}
	}
}