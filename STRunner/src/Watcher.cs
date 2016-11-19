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
	using static Program;


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
	
		
		http://stackoverflow.com/questions/530211/creating-a-blocking-queuet-in-net/530228#530228
	
		*/


	/////////////////////////////////////////////////////////////////////////////

	class SizeQueue<T> {
		private readonly Queue<T> queue = new Queue<T>();
		private readonly int maxSize;
		public SizeQueue( int maxSize ) { this.maxSize = maxSize; }

		public void Enqueue( T item )
		{
			lock( queue ) {
				while( queue.Count >= maxSize ) {
					Monitor.Wait( queue );
				}
				queue.Enqueue( item );
				if( queue.Count == 1 ) {
					// wake up any blocked dequeue
					Monitor.PulseAll( queue );
				}
			}
		}
		public T Dequeue()
		{
			lock( queue ) {
				while( queue.Count == 0 ) {
					Monitor.Wait( queue );
				}
				T item = queue.Dequeue();
				if( queue.Count == maxSize - 1 ) {
					// wake up any blocked enqueue
					Monitor.PulseAll( queue );
				}
				return item;
			}
		}
	}



	/////////////////////////////////////////////////////////////////////////////

	public class Watcher {

		FileSystemWatcher watcher = new FileSystemWatcher();
		string directory;
		string file;
		List<string> watchExts;

		SizeQueue<string> queue = new SizeQueue<string>( 8192 );


		/////////////////////////////////////////////////////////////////////////////

		protected void OnFileChanged( object source, FileSystemEventArgs e )
		{
			// ******
			if( WatcherChangeTypes.Changed == e.ChangeType ) {

				// ******
				if( null == file && watchExts.Count > 0 ) {
					var ext = Path.GetExtension( e.FullPath );
					if( string.IsNullOrEmpty( ext ) ) {
						return;
					}
					//
					// skip dot
					//
					ext = ext.Substring( 1 );
					if( null == watchExts.Find( s => s == ext ) ) {
						return;
					}
				}

				// ******
				queue.Enqueue( e.FullPath );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void Run()
		{
			// ******
			watcher.Changed += new FileSystemEventHandler( OnFileChanged );
			watcher.Created += new FileSystemEventHandler( OnFileChanged );
			//watcher.Deleted += new FileSystemEventHandler( OnFileChanged );

			// ******
			watcher.EnableRaisingEvents = false;
			{
				watcher.Path = directory;
				watcher.Filter = file ?? "*.*";

				// ******
				WriteMessage( $"SharpToolRunner running in directory watch mode" );
				WriteMessage( $"directory: {directory}" );
				WriteMessage( $"start time: {DateTime.Now}" );
				WriteMessage( $"terminate the program to stop watching" );
			}
			watcher.EnableRaisingEvents = true;

			// ******
			while( true ) {
				var path = queue.Dequeue();

				Console.WriteLine( $"begin HandleFile: {path}" );
				{
					string result;
					string outExt;
					var runner = new Runner( path ) { };
					runner.Generate( false, out result, out outExt );
				}
				WriteMessage( $"end HandleFile: {path}" );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public Watcher( string pathIn, List<string> extsToWatch )
		{
			// ******
			if( string.IsNullOrEmpty( pathIn ) ) {
				pathIn = Directory.GetCurrentDirectory();
			}

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

			watchExts = extsToWatch ?? new List<string> { };
			if( watchExts.Count > 0 ) {
				if( !string.IsNullOrEmpty( file ) ) {
					Program.WriteMessage( $"watching a list of file extensions takes precidence over watching for a spcific file, the file \"{file}\" will be ignored" );
					file = null;
				}
			}
		}

	}


}
