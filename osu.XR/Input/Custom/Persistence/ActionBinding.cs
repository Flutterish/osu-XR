using Newtonsoft.Json;
using osu.Framework.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.XR.Input.Custom.Persistence {
	public sealed class ActionBinding {
		[JsonProperty( Order = 1 )]
		public string Name { get; init; }
		[JsonProperty( Order = 2 )]
		public int ID { get; init; }
	}
}
