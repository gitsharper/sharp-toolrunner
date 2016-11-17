using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

using Utilities;
using static Utilities.FileAndDirectoryHelpers;


namespace ToolRunner {

	public enum ActionEnum { Default, RunOnly, RunOnlyIgnoreExitCode }

	/////////////////////////////////////////////////////////////////////////////

	[DebuggerDisplay( "{ExecutableName}, {CmdLine.Count} commands" )]
	[JsonObject( MemberSerialization.OptIn )]
	public class ERCommand {

		/// <summary>
		/// Name of the .com/.exe/.cmd/.bat/.btm file to be executed. If it contains no
		/// path then the environment's "path" will be used to locate it, if there is a
		/// partial path then it will be search for relative to the input file's directory,
		/// and if there is a full path it will be used for the search. The relative path
		/// allows you to have commands local to the project/solution, you can use "..\" to
		/// backup directories. *** note: should support something like: "project:" and
		/// "solution:", but we don't yet
		/// </summary>
		[JsonProperty]
		public string ExecutableName { get; set; } = string.Empty;

		/// <summary>
		/// Extension to use when there is more than one ERCommand, and the command is
		/// not the last one (which gets OutputExtension passed back to Visual Studio)
		/// </summary>
		[JsonProperty]
		public string SaveExtension { get; set; } = string.Empty;

		/// <summary>
		/// A series of command to be placed in the command line passed to the
		/// executable, any quoting must already be inplace within the strings.
		/// Currently the $in, $out and $arg replacement variables are replaced
		/// by: $in, the location in the command line where the input file name should
		/// be placed; $out, the location in the command line where the output file
		/// name should be placed; and $args, where any additional arguments parsed
		/// from the input file should be placed. Currently the executable must produce
		/// an output file, in the future we might support capturing stdout
		/// 
		/// stderr is monitored for errors
		/// 
		/// stdout is ignored
		/// 
		/// </summary>
		[JsonProperty]
		public List<string> CmdLine { get; set; } = new List<string> { };

		[JsonProperty]
		public bool ReadStdout { get; set; } = false;

		/// <summary>
		/// Valid actions: Default (or empty), RunOnly, RunOnlyIgnoreExitCode
		/// </summary>
		[JsonProperty]
		protected string Action { get; set; } = "Default";

		[JsonProperty]
		public bool AllowNoOutput { get; set; } = false;

		[JsonProperty]
		public bool DebugBreak { get; set; } = false;

		/// <summary>
		/// Allow a note to be added to each ERCommand
		/// </summary>
		[JsonProperty]
		public string Note { get; set; } = string.Empty;


		/////////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// How to execute the command.
		/// 
		/// Is not restored by json.net, we look at the Action property and note any
		/// errors if it is not correct.
		/// </summary>
		public ActionEnum ActionEnum { get; set; } = ActionEnum.Default;


		/////////////////////////////////////////////////////////////////////////////

