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
using System.Diagnostics;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;

using EnvDTE;
using EnvDTE100;
using VSLangProj;


namespace CustomToolBase {

	/////////////////////////////////////////////////////////////////////////////

	public abstract class BaseCodeGenerator : IVsSingleFileGenerator, IDisposable {

		private string codeFileNamespace = string.Empty;
		private string codeFilePath = string.Empty;
		private IVsGeneratorProgress codeGeneratorProgress;


		/////////////////////////////////////////////////////////////////////////////

		protected IVsGeneratorProgress CodeGeneratorProgress
		{
			[DebuggerStepThrough]
			get
			{
				return this.codeGeneratorProgress;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected string FileNamespace
		{
			[DebuggerStepThrough]
			get
			{
				return this.codeFileNamespace;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected string InputFilePath
		{
			[DebuggerStepThrough]
			get
			{
				return this.codeFilePath;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public abstract string GetDefaultExtension();
		protected abstract string GenerateCode( string inputFileName, string inputFileContent );


		/////////////////////////////////////////////////////////////////////////////

		public void _Generate( string inputFilePath,
													string inputFileContents,
													string defaultNamespace,
													out IntPtr outputFileContents,
													out int output,
													IVsGeneratorProgress generateProgress )
		{
			// ******
			if( inputFileContents == null ) {
				throw new ArgumentNullException( "inputFileContents" );
			}

			// ******
			try {
				this.codeFilePath = inputFilePath;
				this.codeFileNamespace = defaultNamespace;
				this.codeGeneratorProgress = generateProgress;

				// ******
				//byte[] source = this.GenerateCode( inputFilePath, inputFileContents );
				var source = this.GenerateCode( inputFilePath, inputFileContents );

				if( source == null ) {
					outputFileContents = IntPtr.Zero;
					output = 0;
				}
				else {
					var bytes = System.Text.Encoding.UTF8.GetBytes( source );
					output = bytes.Length;
					outputFileContents = Marshal.AllocCoTaskMem( output );
					Marshal.Copy( bytes, 0, outputFileContents, output );
				}
			}
			//catch ( Exception ex ) {
			//	throw;
			//}
			finally {
				this.codeFilePath = null;
				this.codeFileNamespace = null;
				this.codeGeneratorProgress = null;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected virtual void GeneratorErrorCallback( bool warning, int level, string message, int line, int column )
		{
			// ******
			IVsGeneratorProgress codeGeneratorProgress = this.CodeGeneratorProgress;
			if( codeGeneratorProgress != null ) {
				if( line > 0 ) {
					line--;
				}
				if( column > 0 ) {
					column--;
				}

				// ******
				ErrorHandler.ThrowOnFailure( codeGeneratorProgress.GeneratorError( warning ? -1 : 0, (uint) level, message, (uint) line, (uint) column ) );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		int IVsSingleFileGenerator.DefaultExtension( out string pbstrDefaultExtension )
		{
			// ******
			try {
				pbstrDefaultExtension = this.GetDefaultExtension();
			}
			catch {
				pbstrDefaultExtension = string.Empty;
				return -2147467259;
			}

			// ******
			return 0;
		}


		/////////////////////////////////////////////////////////////////////////////

		int IVsSingleFileGenerator.Generate( string wszInputFilePath,
																					string bstrInputFileContents,
																					string wszDefaultNamespace,
																					IntPtr [] rgbOutputFileContents,
																					out uint pcbOutput,
																					IVsGeneratorProgress pGenerateProgress )
		{
			try {
				//
				// pcbOutput is a 'uint' where this.Generate() takes an 'int' in that postion
				//
				IntPtr rgbOutputFileContentsStandin = IntPtr.Zero;
				int pcbOutputStandin = 0;

				this._Generate( wszInputFilePath, bstrInputFileContents, wszDefaultNamespace, out rgbOutputFileContentsStandin, out pcbOutputStandin, pGenerateProgress );

				rgbOutputFileContents [ 0 ] = rgbOutputFileContentsStandin;
				pcbOutput = (uint) pcbOutputStandin;
			}
			catch {
				pcbOutput = 0;
				rgbOutputFileContents [ 0 ] = IntPtr.Zero;
				return -2147467259;
			}

			// ******
			return 0;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Dispose()
		{
			this.Dispose( true );

			//Trace.WriteLine( "Dispose called");

			GC.SuppressFinalize( this );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected virtual void Dispose( bool disposing )
		{
			this.codeGeneratorProgress = null;
		}


		/////////////////////////////////////////////////////////////////////////////

		~BaseCodeGenerator()
		{
			this.Dispose( false );
		}

		/////////////////////////////////////////////////////////////////////////////

		protected BaseCodeGenerator()
		{
		}


	}

}
