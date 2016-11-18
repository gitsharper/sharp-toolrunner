using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;

using Utilities;

namespace ToolRunner {

	/////////////////////////////////////////////////////////////////////////////

	public class InputFile {

		public string NameWithPath { get; private set; }
		public string PathOnly { get; private set; }
		public string NameWithExt { get; private set; }
		public string NameWithoutExt { get; private set; }
		public string Extension { get; private set; }
		public string Content { get; private set; }

		/////////////////////////////////////////////////////////////////////////////

		public string FileExtWithoutDot { get { return Extension.AssureNoLeadingDot(); } }
		public string FileExtWithDot { get { return Extension.AssureLeadingDot(); } }


		/////////////////////////////////////////////////////////////////////////////

		public string FileNameWithoutExt
		{
			get {
				return Path.GetFileNameWithoutExtension( NameWithPath );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public string SetContent( string newContent )
		{
			var oldContent = Content;
			Content = newContent;
			return oldContent;
		}


		/////////////////////////////////////////////////////////////////////////////

		public InputFile Duplicate()
		{
			return (InputFile) MemberwiseClone();
		}


		/////////////////////////////////////////////////////////////////////////////

		FileStream GetFileStream( string fileName, int tries, int delay = 100 )
		{
			// ******
			var file = new FileInfo( fileName );

			while( true ) {
				try {
					var stream = file.Open( FileMode.Open, FileAccess.Read, FileShare.None );
					return stream;
				}
				catch( IOException ex ) when( tries > 0 ) {
					Thread.Sleep( delay );
					tries -= 1;
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		string ReadFromFile( string filename )
		{
			// ******
			using( var fs = GetFileStream( filename, 10 ) ) {
				byte [] buffer = new byte [ fs.Length ];
				fs.Read( buffer, 0, (int) fs.Length );
				return System.Text.Encoding.UTF8.GetString( buffer );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public InputFile( string inputFileName, string content )
		{
			// ******
			NameWithPath = inputFileName;

			var dir = Path.GetDirectoryName( inputFileName );
			if( null == dir ) {
				throw new Exception( $"input file name \"{inputFileName}\" does not have a path attached!" );
			}

			PathOnly = Path.GetFullPath( dir );

			NameWithExt = Path.GetFileName( inputFileName );
			NameWithoutExt = Path.GetFileNameWithoutExtension( NameWithExt );

			//
			// if not empty should have ext
			//
			Extension = (Path.GetExtension( inputFileName ) ?? string.Empty).AssureLeadingDot();

			// ******
			if( null == content ) {
				try {
					//Content = File.ReadAllText( inputFileName );
					Content = ReadFromFile( inputFileName );
				}
				catch( Exception ex ) {
					Content = string.Empty;
					Console.WriteLine( $"InputFile exception: {ex.Message}" );
				}
			}
			else {
				Content = content;
			}
		}
	}
}
