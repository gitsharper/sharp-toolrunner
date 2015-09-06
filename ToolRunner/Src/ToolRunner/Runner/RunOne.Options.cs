#region License
// 
// Author: Joe McLain <nmp.developer@outlook.com>
// Copyright (c) 2013, Joe McLain and Digital Writing
// 
// Licensed under Eclipse Public License, Version 1.0 (EPL-1.0)
// See the file LICENSE.txt for details.
// 
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

using Utilities;
using CustomToolBase;


// http://code.msdn.microsoft.com/SingleFileGenerator/Project/ProjectRss.aspx

namespace ToolRunner {


	/////////////////////////////////////////////////////////////////////////////

	partial class RunOne {

		public bool KeepTempFiles = false;


		/////////////////////////////////////////////////////////////////////////////

		List<string> GetOptions( ref string source )
		{
			const string STARTS_CMD_ARG = "//#";
			//const string STARTS_RUNNER_ARG = "#:@";

			// ******
			var options = new List<string> { };
			var lines = source.Split( new string [] { "\r\n", "\n\r", "\r", "\n" }, StringSplitOptions.None );

			for( int i = 0; i < lines.Length; i += 1 ) {
				var value = lines [ i ].Trim();

				if( value.StartsWith( STARTS_CMD_ARG ) ) {
					//
					// starts with correct sequence, first replace the line
					// with an empty string so Nmp won't see it
					//
					lines [ i ] = string.Empty;

					// ******
					//
					// remove leadin text, and trim remainder
					//
					var option = value.Substring( STARTS_CMD_ARG.Length ).Trim();
					options.Add( option.Substring( 1 ) );
				}
				//else if( value.StartsWith( STARTS_RUNNER_ARG ) ) {
				//	//
				//	// wait until we need some args before figuring this out
				//	//
				//}
			}

			// ******
			source = string.Join( "\n", lines );
			return options;
		}

		/////////////////////////////////////////////////////////////////////////////

		string ProcessOptions( string source )
		{
			// ******
			var options = GetOptions( ref source );

			if( options.Count > 0 ) {
				foreach( var option in options ) {
					switch( option.ToLower() ) {
						case "keeptempfiles":
							KeepTempFiles = true;
							break;
					}
				}
			}

			// ******
			return source;
		}

	}

}