		public bool IgnoreResults()
		{
			return ActionEnum.RunOnly == this.ActionEnum || ActionEnum.RunOnlyIgnoreExitCode == this.ActionEnum;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected static string _trySubstitute( string str, string strToCheckFor, string substStr )
		{
			// ******
			var index = str.IndexOf( strToCheckFor, StringComparison.OrdinalIgnoreCase );
			if( index >= 0 ) {
				return str.Substring( 0, index ) + substStr + str.Substring( index + strToCheckFor.Length );
			}

			// ******
			return str;
		}


		/////////////////////////////////////////////////////////////////////////////

		/*
$file_path The directory of the current file, e.g., C:\Files. 
$file The full path to the current file, e.g., C:\Files\Document.txt. 
$file_name The name portion of the current file, e.g., Document.txt. 
$file_extension The extension portion of the current file, e.g., txt. 
$file_base_name The name-only portion of the current file, e.g., Document. 
		*/

		public static List<string> Substitute( List<string> args, InputFile inputFile, string srcPath, string destPath, List<string> extraArgs )
		{
			var processedCmds = new List<string> { };
			var moreArgs = extraArgs?.Combine( " " ) ?? null;
			var toolRunnerDir = LibInfo.CodeBasePath + "\\";
			var assetsDir = LibInfo.CodeBasePath + $"\\{Replace.ASSETS_FOLDER}\\";

			foreach( var item in args ) {
				var arg = item;
				if( string.IsNullOrWhiteSpace( arg ) ) {
					continue;
				}

				// ******
				if( null != srcPath ) {
					arg = _trySubstitute( arg, "$in", srcPath );
				}

				if( null != destPath ) {
					arg = _trySubstitute( arg, "$out", destPath );
				}

				if( null != extraArgs ) {
					arg = _trySubstitute( arg, "$args", moreArgs );
				}

				// ******
				arg = _trySubstitute( arg, @"$\", toolRunnerDir );
				arg = _trySubstitute( arg, @"$assets:", assetsDir );

				//
				// longest first so shorter ones don't interfear
				//
				arg = _trySubstitute( arg, @"$file_base_name", inputFile.NameWithoutExt );
				arg = _trySubstitute( arg, @"$file_path", inputFile.PathOnly );
				arg = _trySubstitute( arg, @"$file_ext", inputFile.FileExtWithoutDot );
				arg = _trySubstitute( arg, @"$file", inputFile.NameWithPath );

				processedCmds.Add( arg );
			}

			// ******
			return processedCmds;
		}


		/////////////////////////////////////////////////////////////////////////////

		public List<string> Substitute( InputFile inputFile, string srcPath, string destPath, List<string> extraArgs )
		{
			return Substitute( CmdLine, inputFile, srcPath, destPath, extraArgs );
		}


		/////////////////////////////////////////////////////////////////////////////

		public string SubstituteAndCombine( InputFile inputFile, string srcPath, string destPath, List<string> extraArgs )
		{
			var cmdLine = Substitute( inputFile, srcPath, destPath, extraArgs ).Combine( " " );
			return cmdLine;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void AssureData( IExtSvcProvider service, string fileName )
		{
			ExecutableName = ExecutableName?.Trim() ?? string.Empty;
			SaveExtension = SaveExtension.AssureLeadingDot();
			if( null == CmdLine ) {
				CmdLine = new List<string> { };
			}


			//
			// Action, ActionEnum
			//
			ActionEnum ae;
			if( Enum.TryParse<ActionEnum>( Action, true, out ae ) ) {
				ActionEnum = ae;
			}
			else {
				service.SendWarning( fileName, $"\"{Action ?? ""}\" is not a valid action name, using \"Default\"", -1, -1 );
			}
		}


	}



	/////////////////////////////////////////////////////////////////////////////

	[DebuggerDisplay( "{Id}, {Location}" )]
	[JsonObject( MemberSerialization.OptIn )]
	public class ExtensionRule {

		/// <summary>
		/// Usually a file extension, that is used to locate the command
		/// to execute for the file being passed to Sharp-ToolRunner, it may also
		/// be an arbitray string if this entry is to be a "RunNext" target.
		/// </summary>
		[JsonProperty]
		public string Id { get; set; } = string.Empty;

		/// <summary>
		/// This is the file extension passed back to Visual Studio; it can be
		/// empty and Visual Studio will usually save the file without file extension
		/// of the source file (though occasionally VS will leave a '.' on the end of
		/// the file name and you have to go into the project file to fix it).
		/// </summary>
		[JsonProperty]
		public string OutputExtension { get; set; } = string.Empty;

		/// <summary>
		/// If not empty and the input file name (without extension) matches this 
		/// regular expression then if the command completes successfully AND
		/// there is a RunNext entry, the RunNext entry will be processed.
		/// 
		/// If this entry is empy and there is a RunNext entry then the RunNext
		/// entry will be processed.
		/// 
		/// FilePatternRx allows for "opting-out" of RunNext.
		/// </summary>
		[JsonProperty]
		public string FilePatternRx { get; set; } = string.Empty;

		/// <summary>
		/// Name of another ExtensionRule to process after the current one successfully
		/// completes. This allows the chaining of file processing; e.g., a markdown file
		/// is processed by "node/marked" to produce some Html, that Html is inserted into
		/// an existing file using "insertat.exe" and the result is passed back to Visual
		/// Studio
		/// </summary>
		[JsonProperty]
		public string RunNext { get; set; } = string.Empty;

		/// <summary>
		/// Save intermediate files if there is more than one cmd in Commands. Files will
		/// be overwritten if the ERCommand does not have a SaveExtension set
		/// </summary>
		[JsonProperty]
		public bool SaveIntermediateFiles { get; set; } = false;



		[JsonProperty]
		public bool SaveRestoreResultFiles { get; set; } = false;

		[JsonProperty]
		public bool VSAddResultFiles { get; set; } = false;


		/// <summary>
		/// Files to save before executing the cmd. If execution fails the files are restored,
		/// if the cmd is successful then backed up files are deleted.
		/// </summary>
		[JsonProperty]
		public List<string> ResultFiles { get; set; } = new List<string> { };



		/// <summary>
		/// One or more commands to execute on the input/result files
		/// </summary>
		[JsonProperty]
		public List<ERCommand> Commands { get; set; } = new List<ERCommand> { };

		/// <summary>
		/// Allow a note to be added to each ExtensionRule
		/// </summary>
		[JsonProperty]
		public string Note { get; set; } = string.Empty;

		// not saved
		public string Location { get; set; } = string.Empty;


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns the output extension making sure there is a leading dot EXCEPT
		/// if the extension is empty in which case the result is empty
		/// </summary>

		public string OutputExtWithDot
		{
			get {
				return OutputExtension.AssureLeadingDot();
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns the output extension making sure there is no leading dot,
		/// </summary>

		public string OutputExtWithoutDot
		{
			get {
				return OutputExtension.AssureNoLeadingDot();
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void AssureData( IExtSvcProvider reporter, string fileName )
		{
			Id = Id?.Trim().ToLower() ?? string.Empty;
			OutputExtension = OutputExtension.AssureLeadingDot();
			FilePatternRx = FilePatternRx?.Trim() ?? string.Empty;
			RunNext = RunNext?.Trim() ?? string.Empty;
			if( null == Commands ) {
				Commands = new List<ERCommand> { };
			}
			else {
				Commands.ForEach( erCmd => erCmd.AssureData( reporter, fileName ) );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public ExtensionRule()
		{

		}

	}



	/////////////////////////////////////////////////////////////////////////////

	[DebuggerDisplay( "{rules.Count} rules" )]
	public class ERules {

		// ******
		public const string USER_TOOLRUNNER_FOLDER = "Sharp-ToolRunner";
		public const string CfgFileName = "tool-runner.cfg.json";

		protected List<ExtensionRule> rules = new List<ExtensionRule> { };

		// ******
		IExtSvcProvider service;
		//protected IVSInterface Report;

		// ******
		protected List<string> directories = new List<string> { };


		/////////////////////////////////////////////////////////////////////////////

		protected bool AddRules( string json, string fileName )
		{
			// ******
			try {
				var result = JsonConvert.DeserializeObject<List<ExtensionRule>>( json );
				if( null != result ) {
					result.ForEach( rule => {
						rule.Location = fileName;
						rule.AssureData( service, fileName );
					} );

					//rules.AddRange( result );
					//
					// i'm sure i could do this with intersect or join, or linq something-or-other
					//
					var newRuleSet = new List<ExtensionRule>( result );
					foreach( var rule in rules ) {
						if( null == newRuleSet.Find( r => rule.Id == r.Id ) ) {
							newRuleSet.Add( rule );
						}
					}
					rules = newRuleSet;

				}
			}
			catch( Exception ex ) {
				service.SendWarning( fileName, "Could not process configuration file " + ex.Message, -1, -1 );
				return false;
			}

			// ******
			return true;
		}


		/////////////////////////////////////////////////////////////////////////////

		/*
			add directory where ERules class (dll) lives

			add users directory (mydocuments/sharp-toolrunner)

			start at file directory and walk backwards until find named .csproj
			(first .csproj if no name provided) and use that directory

			keep moving backwards (checking .csproj directory first) looking for
			a named .sln file, if no name provided use the first that is found


		assumption that file to convert is deeper in the project tree than the
		project or solution file


			bool NoProjectSearch
			bool NoSolutionSearch

			//
			// in the following a '*' before the name will force the custom tool to use
			// the ProjectName or SolutionName instead of the names provided by Visual
			// Studio 
			//

		? what about full or pratial paths

				a name with no path is searched by walking the directory tree backwards

				a name with a partial path is searched for relative to the directory in which
				.cfg.json file is located

				a full path (as Visual Studio returns) is checked by removing the project/solution name





			string ProjectName
			string SolutionName

		*/


		void Initialize( string startPath )
		{
			// file directory
			// project directory
			// solution directory
			// users directory
			// extension directory

			// ******
			//
			// file/rules closest to the input file will override rules in files/rules 
			// more distant
			//
			directories.Add( FixPath( startPath, string.Empty ) );

			//try {
			//	//
			//	// should (but dont) check each directory from input down to project, and then
			//	// solution
			//	//
			//	var path = service.ProjectFullNameAndPath;
			//	if( !string.IsNullOrEmpty( path ) ) {
			//		var projectDir = Path.GetFullPath( Path.GetDirectoryName( path ) );
			//		directories.Add( projectDir );
			//	}
			//
			//	path = service.SolutionFullNameAndPath;
			//	if( !string.IsNullOrEmpty( path ) ) {
			//		var solutionDir = Path.GetFullPath( Path.GetDirectoryName( path ) );
			//		directories.Add( solutionDir );
			//	}
			//}
			//catch {
			//}

			//
			// user directory in MyDocuments
			//
			var special = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
			directories.Add( Path.Combine( special, USER_TOOLRUNNER_FOLDER ) );

			//
			// ToolRunner execuitable directory
			//
			var extDir = Path.GetFullPath( LibInfo.CodeBasePath );
			directories.Add( extDir );

			//
			// order farthest to nearest
			//
			directories.Reverse();

			// ******
			foreach( var dir in directories ) {
				if( string.IsNullOrWhiteSpace( dir ) ) {
					continue;
				}

				if( !Directory.Exists( dir ) ) {
					continue;
				}

				var path = Path.Combine( dir, CfgFileName );
				if( !File.Exists( path ) ) {
					continue;
				}

				try {
					var text = File.ReadAllText( path );
					AddRules( text, path );
				}
				catch( Exception ex ) {
					service.SendWarning( path, "Could not read configuration file " + ex.Message, -1, -1 );
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		bool MatchId( string id, string value )
		{
			// ******
			if( !id.Contains( ',' ) ) {
				return id == value;
			}

			// *****
			var parts = id.Split( new char [] { ',' }, StringSplitOptions.RemoveEmptyEntries );
			foreach( var part in parts ) {
				if( part == value ) {
					return true;
				}
			}

			// ******
			return false;
		}



		/////////////////////////////////////////////////////////////////////////////

		public ExtensionRule Locate( string name, string ext )
		{
			// ******
			ExtensionRule rule = null;
			string value = string.Empty;

			// ******
			//
			// if it was passed in try to match name 
			//
			value = name?.Trim().ToLower();
			if( !string.IsNullOrEmpty( value ) ) {
				//rule = rules.Find( r => value == r.Id );
				rule = rules.Find( r => MatchId( r.Id, value ) );
				if( null != rule ) {
					return rule;
				}
			}

			// ******
			if( null == ext ) {
				//
				// not passed in
				//
				return null;
			}

			// ******
			//
			// note: empty "" is valid
			//
			value = ext.Trim().ToLower();
			rule = rules.Find( r => value == r.Id );
			if( null != rule ) {
				return rule;
			}

			// ******
			//
			// finally if there's a dot try removing it
			//
			if( '.' == value.First() ) {
				value = value.Substring( 1 );
				return rules.Find( r => value == r.Id );
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public ERules( string startPath, IExtSvcProvider service )
		{
			this.service = service;
			//this.Report = report;
			Initialize( startPath );
		}

	}


}
