using System;

namespace osu.XR {
	public class ReadonlyIndexer<Tin,Tout> {
		Func<Tin, Tout> getter;

		public ReadonlyIndexer ( Func<Tin, Tout> getter ) {
			this.getter = getter;
		}

		public Tout this[ Tin index ]
			=> getter( index );
	}
}
