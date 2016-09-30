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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.IO;

//using MP.Base;


#pragma warning disable 219


namespace Utilities {

	/////////////////////////////////////////////////////////////////////////////

	public class CmdLineParams {

		// ******
		protected List<string>	cmds = new List<string>();
		protected List<string>	values = new List<string>();

		// ******
		public int Count { get; private set; }


		/////////////////////////////////////////////////////////////////////////////

		private void Add( string argument, string value )
		{
			cmds.Add( string.IsNullOrEmpty(argument) ? (string) null : argument.Trim() );
			values.Add( value );
			++Count;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Remove( int index )
		{
			// ******
			if( index < 0 || index >= cmds.Count ) {
				throw new Exception( "CmdLineParams.Remove was passed an illegal index" );
			}

			// ******
			cmds.RemoveRange( index, 1 );
			values.RemoveRange( index, 1 );
			--Count;
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool GetCommand( int index, out string cmd, out string value )
		{
			// ******
			if( index < 0 || index >= Count ) {
				cmd = null;
				value = null;
				return false;
			}

			// ******
			cmd = cmds [ index ];
			value = values [ index ];

			// ******
			//
			// true if there is a command string
			//
			return null != cmd;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string RemoveOuterQuotes( string str )
		{
			// ******
			if( string.IsNullOrEmpty( str ) ) {
				return string.Empty;
			}

			// ******
			if( str.Length > 1 ) {
				if( '"' == str [ 0 ] && '"' == str [ str.Length - 1 ] ) {
					return str.Substring( 1, str.Length - 2 );
				}
			}

			// ******
			return str;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static bool GetValuePair( string value, out string lhs, out string rhs )
		{
			// ******
			lhs = rhs = string.Empty;

			// ******
			string [] parts = null;
			try {
				parts = value.Split( new char [] { '=', ':' } );
			}
			catch {
				return false;
			}

			// ******
			if( 0 == parts.Length || string.IsNullOrEmpty( parts [ 0 ] ) ) {
				return false;
			}
			else if( 1 == parts.Length ) {
				lhs = RemoveOuterQuotes( parts [ 0 ] );
				return true;
			}

			// ******
			lhs = RemoveOuterQuotes( parts [ 0 ] );
			rhs = RemoveOuterQuotes( parts [ 1 ] );
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////

		private static string envSubstStr = @"(^|[^[])(\[[^[\]]*?\])";
		private static Regex envSubstRegex =  new Regex( envSubstStr );


		/////////////////////////////////////////////////////////////////////////////

		private string EnvSubstMatch( Match match )
		{
			int iFirst;

			// ******
			string chLeadingStr = match.Value [ 0 ].ToString();
			if( "[" == chLeadingStr ) {
				//
				// '[' indicates it was found at the start of the string and we have
				// no preceeding character that we need to return
				//
				iFirst = 1;
				chLeadingStr = string.Empty;
			}
			else {
				iFirst = 2;
			}

			// ******
			string envStr = match.Value.Substring( iFirst, match.Value.Length - (1 == iFirst ? 2 : 3) );

			// ******
			if( string.IsNullOrEmpty( envStr ) ) {
				return chLeadingStr;
			}

			// ******
			envStr = Environment.GetEnvironmentVariable( envStr );

			// ******
			return chLeadingStr + envStr;
		}


		/////////////////////////////////////////////////////////////////////////////

		private string EnvSubst( string text )
		{
			return envSubstRegex.Replace( text, match => { return EnvSubstMatch( match ); } );
		}


		/////////////////////////////////////////////////////////////////////////////

		//public void AppendCommands( string [] args, bool fromResponseFile, bool throwOnError )
		public void AppendCommands( IList<string> args, bool fromResponseFile, bool throwOnError )
		{
			// ******
			foreach( string str in args ) {
				if( string.IsNullOrEmpty( str ) ) {
					continue;
				}

				// ******
				if( fromResponseFile && char.IsWhiteSpace( str [ 0 ] ) ) {
					continue;
				}

				// ******
				string arg = str.Trim();
				if( 0 == arg.Length || '#' == arg [ 0 ] ) {
					continue;
				}

				// ******
				arg = EnvSubst( arg );

				// ******
				char ch = arg [ 0 ];

				if( '-' == ch || '/' == ch ) {
					if( 1 == arg.Length ) {
						if( throwOnError ) {
							throw new Exception( "spurious '-' or '/' on command line" );
						}
					}

					// ******
					//int pos = arg.IndexOfAny( new char [] {':'} );
					int pos = arg.IndexOf( ':' );
					if( pos > 0 ) {
						Add( arg.Substring( 1, pos - 1 ), RemoveOuterQuotes( arg.Substring( 1 + pos ) ) );
					}
					else {
						//
						// an empty value indicates this is a command without a value
						//
						Add( arg.Substring( 1 ), null );
					}

				}
				else if( '@' == ch ) {
					//
					// note: its our callers responsibility to have set the current directory to some
					// default where to look for response files if they don't have a path attached
					//
					string fileName = RemoveOuterQuotes( arg.Substring( 1 ).Trim() );
					if( fileName.Length > 0 && File.Exists( fileName ) ) {
						LoadResponseFile( fileName, false );
					}
				}
				else {
					//
					// an empty argument signifies a stand alone value on the command line
					//
					Add( null, RemoveOuterQuotes( arg ) );
				}

			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool LoadResponseFile( string path, bool throwOnError )
		{
			// ******
			if( string.IsNullOrEmpty( path ) ) {
				return false;
			}

			// ******
			if( !File.Exists( path ) ) {
				return false;
			}

			// ******
			string [] strArgs = File.ReadAllLines( path );
			//for( int i = 0; i < strArgs.Length; i++ ) {
			//	if( !string.IsNullOrEmpty( strArgs [ i ] ) ) {
			//		strArgs [ i ] = strArgs [ i ].Trim();
			//	}
			//}

			var curDir = Path.IsPathRooted( path ) ? Directory.GetCurrentDirectory() : null;
			if( null != curDir ) {
				Directory.SetCurrentDirectory( Path.GetDirectoryName( path ) );
			}
			
			AppendCommands( strArgs, true, throwOnError );

			if( null != curDir ) {
				Directory.SetCurrentDirectory( curDir );
			}


			// ******
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool AddRange( CmdLineParams p )
		{
			// ******
			if( null == p ) {
				return false;
			}

			// ******
			cmds.AddRange( p.cmds );
			values.AddRange( p.values );
			Count += p.Count;

			// ******
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool InsertRange( int index, CmdLineParams p )
		{
			// ******
			if( null == p || index < 0 ) {
				return false;
			}
			else if( index >= Count ) {
				return AddRange( p );
			}

			// ******
			cmds.InsertRange( index, p.cmds );
			values.InsertRange( index, p.values );
			Count += p.Count;

			// ******
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////

		//public CmdLineParams( string [] args, bool fromResponseFile, bool throwOnError )
		public CmdLineParams( IList<string> args, bool fromResponseFile, bool throwOnError )
		{
			AppendCommands( args, fromResponseFile, throwOnError );
		}


		/////////////////////////////////////////////////////////////////////////////

		public CmdLineParams( string path, bool throwOnError )
		{
			LoadResponseFile( path, throwOnError );
		}


		/////////////////////////////////////////////////////////////////////////////

		public CmdLineParams()
		{
		}
	}


	//		/////////////////////////////////////////////////////////////////////////////
	//
	//		static void Process( CmdLineParams cmds )
	//		{
	//		char	lastCmd = ' ';
	//		
	//			// ******
	//			stdErr.WriteLine( "cryptotool" );
	//			stdErr.WriteLine( "" );
	//			
	//			// ******
	//			if( 0 == cmds.Count ) {
	//				stdErr.WriteLine( "-k:filename                generate key pair into filename" );
	//				stdErr.WriteLine( "-u:filename                use key pair file for command that require a public or private key" );
	//				stdErr.WriteLine( "-h1:filename               hash and sign filen using public key provided by -u:filename" );
	//				stdErr.WriteLine( "-ddn:mm/dd/yyyy,mm/dd/yyyy calculate and out days between start (first) and end (second) date" );
	//			}
	//
	//			// ******
	//			for( int iCmd = 0; iCmd < cmds.Count; iCmd++ ) {
	//				string cmd;
	//				string value;
	//				
	//				if( cmds.GetCommand(iCmd, out cmd, out value) ) {
	//					//
	//					// a command
	//					//
	//					switch( cmd ) {
	//						//
	//						// generate a key pair
	//						//
	//						// -k:filenamepath.ext
	//						//
	//						case "k":
	//							NewKeys( value );
	//							break;
	//							
	//						//
	//						// use a key file for commands that require a public or private key, may be
	//						// supplied multiple times
	//						//
	//						// -u:filenamepathe.ext
	//						//
	//						case "u":
	//							UseKeyFile( value );
	//							break;
	//							
	//						//
	//						// hash and sign binary file data, method number one - requires 2 key set
	//						//
	//						//		private key to sign data with (public key must live in appliation)
	//						//
	//						// -h1:filenamepath.ext
	//						//
	//						case "h1":
	//							HashAndSign1( value );
	//							break;
	//
	//						//
	//						// verify hash of file (where both signature and data live)
	//						//
	//						// -v1:finenamepath.ext
	//						//
	//						case "v1":
	//							VerifyHashAndSign1( value );
	//							break;
	//							
	//						//
	//						// drop dead days number
	//						//
	//						// -ddn:startDate,dropDeadDate		where dates are: MM/DD/YYYY, eg -ddn:4/1/1953,4/3/1953
	//						//
	//						case "ddn":
	//							DropDeadDays( value );
	//							break;
	//														
	//						default:
	//							TerminalError( "unknown command '{0}'", cmd );
	//							break;
	//					}
	//				}
	//				else {
	//					//
	//					// a value
	//					//
	//				}
	//				
	//			}
	//		}
	//	
	//
	//		/////////////////////////////////////////////////////////////////////////////
	//
	//		static void Main( string[] args )
	//		{
	//			// ******
	//			CmdLineParams cmds = new CmdLineParams( args, false );
	//		
	//			// ******
	//			Process( cmds );
	//		}
	//		
	//	}


}