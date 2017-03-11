using System;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;

using ToolRunner;
using Utilities;

using static Utilities.FileAndDirectoryHelpers;


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
		public void PandocTest1()
		{
			// ******
			var dataFilePath = GetDataFilePath( "markdown-pandoc\\test1.md" );
			var runner = new Runner( dataFilePath ) { };
			Assert.NotNull( runner );

			var rules = runner.Rules;

			runner.Generate( true, out string result, out string outExt );
			Assert.Null( result );
			Assert.Equal( "", outExt );
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		//
		//
		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void Antlr4Test1()
		{
			// ******
			var dataFilePath = GetDataFilePath( "antlr4\\parameters.g4" );
			var runner = new Runner( dataFilePath ) { };
			Assert.NotNull( runner );

			var rules = runner.Rules;

			runner.Generate( true, out string result, out string outExt );

			//Assert.True( TestNormalizeString( result ).StartsWith( "varVector=(function(){" ) );
		}


		/////////////////////////////////////////////////////////////////////////////

		//[Fact]
		//public void FindResultFilesTest1()
		//{
		//	// ******
		//	var dataFilePath = GetDataFilePath( "antlr4\\parameters.g4" );
		//	var runner = new Runner( dataFilePath ) { };
		//	Assert.NotNull( runner );
		//
		//	var rules = runner.Rules;
		//
		//	runner.Generate( true, out string result, out string outExt );
		//	//Assert.True( TestNormalizeString( result ).StartsWith( "varVector=(function(){" ) );
		//}


		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void TestFindFileInPath()
		{
			string pathOut1;
			var result1 = FindFileInPath( testDataPath, "Not Really UnitTests.csproj", out pathOut1 );
			Assert.True( result1 );

			string pathOut2;
			var result2 = FindFileInPath( testDataPath, "sharp-toolrunner.sln", out pathOut2 );
			Assert.True( result2 );


			string pathOut3;
			var result3 = FindFileInPath( testDataPath, "out-of-towner", out pathOut3 );
			Assert.False( result3 );

			//Assert.Equal( 4, r1.Count );

		}



		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void TestGetSearchPaths()
		{
			var r1 = GetSearchPaths( @"c:\temp\tests\frank\friendly", @"c:\temp", false, null );
			Assert.Equal( 4, r1.Count );

			//
			// this test will fail IF there is any part of @"c:\temp\tests\frank\friendly" path on the machine
			//
			var r2 = GetSearchPaths( @"c:\temp\tests\frank\friendly", @"c:\temp", true, null );
			Assert.Equal( 0, r2.Count );

			var r3 = GetSearchPaths( Path.Combine( testDataPath, "Antlr4" ), testDataPath, true, "tool-runner.cfg.json" );
			Assert.Equal( 1, r3.Count );

		}



		/////////////////////////////////////////////////////////////////////////////

		[Fact]
		public void TestFixPath()
		{
			var r1 = FixPath( @"c:\temp", @"fileName.xx" );
			Assert.Equal( @"c:\temp\filename.xx", r1 );

			var r2 = FixPath( @"c:/temp", @"./fileName.xx" );
			Assert.Equal( @"c:\temp\filename.xx", r2 );

			//
			// this will fail if tests on not run on drive 'D:'
			//
			if( Directory.GetCurrentDirectory().ToLower().StartsWith( "d:" ) ) {
				var r3 = FixPath( @"c:/temp", @"/fileName.xx" );
				Assert.Equal( @"d:\filename.xx", r3 );
			}

			//
			// directory (no file ext)
			//
			var r4 = FixPath( @"c:/temp", @"./directory" );
			Assert.Equal( @"c:\temp\directory", r4 );

			var r5 = FixPath( @"c:/temp", @"" );
			Assert.Equal( @"c:\temp", r5 );

		}



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
			runner.Generate( true, out result, out outExt );
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
			runner.Generate( true, out result, out outExt );
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
			runner.Generate( true, out result, out outExt );
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
			runner.Generate( true, out result, out outExt );
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
			runner.Generate( true, out result, out outExt );
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
			runner.Generate( true, out result, out outExt );
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
			runner.Generate( true, out result, out outExt );
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
			runner.Generate( true, out result, out outExt );
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
			bool success = runner.Generate( true, out result, out outExt );

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
			runner.Generate( true, out result, out outExt );

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

