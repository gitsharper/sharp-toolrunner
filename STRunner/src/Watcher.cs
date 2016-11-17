using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ToolRunner;
using Utilities;

namespace STRunner {


	/*
		public class FileSystemEventArgs : EventArgs {
		public FileSystemEventArgs( WatcherChangeTypes changeType, string directory, string name );

		public WatcherChangeTypes ChangeType { get; }
		public string FullPath { get; }
		public string Name { get; }
	}

	namespace System.IO {
		[Flags]
		public enum WatcherChangeTypes {
			Created = 1,
			Deleted = 2,
			Changed = 4,
			Renamed = 8,
			All = 15
		}
	}
	
		*/
	/////////////////////////////////////////////////////////////////////////////

	public class Watcher {

		FileSystemWatcher watcher = new FileSystemWatcher();
		string directory;
		string file;


		/////////////////////////////////////////////////////////////////////////////
#pragma warning disable 1998

		protected async Task HandleFile( string path )
		{
			var runner = new Runner( path ) { };
			string result;
			string outExt;
			runner.Generate( false, out result, out outExt );
		}

#pragma warning restore 1998

		/////////////////////////////////////////////////////////////////////////////

		protected void OnFileChanged( object source, FileSystemEventArgs e )
		{
			//TVWindow.Dispatcher.BeginInvoke(
			//	System.Windows.Threading.DispatcherPriority.Normal,
			//	(ThreadStart) delegate {
			//		Thread.Sleep( 600 );
			//		LoadFileText();
			//		TVWindow.ReloadTextBlock();
			//	}///textBlock.Text = TextFileText; }
			//);

			//Task.Run( DoStuff );

			if( WatcherChangeTypes.Changed == e.ChangeType ) {
				HandleFile( e.FullPath ).FireAndForget();
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Run()
		{
			// ******
			watcher.Changed += new FileSystemEventHandler( OnFileChanged );
			watcher.Created += new FileSystemEventHandler( OnFileChanged );
			watcher.Deleted += new FileSystemEventHandler( OnFileChanged );

			// ******
			watcher.EnableRaisingEvents = false;

			watcher.Path = directory;
			watcher.Filter = file ?? "*.*";

			watcher.EnableRaisingEvents = true;

			// ******
			// no
			//await Task.Delay( -1 );

			Thread.Sleep( -1 );
		}


		/////////////////////////////////////////////////////////////////////////////

		public Watcher( string pathIn )
		{
			// ******
			var path = Path.GetFullPath( pathIn );
			if( File.Exists( path ) ) {
				directory = Path.GetDirectoryName( path );
				file = Path.GetFileName( path );
			}
			else if( Directory.Exists( path ) ) {
				directory = path;
				file = null;
			}
			else {
				throw new ArgumentException( $"{nameof( path )} does not exist as a directory or full file path" );
			}
		}

	}


}
