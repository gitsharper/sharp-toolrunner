using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Utilities;


namespace ToolRunner {


	/////////////////////////////////////////////////////////////////////////////

	public class Replace {

		public const string ASSETS_FOLDER = ".Assets";

		IExtSvcProvider reporter;
		string basePath;
		List<string> cmdLineArgs;


		/////////////////////////////////////////////////////////////////////////////

		public static string GetInternalPath( string fileNameIn )
		{
			var pos = fileNameIn.IndexOf( ':' );
			if( pos > 1 ) {
				var directory = fileNameIn.Substring( 0, pos );
				var name = fileNameIn.Substring( 1 + pos );
				var internalPath = Path.Combine( LibInfo.CodeBasePath, directory, name );
				return internalPath;
			}
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected bool TryGetSourceFile( InputFile input, string fileNameIn, out string fileContents )
		{
			// ******
			fileContents = string.Empty;
			string path = string.Empty;

			// ******
			try {
				path = GetInternalPath( fileNameIn );

				if( null == path ) {
					var argsList = ERCommand.Substitute( new List<string> { fileNameIn }, input, null, null, null );
					path = argsList.FirstOrDefault();
				}

				if( null == path ) {
					var fileName = fileNameIn.Replace( '/', '\\' );
					path = Path.IsPathRooted( fileName ) ? fileName : Path.Combine( basePath, fileName );
				}

				if( !File.Exists( path ) ) {
					reporter.NotifyOfErrors( ErrorType.PrepError, $"replace: could not locate file: \"{fileNameIn}\" at \"{path}\"" );
					return false;
				}

				// ******
				fileContents = File.ReadAllText( path );
				return true;
			}
			catch( Exception ex ) {
				reporter.NotifyOfErrors( ErrorType.PrepError, $"replace: exception attempting to find/load file: \"{fileNameIn}\" at \"{path}\"\r\n{ex.Message}" );
			}

			// ******
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool Process( InputFile input, out string result )
		{
			// ******
			result = string.Empty;

			// ******
			if( cmdLineArgs.Count < 2 ) {
				reporter.NotifyOfErrors( ErrorType.PrepError, $"replace: requires these argument: [ 'target-location-in-src-file', 'name-of-source-file'  ]" );
			}

			// ******
			var target = cmdLineArgs [ 0 ];
			if( string.IsNullOrWhiteSpace( target ) ) {
				reporter.NotifyOfErrors( ErrorType.PrepError, $"replace: target text is empty" );
				return false;
			}

			// ******
			//
			// path to file
			//
			var filePath = cmdLineArgs [ 1 ]?.Trim();
			if( string.IsNullOrEmpty( filePath ) ) {
				reporter.NotifyOfErrors( ErrorType.PrepError, $"replace: requires name for source file" );
			}

			string srcText;
			//if( !TryGetSourceFile( filePath, out srcText ) ) {
			if( !TryGetSourceFile( input, filePath, out srcText ) ) {
				return false;
			}

			if( string.IsNullOrWhiteSpace( srcText ) ) {
				reporter.NotifyOfErrors( ErrorType.PrepError, $"replace: the source file is empty" );
				return false;
			}

			// ******
			int index = srcText.IndexOf( target );
			if( index < 0 ) {
				reporter.NotifyOfErrors( ErrorType.PrepError, $"replace: could not locate target text in source file, target: \"${target}\"" );
				return false;
			}

			result = srcText.Substring( 0, index ) + input.Content + srcText.Substring( index + target.Length );


			// ******
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public Replace( IExtSvcProvider reporter, string basePath, ERCommand erCmd )
		{
			this.reporter = reporter;
			this.basePath = basePath;
			//this.erCmd = erCmd;
			this.cmdLineArgs = erCmd.CmdLine;
		}

	}



}
