using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using EnvDTE;


public enum ErrorType {
	SettingsError, PrepError, FailureToExecute, Error
}

public delegate void ErrorNotifier( ErrorType et, string error );
public delegate void SuccessNotifier();

// and = something that ignores

public interface IExtSvcProvider {

	//
	// to be added
	//
	string SolutionFullNameAndPath { get; }
	string ProjectFullNameAndPath { get; }


	// this makes separating into ToolRunner and the SFG libraries easier, need to
	// fix later so SFG doen't need to implement these
	//
	ErrorNotifier NotifyOfErrors { get; set; }
	SuccessNotifier NotifyOfSuccess { get; set; }

	void AddFilesToProject( IEnumerable<string> paths );
	void RemoveFilesFromProject( IEnumerable<string> paths );

	void ClearErrorList( bool andRefresh = true );
	void SendError( string fileName, string errorText, int line, int column );
	void SendWarning( string fileName, string message, int line = -1, int column = -1 );
	void SendMessage( string fileName, string message, int line = -1, int column = -1 );
	void OutputToWindow( string fmt, params object [] args );

}

namespace ToolRunner {


	/////////////////////////////////////////////////////////////////////////////

	public class Service : IExtSvcProvider {

		public const string Newline = "\r\n";

		public string SolutionFullNameAndPath { get; private set; } = string.Empty;
		public string ProjectFullNameAndPath { get; private set; } = string.Empty;

		public ErrorNotifier NotifyOfErrors { get; set; }
		public SuccessNotifier NotifyOfSuccess { get; set; }


		/////////////////////////////////////////////////////////////////////////////

		public void AddFilesToProject( IEnumerable<string> paths )
		{
			//foreach( var path in paths) {
			//	OutputToWindow( $"request: add file to project \"{path}\"" );
			//   }
		}


		/////////////////////////////////////////////////////////////////////////////

		public void RemoveFilesFromProject( IEnumerable<string> paths )
		{
			//foreach( var path in paths ) {
			//	OutputToWindow( $"request: remove file from project \"{path}\"" );
			//}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void ClearErrorList( bool andRefresh = true )
		{

		}


		/////////////////////////////////////////////////////////////////////////////

		public void SendError( string fileName, string errorText, int line, int column )
		{
			Console.WriteLine( $"error: {fileName}{Newline}{errorText}{Newline}line {line}, column {column}" );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void SendWarning( string fileName, string message, int line = -1, int column = -1 )
		{
			Console.WriteLine( $"warning: {fileName}{Newline}{message}{Newline}line {line}, column {column}" );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void SendMessage( string fileName, string message, int line = -1, int column = -1 )
		{
			Console.WriteLine( $"warning: {fileName}{Newline}{message}{Newline}line {line}, column {column}" );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void OutputToWindow( string fmt, params object [] args )
		{
			Console.WriteLine( fmt, args );
		}


		/////////////////////////////////////////////////////////////////////////////

		void LocateFileOfType( string startPath, string targetFiles, Action<string> assignTo )
		{
			var pathInfo = Directory.GetParent( startPath + '\\' );
			var root = pathInfo.Root;

			while( pathInfo.FullName != root.FullName ) {
				var matchedFiles = Directory.GetFiles( pathInfo.FullName, targetFiles );

				if( matchedFiles.Length > 0 ) {
					var first = matchedFiles.FirstOrDefault();
					if( null != first ) {
						assignTo( first );
						return;
					}
				}

				// ******
				pathInfo = pathInfo.Parent;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public Service( string inputFileName, string projectType = "" )
		{
			// ******
			if( string.IsNullOrWhiteSpace( inputFileName ) ) {
				return;
			}

			// ******
			var path = Path.GetDirectoryName( inputFileName );

			LocateFileOfType( path, string.IsNullOrWhiteSpace(projectType) ? "*.*proj" :$"*.{projectType}", s => ProjectFullNameAndPath = s );
			LocateFileOfType( path, "*.sln", s => SolutionFullNameAndPath = s );


		}


	}



}