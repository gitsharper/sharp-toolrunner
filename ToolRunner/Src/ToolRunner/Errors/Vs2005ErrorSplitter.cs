using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

using CustomToolBase;
using Utilities;


namespace ToolRunner {


	// an error: "error(6):  output directory is a file: d:\temp-files\tmp\XPathLexer.g4"
	// error(50): Hello.g4:6:0: syntax error: missing COLON at 'bg' while matching a rule
	// error(50): Hello.g4:8:0: syntax error: mismatched input '<EOF>' expecting SEMI while matching a rule

	// error(50): Hello.g4:5:1: syntax error: mismatched input '<EOF>' expecting COLON while matching a rule
	// --------   -------- - -  ----------------------------------------------------------------------------

	// class StringHelpers


	/////////////////////////////////////////////////////////////////////////////

	public class Vs2005ErrorSplitter {

		protected ErrorItem errorItem = new ErrorItem { };

		protected string filePath;
		protected string fullString;
		protected string activeString;
		protected bool oneBasedTextCoords;


		/////////////////////////////////////////////////////////////////////////////

		void AdvanceActiveString( int count )
		{
			activeString = activeString.Substring( count );
		}


		/////////////////////////////////////////////////////////////////////////////

		//
		// "error(50): "
		//

		bool GetErrorCode()
		{
			const string OPEN_ERROR = "error(";

			// ******
			if( !activeString.StartsWith( OPEN_ERROR ) ) {
				return false;
			}
			AdvanceActiveString( OPEN_ERROR.Length );

			// ******
			int errNum;
			if( activeString.GetInt32( out errNum ) ) {
				errorItem.ErrorNumber = errNum;

				// charCount does not match what we think should be

				var charCount = activeString.CountWhile( c => ')' == c || ':' == c || char.IsDigit( c ) || char.IsWhiteSpace( c ) );
				AdvanceActiveString( charCount );
				return true;
			}

			// ******
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////

		//
		// "Hello.g4:6:0: "
		//

		bool GetFileAndPosition()
		{
			// ******
			errorItem.FileName = filePath;

			// ******
			var charCount = activeString.CountWhile( c => !char.IsWhiteSpace( c ) );
			var fileAndPositionStr = activeString.Substring( 0, charCount );

			// ******
			if( fileAndPositionStr.Contains( ':' ) ) {
				var items = fileAndPositionStr.Split( new char [] { ':' }, StringSplitOptions.RemoveEmptyEntries );
				if( items.Length > 0 ) {
					//errorItem.FileName = items [ 0 ];
					errorItem.Line = items.Length > 1 ? Convert.ToInt32( items [ 1 ] ) : -1;
					errorItem.Column = items.Length > 2 ? Convert.ToInt32( items [ 2 ] ) : -1;

					if( oneBasedTextCoords ) {
						//
						// visual studio assumes error lines/columns are zero based for the tool and
						// increases the line/col values to match the editors 1/1 first line and first
						// column - since the error we're parsing assumes a 1/1 first line and column as
						// well we need to decrement the error line/column
						//
						errorItem.Line -= 1;
						errorItem.Column -= 1;
					}

					// ******
					AdvanceActiveString( charCount );
					return true;
				}
			}

			// ******
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool Split()
		{
			// ******
			//
			// "error(nnn)"
			//
			if( !GetErrorCode() ) {
				return false;
			}

			if( GetFileAndPosition() ) {
				//
				// without ':' separators it's a non 'positional' error
				//
				//errorItem.FileName = Path.GetFileName( filePath );
			}
			//errorItem.FileName = filePath;

			//
			// remainder is error description
			//
			errorItem.ErrorText = string.Format("error {0}: {1}", errorItem.ErrorNumber, activeString.Trim() );

			// ******
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected Vs2005ErrorSplitter( string filePathIn, string errStrIn, bool oneBasedTextCoordsIn )
		{
			filePath = filePathIn;
			fullString = errStrIn.Trim();
			activeString = fullString;
			oneBasedTextCoords = oneBasedTextCoordsIn;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static ErrorItem Split( string filePathIn, string errStrIn, bool oneBasedTextCoords )
		{
			// ******
			var splitter = new Vs2005ErrorSplitter( filePathIn, errStrIn, oneBasedTextCoords );
			if( !splitter.Split() ) {
				return new ErrorItem { ParseSuccess = false, ErrorText = "unable to parse error message" };
			}
			else {
				return splitter.errorItem;
			}
		}


	}
}
