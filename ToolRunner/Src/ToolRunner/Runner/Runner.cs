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

		protected bool TryGenerate( ExtensionRule rule, out string result )
		{
			// ******
			result = string.Empty;

			// ******
			var input = currentInput.Duplicate();
			try {
				foreach( var cmd in rule.Commands ) {

					// ******
					var run = new RunOne( input, Service );
					var success = run.Run( cmd, out result );
					if( !success ) {
						return false;
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
					if( null == result ) {
						//
						// there was no output file from the tool
						//
						// if cmd.AllowNoOutput is true (default is false) then the input
						// to the next tool, or the output from sharp-ToolRunner will be empty
						//
						result = string.Empty;
						if( !cmd.AllowNoOutput ) {
							Service.SendError( inputFileName, $"the command \"{cmd.ExecutableName}\" producted no output", -1, -1 );
							return false;
						}
					}

					// ******
					if( rule.SaveIntermediateFiles && cmd != rule.Commands.Last() ) {
						WriteFile( input, cmd.SaveExtension, result );
					}

					// ******
					input.SetContent( result );
				}
			}
			finally {
			}

			// ******
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////

		// http://stackoverflow.com/questions/2477271/concatenate-text-files-with-windows-command-line-dropping-leading-lines

		public bool Generate( out string result, out string outputExt )
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
			var rules = new ERules( currentInput.PathOnly, Service );

			// ******
			var startDir = Directory.GetCurrentDirectory();
			try {
				var runNext = currentInput.NameWithExt;
				while( true ) {
					Directory.SetCurrentDirectory( currentInput.PathOnly );
					var rule = rules.Locate( runNext, currentInput.Extension );
					historyStack.Push( new Tuple<InputFile, ExtensionRule>( currentInput, rule ) );

					if( null == rule ) {
						NotifyOfErrors( ErrorType.SettingsError, $"could not locate rule for {currentInput.NameWithPath}" );
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
						// must match rule.FileNameWithoutExt
						//
						Regex rx = new Regex( rule.FilePatternRx );
						if( !rx.IsMatch( currentInput.FileNameWithoutExt ) ) {
							return true;
						}
					}

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
		/// Ctor when not running under Visual Studio, automatically save file to
		/// output. To defeat this behaviour set SaveOutput to false
		/// </summary>
		/// <param name="inputFileName"></param>

		public Runner( string inputFileName )
			: this( inputFileName, null, null )
		{
			SaveOutput = true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public Runner( string inputFileName, string inputFileContent, IExtSvcProvider service )
		{
			// ******
			if( string.IsNullOrWhiteSpace( inputFileName ) ) {
				throw new ArgumentNullException( nameof( inputFileName ) );
			}

			// ******
			this.inputFileName = inputFileName;
			if( File.Exists( inputFileName ) ) {
				this.initialInput = new InputFile( inputFileName, inputFileContent );
			}
			else {
				//
				// Generate will see that 'initialInput' is empty and cause an 
				// error to occur
				//
			}

			if( null == service) {
				this.Service = service = new Service { };
			}

			this.Service = service;

			service.NotifyOfErrors = this.NotifyOfErrors;
			service.NotifyOfSuccess = this.NotifyOfSuccess;

			// ******
			service.ClearErrorList();
		}

	}

}
