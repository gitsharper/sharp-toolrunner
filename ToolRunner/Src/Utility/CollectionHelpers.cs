using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utilities {

	/////////////////////////////////////////////////////////////////////////////

	public static class CollectionHelpers {

		/////////////////////////////////////////////////////////////////////////////

		public static int CountWhile<T>( IEnumerable<T> items, Func<T, bool> predicate )
		{
			// ******
			int count = 0;
			foreach( var item in items ) {
				if( !predicate( item ) ) {
					return count;
				}
				count += 1;
			}

			// ******
			return items.Count();
		}

	}
}
