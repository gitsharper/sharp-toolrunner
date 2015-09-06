using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using CustomToolBase;

namespace ToolRunner {

	/*
	ParseError: Unrecognised input in D:\Work\New 2014\Projects\gitsharper\Sharp-ToolRunner\UnitTests\Src\TestData\Less\tool-runner.in.txt on line 3, column 2:
	2 	background : blue;
	3 	@ @
	4 }
	
	ParseError: 
	Unrecognised input in D:\Work\New 2014\Projects\gitsharper\Sharp-ToolRunner\UnitTests\Src\TestData\Less\tool-runner.in.txt on 
	line 3, 
	column 2:
		
	err type :
	err message with embeded/trailing line and column	:
	block of code surrounding the error
		
		*/


	/////////////////////////////////////////////////////////////////////////////

	public class LessErrorSplitter {

		protected ErrorItem errorItem = new ErrorItem { };

		protected bool isOneBase = true;

		protected string filePath;
		protected string errorString;
		protected string activeString;
		protected bool oneBasedTextCoords;


		/////////////////////////////////////////////////////////////////////////////

		public bool Split()
		{
			const string regExSplit = @"(?s)\A(.*?):(.*?)\n(.*$)";
			const string regExLineCol = @"line\s*?(\d*?)[, ]*?column\s*?(\d*?)\s*?:";

			// ******                         
			Regex rx = new Regex( regExSplit );
			Match match = rx.Match( errorString );

			// ******                         
			//
			// only require first 3 matches
			if( !match.Success ) {
				return false;
			}

			// ******
			//
			// group[0] represents the overall capture
			// group[1] error type
			// group[2] error explanation
			// group[3] file listing where error occurs
			//
			var groups = match.Groups;
			//var errorType = groups [ 1 ].Value.Trim();
			var errorMsg = groups [ 2 ].Value.Trim();
			//var remainder = groups [ 3 ].Value;

			// ... runner.in.txt on line 3, column 2:
			Regex rx2 = new Regex( regExLineCol );
			Match match2 = rx2.Match( errorMsg );
			if( !match2.Success ) {
				return false;
			}
			var groups2 = match2.Groups;

			int line;
			if( !int.TryParse( groups2 [ 1 ].Value, out line ) ) {
				return false;
			}

			int col;
			if( !int.TryParse( groups2 [ 2 ].Value, out col ) ) {
				return false;
			}

			errorItem = new ErrorItem( false, -1 ) {
				FileName = filePath,
				ErrorText = errorString,
				Line = line,
				Column = col
			};

			// ******
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected LessErrorSplitter( string filePathIn, string errStrIn )
		{
			filePath = filePathIn;
			errorString = errStrIn.Trim();
			activeString = errorString;
			oneBasedTextCoords = true;  //oneBasedTextCoordsIn;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static List<ErrorItemBase> Split( string filePathIn, string errStrIn )
		{
			// ******
			var errs = new List<ErrorItemBase> { };

			// ******
			var splitter = new LessErrorSplitter( filePathIn, errStrIn );
			if( !splitter.Split() ) {
				errs.Add( new ErrorItemBase( filePathIn, "unknown 'less' error format" ) );
				errs.Add( DefaultErrorSplitter.Split( filePathIn, errStrIn ) );
			}
			else {
				errs.Add( splitter.errorItem );
			}

			// ******
			return errs;
		}


	}
}
