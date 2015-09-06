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

//namespace CustomToolUtilities {

namespace ToolRunner {


    /*

		Error: while handling a .net object; exception: unable to locate an implementation of "Replace" where arguments match (or can be converted from):
"" (values passed: )
Tried matching:
String.Replace( Char, Char )
String.Replace( String, String );
Line=7, Col=2, File "D:\Work\New 2014\Projects\gitsharper\Sharp-ToolRunner\UnitTests\Src\TestData\Nmp4\NmpError1.nmp4";
------------------------------------------------------------
Error in:
------------------------------------------------------------
#fredIs.Replace()
------------------------------------------------------------
Context:
------------------------------------------------------------
#.popBuffer() 
#fredIs.Replace()
------------------------------------------------------------
Invocation stack:
  #fredIs
Invocation stack ends

		
		*/


    /////////////////////////////////////////////////////////////////////////////

    public class NmpErrorSplitter {

        protected ErrorItem errorItem = new ErrorItem { };

        protected bool isOneBase = true;

        protected string filePath;
        protected string errorString;
        protected string activeString;
        protected bool oneBasedTextCoords;


        /////////////////////////////////////////////////////////////////////////////

        //public ErrorItem ParseNmpError( string errStr )
        //{
        //	// ******
        //	var errorParts = errStr.Split( ';' );
        //	if( errorParts.Length < 4 ) {
        //		return null;
        //	}

        //	// ******
        //	var locationInfo = errorParts [ 2 ];
        //	var parts = locationInfo.Split( ',' );
        //	if( 3 != parts.Length ) {
        //		return null;
        //	}

        //	// ******
        //	try {

        //		// ******
        //		ErrorItem errorItem = new ErrorItem { };

        //		errorItem.Line = Int32.Parse( parts [ 0 ].Substring( parts [ 0 ].IndexOf( '=' ) + 1 ) );
        //		errorItem.Column = Int32.Parse( parts [ 1 ].Substring( parts [ 1 ].IndexOf( '=' ) + 1 ) );

        //		if( parts [ 2 ].Contains( "File" ) ) {
        //			var fn = parts [ 2 ].Substring( parts [ 2 ].IndexOf( '"' ) );
        //			errorItem.FileName = fn.Substring( 1, fn.Length - 2 );
        //		}
        //		else {
        //			errorItem.FileName = parts [ 2 ];
        //		}

        //		// ******
        //		errorItem.ErrorText = errStr;

        //		// ******
        //		return errorItem;
        //	}
        //	catch {
        //		return null;
        //	}
        //}


        /////////////////////////////////////////////////////////////////////////////

        public bool Split()
        {
            const string regExLineCol = @"Line=(\d*?)[, ]*?Col=(\d*?)\s*?,";

            // ******                         
            Regex rx = new Regex( regExLineCol );
            Match match2 = rx.Match( errorString );
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

        protected NmpErrorSplitter( string filePathIn, string errStrIn )
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
            var splitter = new NmpErrorSplitter( filePathIn, errStrIn );
            if( !splitter.Split() ) {
                errs.Add( new ErrorItemBase( filePathIn, "unknown 'nmp4' error format" ) );
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
