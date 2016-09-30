using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Utilities {

	public static class FileAndDirectoryHelpers {

		/////////////////////////////////////////////////////////////////////////////

		public static string GetTempFilePath( string inputFileName )
		{
			// ******
			var tempDir = Path.GetTempPath();
			var fileName = Path.IsPathRooted( inputFileName ) ? Path.GetFileName( inputFileName ) : inputFileName;

			// ******
			return Path.Combine( tempDir, fileName );
		}

		
		/////////////////////////////////////////////////////////////////////////////

		public static bool PathIsRootedInPath( string fullPath, string rootedPath )
		{
			return NormalizePathString( fullPath ).StartsWith( NormalizePathString( rootedPath ) );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string NormalizePathString( string pathIn )
		{
			// ******
			if( string.IsNullOrWhiteSpace( pathIn ) ) {
				return string.Empty;
			}

			// ******
			var path = pathIn.Trim().Replace( '/', '\\' );
			return path.ToLower();
		}


		/////////////////////////////////////////////////////////////////////////////

		public static bool FindFileInPath( string path, string fileName, out string pathOut )
		{
			// ******
			if( string.IsNullOrWhiteSpace( path ) ) {
				throw new ArgumentException( $"{nameof( path )} is empty" );
			}

			if( string.IsNullOrWhiteSpace( fileName ) ) {
				throw new ArgumentException( $"{nameof( fileName )} is empty" );
			}

			// ******
			pathOut = string.Empty;
			if( !Directory.Exists( path ) ) {
				return false;
			}

			while( true ) {

				var tempPath = Path.Combine( path, fileName );

				if( File.Exists( tempPath ) ) {
					pathOut = tempPath;
					return true;
				}

				var pos = path.LastIndexOf( '\\' );
				if( pos < 0 ) {
					return false;
				}

				path = path.Substring( 0, pos );


			}

		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Get a list of paths starting in 'startDirectoryIn' decending to the
		/// 'finalDirectoryIn'. If 'fileToSearchFor' is not null/empty only return
		/// a path if the file is found in it (the path)
		/// 
		/// 'finalDirectoryIn' MUST be rooted in 'startDirectoryIn'; this means if
		/// you walk the directories backward toward the root directory from 'startDirecoryIn'
		/// you will arrive at 'finalDirectoryIn'
		/// </summary>
		/// <param name="startDirectoryIn"></param>
		/// <param name="finalDirectoryIn"></param>
		/// <param name="fileToSearchFor"></param>
		/// <returns></returns>

		public static List<string> GetSearchPaths( string startDirectoryIn, string finalDirectoryIn, bool validate = false, string fileToSearchFor = null )
		{
			// ******
			if( string.IsNullOrWhiteSpace( startDirectoryIn ) ) {
				throw new ArgumentException( $"{nameof( startDirectoryIn )} is empty" );
			}

			//if( null == finalDirectoryIn ) {
			//	throw new ArgumentException( $"{ nameof( finalDirectoryIn ) } is empty" );
			//}

			// ******
			var startDirectory = NormalizePathString( startDirectoryIn );
			var finalDirectory = NormalizePathString( finalDirectoryIn );
			var list = new List<string> { };

			// ******
			if( string.IsNullOrWhiteSpace( finalDirectory ) ) {
				//
				// not provided with a directory
				//
				return list;
			}

			// ******
			if( !PathIsRootedInPath( startDirectory, finalDirectory ) || startDirectory == finalDirectory ) {
				//
				// if not rooted then only this directory
				//
				list.Add( finalDirectory );
				return list;
			}

			//
			// rooted in startDirectory so get a list of all directories between the
			// two, including both
			//

			var diff = startDirectory.Substring( finalDirectory.Length );
			var l1 = diff.Split( new char [] { '\\' }, StringSplitOptions.RemoveEmptyEntries );

			// ******
			Func<string, bool> test = ( p ) => {
				if( !validate ) {
					return true;
				}

				if( !Directory.Exists( p ) ) {
					return false;
				}

				if( string.IsNullOrEmpty( fileToSearchFor ) ) {
					return true;
				}

				if( File.Exists( Path.Combine( p, fileToSearchFor ) ) ) {
					return true;
				}

				return false;
			};

			var path = finalDirectory;
			if( test( path ) ) {
				list.Add( path );
			}

			foreach( var pathPart in l1 ) {
				path = Path.Combine( path, pathPart );
				if( test( path ) ) {
					list.Add( path );
				}
			}

			// ******
			return list;
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Generate a full path from 'currentDirectoryIn' and 'pathIn'.
		/// </summary>
		/// <param name="currentDirectoryIn"></param>
		/// <param name="pathIn"></param>
		/// <returns></returns>

		public static string FixPath( string currentDirectoryIn, string pathIn )
		{
			// ******
			if( string.IsNullOrWhiteSpace( currentDirectoryIn ) ) {
				throw new ArgumentException( $"{ nameof( currentDirectoryIn ) } is empty" );
			}

			//if( null == pathIn ) {
			//	throw new ArgumentException( $"{ nameof( pathIn ) } is empty" );
			//}

			// ******
			var currentDirectory = NormalizePathString( currentDirectoryIn );
			var path = NormalizePathString( pathIn );
			string result = string.Empty;

			if( Path.IsPathRooted( path ) ) {
				//
				// UNC paths not supported
				//
				// TODO: support UNC paths
				//
				if( pathIn.StartsWith( @"\\" ) ) {
					throw new NotImplementedException( "UNC paths are not currently supported" );
				}

				// ******
				//
				// a path in the form "\[...\]file" will use the current drive; NOT a good idea
				// to use this form
				//
				//
				result = Path.GetFullPath( path );
			}
			else {
				var path2 = path.SkipWhile( c => '.' == c || '\\' == c );
				result = Path.Combine( currentDirectory, path2 );
			}

			// ******
			return result.ToLower();
		}


	}
}
