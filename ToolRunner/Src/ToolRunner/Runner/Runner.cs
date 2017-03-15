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
using System.Text.RegularExpressions;

using System.IO;

using Utilities;
using CustomToolBase;


// http://code.msdn.microsoft.com/SingleFileGenerator/Project/ProjectRss.aspx

namespace ToolRunner {

	//public enum ErrorType {
	//	SettingsError, PrepError, FailureToExecute, Error
	//}


	/////////////////////////////////////////////////////////////////////////////

	public class Runner {

		// ******
		protected string inputFileName;

		// ******
		protected InputFile initialInput;
		protected InputFile currentInput;
		//protected IVsGeneratorProgress progress;

		// ******
		public IExtSvcProvider Service { get; private set; } = null;

		// ******
		public bool SaveOutput { get; set; } = false;

		public string InputFileName { get { return inputFileName; } }



		/////////////////////////////////////////////////////////////////////////////

		ERules _rules;

		//
		// this is called by the debugger if it has to populate a display of
		// Runner properties (or refresh them) - so the retreval of rules will occur if 'inputFileName'
		// is not empty at the time
		//

		public ERules Rules
		{
			get {
				if( null == _rules ) {
					if( string.IsNullOrEmpty( inputFileName ) ) {
						throw new Exception( $"\"{nameof( inputFileName )}\" must be set before calling 'Rules'" );
					}
					_rules = new ERules( Path.GetDirectoryName( inputFileName ), Service );
				}
				return _rules;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		//public void SendError( string errorText, int lineIn, int column )
		//{
		//	//
		//	// using progress.GeneratorError() allows for a "clickable" error,
		//	// manipulating the error list with the routine in 'vsi' do not
		//	//
		//	if( null != progress ) {
		//		//
		//		// vs uses 0 based lines for reporting purposes
		//		//
		//		uint line = (uint) lineIn - 1;
		//		progress.GeneratorError( 0, 0, errorText, line, (uint) column );
		//	}
		//}


		/////////////////////////////////////////////////////////////////////////////

		//protected void SendToErrorList( VsErrorMsgType msgType, string fileName, string message, int line, int column )
		//{
		//	//
		//	// need alternate message sink for testing and when we create stand alone
		//	// version of this
		//	//

		//	if( null != Report ) {
		//		Report.SendToErrorList( msgType, fileName, message, line, column );
		//	}
		//}


		/////////////////////////////////////////////////////////////////////////////

		//public void SendWarning( string fileName, string message, int line, int column )
		//{
		//	SendToErrorList( VsErrorMsgType.Warning, fileName, message, line, column );
		//}


		/////////////////////////////////////////////////////////////////////////////

		//public void SendMessage( string fileName, string message, int line, int column )
		//{
		//	SendToErrorList( VsErrorMsgType.Warning, fileName, message, line, column );
		//}


		/////////////////////////////////////////////////////////////////////////////

		public void NotifyOfErrors( ErrorType et, string error )
		{
			// ******
			var errs = new List<ErrorItemBase> { };
			var srcExt = currentInput.FileExtWithoutDot;

			switch( et ) {
				case ErrorType.SettingsError:
					errs.Add( new ErrorItemBase( currentInput.PathOnly, $"could not find setting for \"{error}\"" ) );
					break;

				case ErrorType.PrepError:
					errs.Add( new ErrorItemBase( currentInput.PathOnly, error ) );
					break;

				case ErrorType.FailureToExecute:
					errs.Add( new ErrorItemBase( currentInput.PathOnly, $"unable to execute command \"{error}\"" ) );
					break;

				case ErrorType.Error:
					//
					// need to create error parsers for the "built-in"'s we know about
					//
					if( string.IsNullOrWhiteSpace( srcExt ) ) {
						throw new ArgumentNullException( nameof( srcExt ) );
					}
					errs.AddRange( ErrorSplitter.Split( srcExt, currentInput.NameWithPath, error ) );
					break;
			}

			// ******
			foreach( var e in errs ) {
				Service.SendError( inputFileName, e.ErrorText, e.Line, e.Column );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void NotifyOfSuccess()
		{
			if( null != Service ) {
				Service.ClearErrorList();
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected void WriteFile( InputFile input, string ext, string text )
		{
			var filePath = Path.Combine( input.PathOnly, input.NameWithoutExt ) + ext;
			File.WriteAllText( filePath, text );
		}


		/////////////////////////////////////////////////////////////////////////////

		string GetTempFilePath( string inputFileName )
		{
			// ******
			var tempDir = Path.GetTempPath();
			var fileName = Path.IsPathRooted( inputFileName ) ? Path.GetFileName( inputFileName ) : inputFileName;

			// ******
			return Path.Combine( tempDir, fileName );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected bool TryGenerate( ExtensionRule rule, out string result )
		{
			// ******
			result = string.Empty;
			var input = currentInput.Duplicate();

			// ******
			var backedUpFiles = new List<string> { };
			if( rule.SaveRestoreResultFiles ) {
				//
				// rule calls for files in rule.ResultFiles to be backed up, and restored if
				// there is a failure
				//
				var existingGeneratedFiles = ResultFilesHelper.DiscoverGeneratedFiles( input, rule.ResultFiles );
				if( existingGeneratedFiles.Count > 0 ) {
					backedUpFiles = ResultFilesHelper.RenameFilesForBackup( existingGeneratedFiles );
				}
			}

			// ******
			bool generateSuccess = true;
			foreach( var cmd in rule.Commands ) {

				// ******
				var run = new RunOne( input, Service );
				//
				// if 'success' is false we DO NOT know if the command could not be executed,
				// or if the commands exit code was other than zero - we just know Run failed
				//
				var success = run.Run( cmd, out result );
				if( !success ) {
					generateSuccess = false;
					break;
				}

				if( cmd.IgnoreResults() ) {
					//
					// RunOnlyIgnoreExitCode or RunOnly, any output is ignored so there
					// is no need to set new Content on input, or save the file
					//
					continue;
				}

				// ******
				//
				// if true it flags that the tool we executed did not return any results, Run()
				// leaves it up to us to handle	if it's an error, or not
				//
				if( string.IsNullOrEmpty( result ) ) {
					//
					// there was no output from the tool
					//
					// if cmd.AllowNoOutput is true (default is false) then the input
					// to the next tool, or the output from sharp-ToolRunner will be empty
					//
					result = string.Empty;
					if( !cmd.AllowNoOutput ) {
						Service.SendError( inputFileName, $"the command \"{cmd.ExecutableName}\" producted no output", -1, -1 );
						generateSuccess = false;
						break;
					}
				}

				// ******
				if( rule.SaveIntermediateFiles && cmd != rule.Commands.Last() ) {
					WriteFile( input, cmd.SaveExtension, result );
				}

				// ******
				input.SetContent( result );
			}

			// ******
			if( generateSuccess ) {
				ResultFilesHelper.DeleteFiles( backedUpFiles );

				if( rule.VSAddResultFiles ) {
					//
					// rule wants the file in rule.ResultFiles to be added to the visual studio project
					// beneath the source file - children of the source file
					//
					// note this will only work if running as a custom tool under visual studio, the
					// command line tool just ignores the request
					//
					ResultFilesHelper.AddFilesToProject( input, rule.ResultFiles, Service );
				}
			}
			else {
				ResultFilesHelper.RestoreFilesFromBackup( backedUpFiles );
			}

			// ******
			return generateSuccess;
		}


		/////////////////////////////////////////////////////////////////////////////

		// http://stackoverflow.com/questions/2477271/concatenate-text-files-with-windows-command-line-dropping-leading-lines

		public bool Generate( bool notifyIfRuleNotFound, out string result, out string outputExt )
		{
			// ******
			result = string.Empty;
			outputExt = string.Empty;

			// ******
			if( null == initialInput ) {
				//
				// input file not found
				//
				Service.SendError( inputFileName, $"file \"{inputFileName}\" was not found", -1, -1 );
				return false;
			}

			// ******
			//
			// debugging
			//
			var historyStack = new Stack<Tuple<InputFile, ExtensionRule>> { };

			// ******
			//
			// FilePath is used as the start point to discover config files
			//
			currentInput = initialInput;
			//var rules = new ERules( currentInput.PathOnly, Service );
			var rules = Rules;

			// ******
			var startDir = Directory.GetCurrentDirectory();
			try {
				var runNext = currentInput.NameWithExt;
				while( true ) {
					Directory.SetCurrentDirectory( currentInput.PathOnly );
					var rule = rules.Locate( runNext, currentInput.Extension );
					historyStack.Push( new Tuple<InputFile, ExtensionRule>( currentInput, rule ) );

					if( null == rule ) {
						if( notifyIfRuleNotFound ) {
							NotifyOfErrors( ErrorType.SettingsError, $"could not locate rule for {currentInput.NameWithPath}" );
						}
						return false;
					}

					if( null == rule.Commands || 0 == rule.Commands.Count ) {
						NotifyOfErrors( ErrorType.SettingsError, $"no commands to execute in rule {rule.Id}" );
						return false;
					}

					// ******
					outputExt = rule.OutputExtension?.Trim() ?? string.Empty;
					if( !string.IsNullOrWhiteSpace( outputExt ) && '.' != outputExt.First() ) {
						outputExt = "." + outputExt;
					}

					// ******
					if( !TryGenerate( rule, out result ) ) {
						result = string.Empty;
						return false;
					}

					// ******
					//
					// this will normally be true for the command line version of STR and false for
					// the visual studio tool (single file generator)
					//
					if( SaveOutput ) {
						WriteFile( currentInput, outputExt, result );
					}

					// ******
					//
					// post run
					//
					// ******
					if( string.IsNullOrWhiteSpace( rule.RunNext ) ) {
						return true;
					}

					// ******
					if( !string.IsNullOrWhiteSpace( rule.FilePatternRx ) ) {
						//
						// must match rule.FileNameWithoutExt if we're to process
						// rule.RunNext
						//
						Regex rx = new Regex( rule.FilePatternRx );
						if( !rx.IsMatch( currentInput.FileNameWithoutExt ) ) {
							return true;
						}
					}

					// ******
					//
					// new InputFile instance where file name includes the output ext from the previous
					// run
					//
					currentInput = new InputFile( Path.Combine( currentInput.PathOnly, currentInput.FileNameWithoutExt ) + outputExt, result );
					runNext = rule.RunNext;
				}
			}
			finally {
				Directory.SetCurrentDirectory( startDir );
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Ctor when NOT running under Visual Studio, automatically save file to
		/// output. To defeat this behaviour set SaveOutput to false
		/// </summary>
		/// <param name="inputFileName"></param>

		public Runner( string inputFileName, string projectExt = "" )//, IExtSvcProvider service = null )
			: this( inputFileName, null, new Service(inputFileName, projectExt )) //service )
		{

			//
			// pass in project type and new service
			//

			SaveOutput = true;
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Ctor when running as a single file generator under visual studio as a
		/// custom tool.
		/// </summary>
		/// <param name="inputFileName"></param>
		/// <param name="inputFileContent"></param>
		/// <param name="service"></param>

		public Runner( string inputFileName, string inputFileContent, IExtSvcProvider service )
		{
			// ******
			if( string.IsNullOrWhiteSpace( inputFileName ) ) {
				throw new ArgumentNullException( nameof( inputFileName ) );
			}

			// ******
			this.inputFileName = inputFileName;
			bool fileExists = File.Exists( inputFileName );

			if( fileExists ) {
				this.initialInput = new InputFile( inputFileName, inputFileContent );
			}
			else {
				//
				// Generate will see that 'initialInput' is empty and cause an 
				// error to occur
				//
			}

			if( null == service ) {
				this.Service = service = new Service( inputFileName );
			}
			else {
				this.Service = service;
			}

			service.NotifyOfErrors = this.NotifyOfErrors;
			service.NotifyOfSuccess = this.NotifyOfSuccess;

			// ******
			service.ClearErrorList();
		}

	}

}
