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
		List<string> args;


		/////////////////////////////////////////////////////////////////////////////

		protected bool TryLoadInternalFile( string directoryName, string fileName, out string fileContents )
		{
			// ******
			fileContents = string.Empty;
			string assetPath = string.Empty;

			// ******
			try {
				assetPath = Path.Combine( LibInfo.CodeBasePath, directoryName, fileName );
				if( !File.Exists( assetPath ) ) {
					reporter.NotifyOfErrors( ErrorType.PrepError, $"replace: could not locate internal file: \"{fileName}\" at \"{assetPath}\"" );
					return false;
				}

				// ******
				fileContents = File.ReadAllText( assetPath );
				return true;
			}
			catch( Exception ex ) {
				reporter.NotifyOfErrors( ErrorType.PrepError, $"replace: exception attempting to find/load internal file: \"{fileName}\" at \"{assetPath}\"\r\n{ex.Message}" );
			}

			// ******
			return false;
		}



		/////////////////////////////////////////////////////////////////////////////

		protected bool TryGetSourceFile( string fileNameIn, out string fileContents )
		{
			// ******
			fileContents = string.Empty;
			string path = string.Empty;

			// ******

			//
			// we need an internal assest manager, this is the second time we've looked for
			// "internal:" - other was parsing/calling Replace
			//

			//
			// colon must exist and be in a position greater than 1 so that we don't catch
			// "c:\something ..."
			//
			var pos = fileNameIn.IndexOf( ':' );
			if( pos > 1) {
				var directory = fileNameIn.Substring( 0, pos );
				var name = fileNameIn.Substring( 1 + pos );
				return TryLoadInternalFile( directory, name, out fileContents );
			}


			// ******
			try {
				var fileName = fileNameIn.Replace( '/', '\\' );
				path = Path.IsPathRooted( fileName ) ? fileName : Path.Combine( basePath, fileName );

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

		public bool Process( string content, out string result )
		{
			// ******
			result = string.Empty;

			// ******
			if( args.Count < 2 ) {
				reporter.NotifyOfErrors( ErrorType.PrepError, $"replace: requires these argument: [ 'target-location-in-src-file', 'name-of-source-file'  ]" );
			}

			// ******
			var target = args [ 0 ];
			if( string.IsNullOrWhiteSpace( target ) ) {
				reporter.NotifyOfErrors( ErrorType.PrepError, $"replace: target text is empty" );
				return false;
			}

			// ******
			var filePath = args [ 1 ]?.Trim();
			if( string.IsNullOrEmpty( filePath ) ) {
				reporter.NotifyOfErrors( ErrorType.PrepError, $"replace: requires name for source file" );
			}

			string srcText;
			if( !TryGetSourceFile( filePath, out srcText ) ) {
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

			result = srcText.Substring( 0, index ) + content + srcText.Substring( index + target.Length );


			// ******
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////

		public Replace( IExtSvcProvider reporter, string basePath, ERCommand erCmd )
		{
			this.reporter = reporter;
			this.basePath = basePath;
			//this.erCmd = erCmd;
			this.args = erCmd.CmdLine;
		}

	}



}
