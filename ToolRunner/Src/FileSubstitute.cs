using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CustomTool {

	/////////////////////////////////////////////////////////////////////////////
	// $file_path The directory of the current file, e.g., C:\Files.
	// $file The full path to the current file, e.g., C:\Files\Chapter1.txt.
	// $file_name The name portion of the current file, e.g., Chapter1.txt.
	// $file_extension The extension portion of the current file, e.g., txt.
	// $file_base_name The name-only portion of the current file, e.g., Document.
	// $folder The path to the first folder opened in the current project.
	// $project The full path to the current project file.
	// $project_path The directory of the current project file.
	// $project_name The name portion of the current project file.
	// $project_extension The extension portion of the current project file.
	// $project_base_name The name-only portion of the current project file.
	// $packages The full path to the Packages folder. 

	public class FileSubstitute {

		protected const string FILE_EXTENSION = "$file_extension";
		protected const string FILE_BASE_NAME = "$file_base_name";
		protected const string FILE_PATH = "$file_path";
		protected const string FILE_NAME = "$file_name";
		protected const string FILE = "$file";

		// ******
		protected string FilePathIn;

		// ******
		public string File => Path.GetFullPath( FilePathIn );
		public string FilePath => Path.GetDirectoryName( FilePathIn );
		public string FileName => Path.GetFileName( FilePathIn );
		public string FileExtension => Path.GetExtension( FilePathIn );
		public string FileBaseName => Path.GetFileNameWithoutExtension( FilePathIn );


		/////////////////////////////////////////////////////////////////////////////

		protected bool _trySubstitute( string str, string strToCheckFor, string substStr, out string result )
		{
			// ******
			var index = str.IndexOf( strToCheckFor, StringComparison.OrdinalIgnoreCase );
			if( index >= 0 ) {
				result = str.Substring( 0, index ) + substStr + str.Substring( index + strToCheckFor.Length );
				return true;
			}

			// ******
			result = str;
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////

		public bool TrySubstitute( string strIn, out string strOut )
		{
			// ******
			var str = strIn;
			var success = false;

			while( true ) {
				//
				// repeat until no more substitutions, not doing longest first so the shorter
				// $file_xxx don't interfear with the longers ones
				//
				if( _trySubstitute( str, FILE_EXTENSION, FileExtension, out str ) ) {
					success = true;
					continue;
				}

				if( _trySubstitute( str, FILE_BASE_NAME, FileBaseName, out str ) ) {
					success = true;
					continue;
				}

				if( _trySubstitute( str, FILE_PATH, FilePath, out str ) ) {
					success = true;
					continue;
				}

				if( _trySubstitute( str, FILE_NAME, FileName, out str ) ) {
					success = true;
					continue;
				}

				if( _trySubstitute( str, FILE, File, out str ) ) {
					success = true;
					continue;
				}

				break;
			}


			// ******
			strOut = success ? str : string.Empty;
			return success;
		}


		/////////////////////////////////////////////////////////////////////////////

		public FileSubstitute( string filePathIn )
		{
			if( string.IsNullOrWhiteSpace( filePathIn ) ) {
				throw new ArgumentNullException( nameof( filePathIn ) );
			}
			this.FilePathIn = filePathIn;
		}
	}




}
