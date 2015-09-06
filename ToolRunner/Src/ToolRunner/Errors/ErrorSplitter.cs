using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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


	/////////////////////////////////////////////////////////////////////////////

	public class ErrorSplitter {

		/////////////////////////////////////////////////////////////////////////////

		public static List<ErrorItemBase> Split( string fileExt, string filePathIn, string errStrIn )
		{
			// ******
			var errs = new List<ErrorItemBase> { };

			var ext = fileExt?.ToLower() ?? string.Empty;
			if( '.' == ext.First() ) {
				ext = ext.Substring( 1 );
			}

			// ******
			if( "less" == ext ) {
				var lessErrs = LessErrorSplitter.Split( filePathIn, errStrIn );
				errs.AddRange( lessErrs );
			}
			else if( "nmp4" == ext ) {
				var lessErrs = NmpErrorSplitter.Split( filePathIn, errStrIn );
				errs.AddRange( lessErrs );
			}

			// ******
			//
			// default
			//
			if( 0 == errs.Count ) {
				errs.Add( DefaultErrorSplitter.Split( filePathIn, errStrIn ) );
			}

			// ******
			return errs;
		}


	}
}
