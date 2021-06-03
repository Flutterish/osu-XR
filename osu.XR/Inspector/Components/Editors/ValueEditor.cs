using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Inspector.Components.Editors {
	public abstract class ValueEditor : FillFlowContainer {
		public readonly Bindable<object> Current = new();

		public ValueEditor () {
			RelativeSizeAxes = Axes.X;
			AutoSizeAxes = Axes.Y;
			Direction = FillDirection.Vertical;
		}

		public void OnException ( Exception e ) {
			if ( e is TargetInvocationException )
				e = e.InnerException;
			HandleException( e );
		}

		protected virtual void HandleException ( Exception e ) {
			OpenVR.NET.Events.Exception( e, e.Message );
		}

		private static readonly Dictionary<Type, Func<object, ValueEditor>> editors = new() {
			[ typeof( int ) ] = 
			v => new TextfieldEditor<int>( s => {
				if ( int.TryParse( s, out var v ) )
					return (true, v);
				return (false, 0);
			}, (int)v ),

			[ typeof( float ) ] =
			v => new TextfieldEditor<float>( s => {
				if ( float.TryParse( s, out var v ) )
					return (true, v);
				return (false, 0);
			}, (float)v ),

			[ typeof( double ) ] =
			v => new TextfieldEditor<double>( s => {
				if ( double.TryParse( s, out var v ) )
					return (true, v);
				return (false, 0);
			}, (double)v ),

			[ typeof( string ) ] = 
			v => new TextfieldEditor<string>( s => (true, s), v as string ),

			[ typeof( bool ) ] =
			v => new ToggleEditor( (bool)v ),

			[ typeof( Vector2 ) ] =
			v => new Vector2Editor( (Vector2)v ),

			[ typeof( Vector3 ) ] =
			v => new Vector3Editor( (Vector3)v ),

			[ typeof( Color4 ) ] =
			v => new Color4Editor( (Color4)v )
		};

		public static bool HasEditorFor ( Type t ) {
			return t.IsEnum || editors.ContainsKey( t );
		}
		public static ValueEditor GetEditorFor ( Type t, object value = default ) {
			return t.IsEnum
				? Activator.CreateInstance( typeof( EnumEditor<> ).MakeGenericType( t ), new object[] { value } ) as ValueEditor
				: editors[ t ]( value );
		}
	}

	public abstract class ValueEditor<T> : ValueEditor {
		public ValueEditor ( T defaultValue = default ) {
			Current = new Bindable<T>( defaultValue );
			Current.BindValueChanged( v => base.Current.Value = v.NewValue );
		}

		new public readonly Bindable<T> Current;
	}
}
