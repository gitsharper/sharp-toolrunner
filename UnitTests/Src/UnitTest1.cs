using System;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;

using ToolRunner;

namespace UnitTests {

	/////////////////////////////////////////////////////////////////////////////

	public class UnitTest1 {

		string testDataPath = Path.GetFullPath( @"..\..\Src\TestData\" );


		public bool ProgramInvoked { get; set; } = false;
		public bool XunitInvoked { get { return !ProgramInvoked; } }

		/////////////////////////////////////////////////////////////////////////////

		private string TestNormalizeString( string str )
		{
			var sb = new StringBuilder { };

			foreach( var ch in str ) {
				if( !char.IsControl( ch ) && !char.IsWhiteSpace( ch ) ) {
					sb.Append( ch );
				}
			}

			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public string GetDataFilePath( string fileName )
		{
			return Path.Combine( testDataPath, fileName );
		}




		/////////////////////////////////////////////////////////////////////////////
		//
		//
		//
		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void TypescriptTest1()
		{
			// ******
			var dataFilePath = GetDataFilePath( "Typescript\\raytracer.ts" );
			var runner = new Runner( dataFilePath ) { };
			Assert.NotNull( runner );

			string result;
			string outExt;
			runner.Generate( out result, out outExt );
			Assert.True( TestNormalizeString( result ).StartsWith( "varVector=(function(){" ) );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void RunOnlyTest1()
		{
			// ******
			var dataFilePath = GetDataFilePath( "RunOnly\\Test1.txt.nmp4" );
			var runner = new Runner( dataFilePath ) { };
			Assert.NotNull( runner );

			string result;
			string outExt;
			runner.Generate( out result, out outExt );
			Assert.Equal( "Fred is dead", result );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void ReplaceTest1()
		{
			// ******
			var dataFilePath = GetDataFilePath( "MdReplace\\Test1.md" );
			var runner = new Runner( dataFilePath ) { };
			Assert.NotNull( runner );

			string result;
			string outExt;
			runner.Generate( out result, out outExt );
			Assert.True( result.StartsWith( "<!DOCTYPE html>" ) );
			Assert.Equal( ".html", outExt );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void Chaining1()
		{
			// ******
			var dataFilePath = GetDataFilePath( "Chaining\\Cmd-As-File-Name.nmpx" );
			var runner = new Runner( dataFilePath ) { };
			Assert.NotNull( runner );

			string result;
			string outExt;
			runner.Generate( out result, out outExt );
			Assert.Equal( "Fred is sometimes dead", result.Trim() );
			Assert.Equal( ".copied", outExt );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void MultipleTests2()
		{
			// ******
			var dataFilePath = GetDataFilePath( "MultipleCmds\\Cmd-As-File-Name.nmpx" );
			var runner = new Runner( dataFilePath ) { };
			Assert.NotNull( runner );

			string result;
			string outExt;
			runner.Generate( out result, out outExt );
			Assert.Equal( "Fred is sometimes dead", result.Trim() );
			Assert.Equal( ".txt", outExt );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void MultipleTests1()
		{
			// ******
			var dataFilePath = GetDataFilePath( "MultipleCmds\\Cmd1.nmpx" );
			var runner = new Runner( dataFilePath ) { };
			Assert.NotNull( runner );

			string result;
			string outExt;
			runner.Generate( out result, out outExt );
			Assert.Equal( "Fred is dead", result.Trim() );
			Assert.Equal( ".txt", outExt );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void RunNmp()
		{
			// ******
			var dataFilePath = GetDataFilePath( "Nmp4\\Test1.txt.nmp4" );
			var runner = new Runner( dataFilePath ) { };
			Assert.NotNull( runner );

			string result;
			string outExt;
			runner.Generate( out result, out outExt );
			Assert.Equal( "Fred is dead", result );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void RunNmpError1()
		{
			// ******
			var dataFilePath = GetDataFilePath( "Nmp4\\NmpError1.nmp4" );
			var runner = new Runner( dataFilePath ) { };
			Assert.NotNull( runner );

			string result;
			string outExt;
			runner.Generate( out result, out outExt );
			Assert.Equal( string.Empty, result );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void RunLessError1()
		{
			// ******
			var dataFilePath = GetDataFilePath( "Less\\LessError1.less" );
			var runner = new Runner( dataFilePath ) { };
			Assert.NotNull( runner );

			string result;
			string outExt;
			bool success = runner.Generate( out result, out outExt );

			Assert.False( success );
			Assert.Equal( string.Empty, result );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void RunLess()
		{
			// ******
			var dataFilePath = GetDataFilePath( "Less\\Less1.less" );
			var runner = new Runner( dataFilePath ) { };
			Assert.NotNull( runner );

			string result;
			string outExt;
			runner.Generate( out result, out outExt );

			var normalized = TestNormalizeString( result );
			Assert.Equal( "body{background:blue;}", normalized );
		}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void InitializeTest()
		{
			var dataFilePath = GetDataFilePath( "Less\\Less1.less" );
			var runner = new Runner( dataFilePath ) { };
			Assert.NotNull( runner );
		}


	}

}

