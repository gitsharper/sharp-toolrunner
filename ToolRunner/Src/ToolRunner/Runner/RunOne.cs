using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Utilities;

namespace ToolRunner {

	/////////////////////////////////////////////////////////////////////////////

	public partial class RunOne {

		// ******
		protected readonly IExtSvcProvider reporter;
		InputFile input;

		/////////////////////////////////////////////////////////////////////////////

		//string OutputFileName
		//{
		//	get
		//	{
		//		return Path.Combine( input.PathOnly, Path.GetFileNameWithoutExtension( input.NameWithPath ) ) + input.Extension;
		//	}
		//}

		/////////////////////////////////////////////////////////////////////////////

		string GetFilePath( string bareFileName, string fileExt )
		{
			var fileName = string.IsNullOrEmpty( fileExt ) ? bareFileName : bareFileName + fileExt;
			return Path.Combine( input.PathOnly, fileName );
		}


		/////////////////////////////////////////////////////////////////////////////

		static void WriteNmpInputFile( string path, string contents )
		{
			File.WriteAllText( path, contents );
		}


		/////////////////////////////////////////////////////////////////////////////

		static string ReadNmpOutputFile( string path )
		{
			return File.ReadAllText( path );
		}


		/////////////////////////////////////////////////////////////////////////////

		string FixSrcNameInStdErr( string stdErr, string tempInputName, string actualInputName )
		{
			// ******
			string result = stdErr;
			while( true ) {
				if( result.Contains( tempInputName ) ) {
					result = result.Replace( tempInputName, actualInputName );
				}
				else {
					return result;
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected bool TryHandleInternalCmd( string cmdIn, ERCommand erCmd, string content, out string result )
		{
			// ******
			result = string.Empty;
			var cmd = cmdIn?.Trim().ToLower();

			// ******
			if( !string.IsNullOrEmpty( cmd ) ) {
				switch( cmd.Trim().ToLower() ) {
					case "replace":
						return new Replace( reporter, input.PathOnly, erCmd ).Process( content, out result );
				}
			}

			// ******
			reporter.NotifyOfErrors( ErrorType.PrepError, $"unknown internal command: {cmdIn}" );
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool Run( ERCommand erCmd, out string result )
		{
			// ******
			result = string.Empty;

			if( Debugger.IsAttached && erCmd.DebugBreak ) {
				Debugger.Break();
			}

			// ******
			var cmd = erCmd.ExecutableName;
			if( string.IsNullOrWhiteSpace( cmd ) ) {
				reporter.NotifyOfErrors( ErrorType.PrepError, "no command name was provided" );
				return false;
			}

			// ******
			const string INTERNAL = "internal:";
			if( cmd.StartsWith( INTERNAL, StringComparison.OrdinalIgnoreCase ) ) {
				return TryHandleInternalCmd( cmd.Substring( INTERNAL.Length ), erCmd, input.Content, out result );
			}

			// ******
			var cmdExt = Path.GetExtension( cmd );
			//
			// should look this up in the registery
			//
			if( ".cmd" == cmdExt || ".bat" == cmdExt ) {
				cmd = "cmd.exe";
			}
			else if( ".btm" == cmdExt ) {
				cmd = "tcc.exe";
			}

			// ******
			bool success = false;
			//var bareFileName = "tool-runner";
			var bareFileName = input.NameWithoutExt;
			var cmdInputFile = GetFilePath( bareFileName, ".in" + input.Extension );
			var cmdOutputFile = GetFilePath( bareFileName, ".out.txt" );

			// ******

			//
			// need to get extra args from input file during this call
			//

			var proccessedInputText = ProcessOptions( input.Content );
			var commandLine = erCmd.SubstituteAndCombine( input, cmdInputFile, cmdOutputFile, new List<string> { } );

			// ******
			WriteNmpInputFile( cmdInputFile, proccessedInputText );

			// ******
			var run = new External { };
			var exeResult = run.Execute( cmd, false, commandLine, 10000 );

			if( exeResult.Success ) {
				var exitCode = exeResult.ExitCode;

				if( ActionEnum.RunOnlyIgnoreExitCode == erCmd.ActionEnum ) {
					success = true;
				}
				else if( ActionEnum.RunOnly == erCmd.ActionEnum ) {
					success = 0 == exitCode;
				}
				else {

					if( 0 == exitCode ) {
						//
						// success
						//
						success = true;

						if( erCmd.ReadStdout ) {
							result = exeResult.StdOut;
						}
						else if( File.Exists( cmdOutputFile ) ) {
							result = ReadNmpOutputFile( cmdOutputFile );
						}
						else {
							result = null;
						}

						if( success ) {
							reporter.NotifyOfSuccess();
						}
					}
					else {
						//
						// failure
						//
						var errStr = FixSrcNameInStdErr( exeResult.StdErr, cmdInputFile, input.NameWithPath );
						reporter.NotifyOfErrors( ErrorType.Error, errStr );
						success = false;
					}
				}

			}
			else {
				//
				// failed to execute cmd
				//
				reporter.NotifyOfErrors( ErrorType.FailureToExecute, cmd );
				success = false;
			}


			// ******
			if( !KeepTempFiles ) {
				DeleteFiles( new List<string> { cmdInputFile, cmdOutputFile } );
			}

			// ******
			return success;
		}


		/////////////////////////////////////////////////////////////////////////////

		public RunOne( InputFile inputFile, IExtSvcProvider reporter )
		{
			this.input = inputFile;
			this.reporter = reporter;
		}

	}
}
