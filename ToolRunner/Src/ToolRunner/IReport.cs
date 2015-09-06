using System;
using System.Collections.Generic;
using System.Linq;
using CustomToolBase;

namespace ToolRunner {

	/////////////////////////////////////////////////////////////////////////////

	public interface _IReport {

		//VSInterface ivs { get; }

		void SendError( string errorText, int lineIn, int column );
		void SendWarning( string fileName, string message, int line, int column );
		void SendMessage( string fileName, string message, int line, int column );
		void NotifyOfErrors( ErrorType et, string error );
		void NotifyOfSuccess();

	}
}
