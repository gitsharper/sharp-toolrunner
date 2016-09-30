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

using System.IO;

using Utilities;
using CustomToolBase;


// http://code.msdn.microsoft.com/SingleFileGenerator/Project/ProjectRss.aspx

namespace ToolRunner {


	/////////////////////////////////////////////////////////////////////////////

	partial class RunOne {

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

		//string [] generatedFileNameCommonParts = {
		//						".tokens",
		//						"Parser.cs",
		//						"Lexer.cs",
		//						"Lexer.tokens",
		//						"BaseListener.cs",
		//						"Listener.cs",
		//						"BaseVisitor.cs",
		//						"Visitor.cs",
		//					};

#pragma warning disable CA1811

		List<string> GetPossibleFileNames( string inputFileName )
		{
			//// ******
			//var path = Path.GetDirectoryName( inputFileName );
			//var fileName = Path.GetFileNameWithoutExtension( inputFileName );
			//var baseNameAndPath = Path.Combine( path, fileName );

			//// ******
			//var list = new List<string> { };
			//foreach( var item in generatedFileNameCommonParts ) {
			//	list.Add( baseNameAndPath + item );
			//}

			//// ******
			//return list;

			var toAppeaseCodeAnalysis = inputFileName;
			inputFileName = toAppeaseCodeAnalysis;
			throw new NotImplementedException( "GetPossibleFileNames" );
		}


		/////////////////////////////////////////////////////////////////////////////

		string AddFilesToProject()
		{
			// ******
			var allPossibleFiles = GetPossibleFileNames( input.NameWithPath );
			var filesThatExist = DiscoverGeneratedFiles( input.NameWithPath );
			var filesThatDontExist = allPossibleFiles.Except( filesThatExist );

			// ******
			service.AddFilesToProject( filesThatExist ); //, null );
			service.RemoveFilesFromProject( filesThatDontExist );

			// ******
			var sb = new StringBuilder { };

			sb.Append( "Files that are in, or have just been added to project:\r\n" );
			foreach( var name in filesThatExist ) {
				sb.AppendFormat( "{0}\r\n", name );
			}

			sb.Append( "Files that have been removed from the project:\r\n" );
			foreach( var name in filesThatDontExist ) {
				sb.AppendFormat( "{0}\r\n", name );
			}

			// ******
			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		List<string> DiscoverGeneratedFiles( string inputFileName )
		{
			// ******
			var possibleFilePaths = GetPossibleFileNames( inputFileName );
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

		List<string> RenameFilesForBackup( List<string> files )
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

		void RestoreFilesFromBackup( List<string> files )
		{
			// ******
			foreach( var fileName in files ) {
				var restoredFileName = Path.Combine( Path.GetDirectoryName( fileName ), Path.GetFileNameWithoutExtension( fileName ) );
				try {
					if( File.Exists( restoredFileName ) ) {
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

		void DeleteFiles( List<string> files )
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



	}

}
