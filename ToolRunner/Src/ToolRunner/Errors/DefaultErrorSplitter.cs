using System;
using System.Collections.Generic;
using System.Linq;
using CustomToolBase;

namespace ToolRunner {

	public static class DefaultErrorSplitter {

		/////////////////////////////////////////////////////////////////////////////

		public static ErrorItemBase Split( string filePathIn, string errStrIn )
		{
			var errorItem = new ErrorItemBase {
				FileName = filePathIn,
				ErrorText = errStrIn
			};

			return errorItem;
		}
	}
}
