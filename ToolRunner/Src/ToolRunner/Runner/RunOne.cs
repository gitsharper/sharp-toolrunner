using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Utilities;

using static Utilities.FileAndDirectoryHelpers;

namespace ToolRunner {

	/////////////////////////////////////////////////////////////////////////////

	public partial class RunOne {

		// ******
		protected readonly IExtSvcProvider service;
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

		//string FixSrcNameInStdErr( string stdErr, string tempInputName, string actualInputName )
		//{
		//	// ******
		//	string result = stdErr;
		//	while( true ) {
		//		if( result.Contains( tempInputName ) ) {
		//			result = result.Replace( tempInputName, actualInputName );
		//		}
		//		else {
		//			return result;
		//		}
		//	}
		//}


		/////////////////////////////////////////////////////////////////////////////

		protected bool TryHandleInternalCmd( string cmdIn, ERCommand erCmd, InputFile input, out string result )
		{
			// ******
			result = string.Empty;
			var cmd = cmdIn?.Trim().ToLower();

			// ******
			if( !string.IsNullOrEmpty( cmd ) ) {
				switch( cmd.Trim().ToLower() ) {
					case "replace":
						return new Replace( service, input.PathOnly, erCmd ).Process( input, out result );
				}
			}

			// ******
			service.NotifyOfErrors( ErrorType.PrepError, $"unknown internal command: {cmdIn}" );
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////

		// this all assumes one file in, one file out

		// multi file

		// need directory out NOT file name (multi file output)


		public bool Run( ERCommand erCmd, out string result )
		{
			// ******
			result = null;

			if( Debugger.IsAttached && erCmd.DebugBreak ) {
				Debugger.Break();
			}

			// ******
			var execuitable = erCmd.ExecutableName;
			if( string.IsNullOrWhiteSpace( execuitable ) ) {
				service.NotifyOfErrors( ErrorType.PrepError, "no command name was provided" );
				return false;
			}

			// ******
			const string INTERNAL = "internal:";
			if( execuitable.StartsWith( INTERNAL, StringComparison.OrdinalIgnoreCase ) ) {
				return TryHandleInternalCmd( execuitable.Substring( INTERNAL.Length ), erCmd, input, out result );
			}

			// ******
			var cmdExt = Path.GetExtension( execuitable );
			//
			// should look this up in the registery
			//
			if( ".cmd" == cmdExt || ".bat" == cmdExt ) {
				execuitable = "cmd.exe";
			}
			else if( ".btm" == cmdExt ) {
				execuitable = "tcc.exe";
			}

			// ******
			bool success = false;
			var bareFileName = input.NameWithoutExt;
			var cmdInputFile = GetTempFilePath( bareFileName );
			var cmdOutputFile = GetFilePath( bareFileName, ".out.txt" );

			try {
				var proccessedInputText = ProcessOptions( input.Content );
				var commandLine = erCmd.SubstituteAndCombine( input, cmdInputFile, cmdOutputFile, new List<string> { } );

				// ******
				WriteNmpInputFile( cmdInputFile, proccessedInputText );

				// ******
				var run = new External { };
				var exeResult = run.Execute( execuitable, false, commandLine, 10000 );

				if( exeResult.Success ) {
					var exitCode = exeResult.ExitCode;

					if( ActionEnum.RunOnlyIgnoreExitCode == erCmd.ActionEnum ) {
						//
						// don't expect or handle output file, no exit code check
						//
						success = true;
					}
					else if( ActionEnum.RunOnly == erCmd.ActionEnum ) {
						//
						// don't expect or handle output file, do check exit code
						//
						success = 0 == exitCode;
					}
					else {
						//
						// expect output, do check exit code
						//
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

							//if( success ) {
								service.NotifyOfSuccess();
							//}
						}
						else {
							//
							// failure
							//
							//var errStr = FixSrcNameInStdErr( exeResult.StdErr, cmdInputFile, input.NameWithPath );
							var errStr = exeResult.StdErr.Replace( cmdInputFile, input.NameWithPath );
							service.NotifyOfErrors( ErrorType.Error, errStr );
							success = false;
						}
					}

				}
				else {
					//
					// failed to execute cmd
					//
					service.NotifyOfErrors( ErrorType.FailureToExecute, execuitable );
					success = false;
				}

			}
			finally {

				if( !KeepTempFiles ) {
					ResultFilesHelper.DeleteFiles( new List<string> { cmdInputFile, cmdOutputFile } );
				}

			}


			// ******
			return success;
		}


		/////////////////////////////////////////////////////////////////////////////

		public RunOne( InputFile inputFile, IExtSvcProvider service )
		{
			this.input = inputFile;
			this.service = service;
		}

	}
}
