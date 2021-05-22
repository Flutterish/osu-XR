using Newtonsoft.Json.Linq;
using osu.Framework.Extensions;
using osu.XR.Drawables;
using System.Linq;

namespace osu.XR.Input.Custom.Persistence {
	public class SaveDataContext {
		RulesetVariantXrBindingsSubsection section;
		public SaveDataContext ( RulesetVariantXrBindingsSubsection section ) {
			this.section = section;
		}

		public int RulesetActionIndexOf ( object rulesetAction )
			=> section.RulesetActions.IndexOf( rulesetAction );

		public ActionBinding SaveActionBinding ( object action )
			=> new ActionBinding {
				ID = RulesetActionIndexOf( action ),
				Name = action?.GetDescription()
			};

		public object LoadActionBinding ( JToken token, string name ) {
			var id = ( token as JObject )[ name ].ToObject<ActionBinding>().ID;

			return section.RulesetActions.ElementAtOrDefault( id );
		}
	}
}
