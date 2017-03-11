using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ToolRunner {

	/////////////////////////////////////////////////////////////////////////////

	public class ResultFilesHelper {

		/////////////////////////////////////////////////////////////////////////////

		static List<string> GetPossibleFileNames( InputFile srcFile, IEnumerable<string> resultFiles )
		{
			// ******
			var list = new List<string> { };
			if( null == resultFiles || 0 == resultFiles.Count() ) {
				return list;
			}

			// ******
			var path = srcFile.PathOnly;
			var fileNameOnly = srcFile.NameWithoutExt;

			foreach( var item in resultFiles ) {
				if( string.IsNullOrWhiteSpace( item ) ) {
					continue;
				}

				// ******
				var name = '*' == item [ 0 ] ? fileNameOnly + item.Substring( 1 ) : item;
				list.Add( Path.Combine( path, name ) );
			}

			// ******
			return list;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void DeleteFiles( List<string> files )
		{
			foreach( var file in files ) {
				try {
					if( File.Exists( file ) ) {
						File.Delete( file );
					}
				}
				catch {
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public static List<string> RenameFilesForBackup( List<string> files )
		{
			// ******
			var renamedFiles = new List<string> { };

			foreach( var fileName in files ) {
				var tempFileName = fileName + ".backup";
				renamedFiles.Add( tempFileName );

				try {
					if( File.Exists( tempFileName ) ) {
						File.Delete( tempFileName );
					}
				}
				catch {
				}

				File.Move( fileName, tempFileName );
			}

			// ******
			return renamedFiles;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void RestoreFilesFromBackup( List<string> files )
		{
			// ******
			foreach( var fileName in files ) {
				var restoredFileName = Path.Combine( Path.GetDirectoryName( fileName ), Path.GetFileNameWithoutExtension( fileName ) );
				try {
					if( File.Exists( restoredFileName ) ) {

						// ?????

						File.Delete( restoredFileName );
					}
					if( File.Exists( fileName ) ) {
						File.Move( fileName, restoredFileName );
					}
				}
				catch {

				}
			}
		}

		/////////////////////////////////////////////////////////////////////////////

		public static List<string> DiscoverGeneratedFiles( InputFile srcFile, IEnumerable<string> resultFiles )
		{
			// ******
			var possibleFilePaths = GetPossibleFileNames( srcFile, resultFiles );
			var foundFiles = new List<string> { };

			foreach( var filePath in possibleFilePaths ) {
				if( File.Exists( filePath ) ) {
					foundFiles.Add( filePath );
				}
			}

			// ******
			return foundFiles;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static void AddFilesToProject( InputFile srcFile, IEnumerable<string> resultFiles, IExtSvcProvider service )
		{
			// ******
			var allPossibleFiles = GetPossibleFileNames( srcFile, resultFiles );
			var filesThatExist = DiscoverGeneratedFiles( srcFile, resultFiles );
			var filesThatDontExist = allPossibleFiles.Except( filesThatExist );

			// ******
			//
			// add to project
			//
			service.AddFilesToProject( filesThatExist );
			//
			// remove from project
			//
			service.RemoveFilesFromProject( filesThatDontExist );
		}



	}
}
