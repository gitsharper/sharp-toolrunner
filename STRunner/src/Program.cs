using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ToolRunner;
using Utilities;

namespace STRunner {

	/////////////////////////////////////////////////////////////////////////////
	//
	//
	//
	/////////////////////////////////////////////////////////////////////////////

	//[Fact]
	//public void TypescriptTest1()
	//{
	//	// ******
	//	var dataFilePath = GetDataFilePath( "Typescript\\raytracer.ts" );
	//	var runner = new Runner( dataFilePath ) { };
	//	Assert.NotNull( runner );
	//
	//	string result;
	//	string outExt;
	//	runner.Generate( out result, out outExt );
	//	Assert.True( TestNormalizeString( result ).StartsWith( "varVector=(function(){" ) );
	//}

	/////////////////////////////////////////////////////////////////////////////

	// project dir
	// solution dir


	/////////////////////////////////////////////////////////////////////////////

	partial class Program {

		static TextWriter StdErr = Console.Error;
		static TextWriter StdOut = Console.Out;

		/////////////////////////////////////////////////////////////////////////////
		//
		//
		//
		/////////////////////////////////////////////////////////////////////////////

		static public void WriteMessage( string str )
		{
			StdOut.WriteLine( str );
		}


		/////////////////////////////////////////////////////////////////////////////

		static public void WriteStdError( string str )
		{
			StdErr.WriteLine( str );
		}


		/////////////////////////////////////////////////////////////////////////////

		static public void Die( string str )
		{
			WriteStdError( str );
			if( Debugger.IsAttached ) {
				Debugger.Break();
			}
			Environment.Exit( 1 );
		}


		/////////////////////////////////////////////////////////////////////////////

		static public void Goodby( string str )
		{
			// ******
			WriteMessage( str );
			if( Debugger.IsAttached ) {
				Debugger.Break();
			}
			Environment.Exit( 0 );
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		//
		//
		/////////////////////////////////////////////////////////////////////////////

		static string FixFilePath( string value )
		{
			// ******
			if( string.IsNullOrWhiteSpace( value ) ) {
				return string.Empty;
			}

			var fn = value.Trim().Trim( new char [] { '\'', '"' } ).Trim();
			if( string.IsNullOrWhiteSpace( fn ) ) {
				return string.Empty;
			}

			// ******
			if( '.' == fn [ 0 ] ) {
				//
				// not this ASSUMES that strunner is executing in the directory to which
				// the caller wants the '.' to reference because there is no way strunner
				// can figure it out
				//
				fn = Path.Combine( Directory.GetCurrentDirectory(), fn.Substring( 1 ) );
			}

			// ******
			return fn;
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// processes commands to extract tokens and secrets
		/// </summary>
		/// <param name="cmds"></param>
		/// <returns></returns>

		static void ProcessCmds( CmdLineParams cmds )
		{
			// ******
			var fileList = new List<string> { };
			var solutionFldr = string.Empty;
			var projectFldr = string.Empty;


			// ******
			for( int i = 0; i < cmds.Count; i += 1 ) {
				string cmd;
				string value;
				bool isCmd = cmds.GetCommand( i, out cmd, out value );

				// ******
				if( !isCmd ) {
					//
					// the file to process
					//
					var fn = FixFilePath( value );
					if( !string.IsNullOrWhiteSpace( fn ) ) {
						fileList.Add( fn );
					}
				}
				else {
					switch( cmd ) {
						case "p":
						case "project":
							break;

						case "s":
						case "solution":
							break;

						default:
							Die( $"error: unknown command \"/{cmd}:{value}\"" );
							break;
					}
				}
			}

			// ******
			if( 0 == fileList.Count ) {
				Die( "error: no file name provided" );
			}
			else if( fileList.Count > 1 ) {
				Die( "error: more than one file name provided" );
			}
			else {
				var filePath = fileList.First();
				if( !File.Exists( filePath ) ) {
					Die( $"error: could not locate file \"{filePath}\"" );
				}


				//	// ******
				//	var dataFilePath = GetDataFilePath( "Typescript\\raytracer.ts" );
				//	var runner = new Runner( dataFilePath ) { };
				//	Assert.NotNull( runner );
				//
				//	string result;
				//	string outExt;
				//	runner.Generate( out result, out outExt );
				//	Assert.True( TestNormalizeString( result ).StartsWith( "varVector=(function(){" ) );

				try {
					var runner = new Runner( filePath ) { };

					string result;
					string outExt;
					runner.Generate( out result, out outExt );
				}
				catch( Exception ex ) {
					throw;
				}
			}


			// ******
			return;
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		//
		//
		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// processes commands to extract tokens and secrets
		/// </summary>
		/// <param name="cmds"></param>
		/// <returns></returns>

		static void PreProcessCmds( CmdLineParams cmds )
		{
			// ******
			for( int i = 0; i < cmds.Count; /* incrementing counter handled below */ ) {
				string cmd;
				string value;
				bool isCmd = cmds.GetCommand( i, out cmd, out value );

				// ******
				bool cmdHandled = true;
				switch( cmd ) {
					//case "pin":
					//	accessData.ForcePinAuth = true;
					//	break;
					//
					//case "consumerToken":
					//	if( !haveConsumerTokenAndSecret ) {
					//		accessData.ConsumerToken = value;
					//	}
					//	break;
					//
					//case "consumerSecret":
					//	if( !haveConsumerTokenAndSecret ) {
					//		accessData.ConsumerSecret = value;
					//	}
					//	break;
					//
					//case "userAccessToken":
					//	accessData.UserAccessToken = value;
					//	break;
					//
					//case "userAccessSecret":
					//	accessData.UserAccessSecret = value;
					//	break;

					default:
						cmdHandled = false;
						break;
				}

				// ******
				if( cmdHandled ) {
					//
					// remove the command we processed
					//
					cmds.Remove( i );
				}
				else {
					//
					// next command
					//
					i++;
				}
			}

			// ******
			return;
		}


		/////////////////////////////////////////////////////////////////////////////

		// C:\Users\joe\AppData\Roaming\cltweetie\cltweetie.config
		const string AppName = "strunner";

		static int Main( string [] args )
		{
			// ******
			//
			// load config file(s) if they can be found
			//
			// note: duplicates of the same command will be processed in the order in which they are
			// found, this allows a scenario where the "last config processed wins"; however since they
			// are each processed (new commands do not replace earliers commands with the same name) there
			// might be side effects from that processing - as of this time that is not at issue
			//
			var configFileName = AppName + ".config";
			var appDataFolder = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ), AppName );
			var personalFolder = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.Personal ), AppName );

			var cmds = new CmdLineParams( args, false, false );

			//
			// try in app folder
			//
			cmds.LoadResponseFile( Path.Combine( LibInfo.CodeBase, configFileName ), false );

			//
			// then appication data folder (roaming)
			//
			cmds.LoadResponseFile( Path.Combine( appDataFolder, configFileName ), false );

			//
			// personal folder (documents)
			//
			cmds.LoadResponseFile( Path.Combine( personalFolder, configFileName ), false );

			// ******
			try {
				PreProcessCmds( cmds );
				ProcessCmds( cmds );

				//Console.ReadKey();
				if( Debugger.IsAttached ) {
					Debugger.Break();
				}

				return 0;
			}
			catch( Exception ex ) {
				Console.WriteLine( ex.ToString() );

				//Console.ReadKey();
				if( Debugger.IsAttached ) {
					Debugger.Break();
				}

				return 1;
			}
		}
	}





}
