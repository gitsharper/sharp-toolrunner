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
using System.Diagnostics;
using System.Linq;
using System.Text;

//using System.Runtime.InteropServices;

//using Microsoft.VisualStudio;
//using Microsoft.VisualStudio.Shell;


//using Microsoft.VisualStudio.Shell.Interop;
////using Microsoft.VisualStudio.OLE.Interop;
//using Microsoft.VisualStudio.Designer.Interfaces;

////using Microsoft.VisualStudio.TextManager.Interop;

//using EnvDTE;
//using EnvDTE80;
//using EnvDTE100;
//using VSLangProj;
//using VSLangProj80;

using EnvDTE;
using EnvDTE100;
//using VSLangProj;

using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

using System.CodeDom.Compiler;
using System.CodeDom;
using System.IO;
using Microsoft.Win32;


// http://code.msdn.microsoft.com/SingleFileGenerator/Project/ProjectRss.aspx

using Utilities;

namespace CustomToolBase {

	public enum VsErrorMsgType {
		Error,
		Warning,
		Message,
		Trace,
	}

	public class VsError {

		public ErrorItemBase Error { get; private set; }

		public VsErrorMsgType MsgType { get; set; }
		public string FileName { get { return Error.FileName; } }
		public int Line { get { return Error.Line; } }
		public int Column { get { return Error.Column; } }
		public string ErrorText { get { return Error.ErrorText; } }

		public VsError()
		{

		}

		public VsError( ErrorItemBase error )
		{
			Error = error;
			MsgType = VsErrorMsgType.Error;
		}
	}


	/////////////////////////////////////////////////////////////////////////////

	public class VSInterface : IExtSvcProvider {

		//const int ErrorMessage = 0;
		//const int WarningMessage = 1;
		//const int Message = 2;
		//const int TraceMessage = 3;

		DTE Dte;
		ServiceProvider GlobalServiceProvider;
		ServiceProvider SiteServiceProvider;
		IVsGeneratorProgress CodeGeneratorProgress;

		static ErrorListProvider errListProvider = null;

		//string defaultExtension = string.Empty;

		protected Solution _solution;
		protected ProjectItem _projectItem;
		protected ConfigurationManager _configurationManager;
		protected Project _project;

		protected Configuration _projectConfiguration;


		/////////////////////////////////////////////////////////////////////////////

		public Solution Solution { get { return _solution; } }
		public Project Project { get { return _project; } }
		public ProjectItem ProjectItem { get { return _projectItem; } }
		public ConfigurationManager ConfigurationManager { get { return _configurationManager; } }



		/////////////////////////////////////////////////////////////////////////////

		// https://social.msdn.microsoft.com/Forums/vstudio/en-US/71453a75-d25a-4baa-b17e-eb66937a1d7f/can-i-get-projectguid-for-a-c-project?forum=vsx

		// IVsSolution.GetGuidOfProject Method 

		IVsHierarchy GetIVsHierarchy()
		{
			return SiteServiceProvider.GetService( typeof( IVsHierarchy ) ) as IVsHierarchy;
		}


		/////////////////////////////////////////////////////////////////////////////

		public ProjectItem FindProjectItem( string filePath )
		{
			// ******
			var fileName = Path.GetFileName( filePath );

			foreach( var o in ProjectItem.ProjectItems ) {
				var item = o as ProjectItem;
				if( null != item && 0 == string.CompareOrdinal( fileName, item.Name ) ) {
					return item;
				}
			}

			// ******
			return null;
		}


		/////////////////////////////////////////////////////////////////////////////

		public ProjectItem AddToProjectFromFile( string filePath )
		{
			// EnvDTE.ProjectItem itm = item.ProjectItems.AddFromFile( strFile );

			// ******
			ProjectItem item;
			if( null == (item = FindProjectItem( filePath )) ) {
				item = ProjectItem.ProjectItems.AddFromFile( filePath );
			}

			// ******
			return item;
		}


		/////////////////////////////////////////////////////////////////////////////

		public void AddFilesToProject( IEnumerable<string> paths )//, Action<ProjectItem> action )
		{
			foreach( var path in paths ) {
				var item = AddToProjectFromFile( path );
				//if( null != action && null != item ) {
				//	action( item );
				//}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void RemoveFilesFromProject( IEnumerable<string> paths )
		{
			foreach( var path in paths ) {
				var item = FindProjectItem( path );
				if( null != item ) {
					item.Delete();
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////
		//
		//
		//
		/////////////////////////////////////////////////////////////////////////////

		public string SolutionFullNameAndPath { get { return Solution.FullName; } }
		public string ProjectFullNameAndPath { get { return Project.FullName; } }

		public ErrorNotifier NotifyOfErrors { get; set; } = ( rrorType, error ) => { };
		public SuccessNotifier NotifyOfSuccess { get; set; } = () => { };


		/////////////////////////////////////////////////////////////////////////////

		protected void SendToErrList( ErrorListProvider errListProvider, VsErrorMsgType msgType, string fileName, string message, int line, int column )
		{
			// ******
			//ErrorListProvider ep = new ErrorListProvider( GlobalServiceProvider );
			if( null == errListProvider ) {
				return;
			}

			// ******
			TaskErrorCategory category;

			switch( msgType ) {
				case VsErrorMsgType.Error:
					category = TaskErrorCategory.Error;
					break;

				case VsErrorMsgType.Warning:
					category = TaskErrorCategory.Warning;
					break;

				default:
					category = TaskErrorCategory.Message;
					break;
			}

			// ******
			//
			// http://msdn.microsoft.com/en-us/library/microsoft.visualstudio.shell.errortask.aspx
			//
			ErrorTask et = new ErrorTask();

			et.CanDelete = true;
			et.ImageIndex = 0;
			et.ErrorCategory = category;
			et.Priority = TaskPriority.Normal;  //High;

			//et.Document = fileName ?? string.Empty;
			et.Document = Path.GetFileName( fileName );// ?? string.Empty;

			et.Text = message ?? string.Empty;
			et.Line = (int) line;
			et.Column = (int) column;

			//et.HierarchyItem = GetIVsHierarchy();

			//et.Navigate += (a, b) => {
			//	int i = 0;
			//} ;


			// ******
			try {
				errListProvider.Tasks.Add( et );
				//errListProvider.Show();
			}
			catch( Exception ex ) {
				string str = ex.Message;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void SendToErrorList( VsErrorMsgType msgType, string fileName, string message, int line, int column )
		{
			SendToErrList( errListProvider, msgType, fileName, message, line, column );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void ClearErrorList( bool andRefresh = true )
		{
			// ******
			//
			// clear the error list
			//
			TaskProvider.TaskCollection tc = errListProvider.Tasks;
			tc.Clear();

			// ******
			if( andRefresh ) {
				errListProvider.Refresh();
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void SendError( string fileName, string errorText, int lineIn, int column )
		{
			//
			// using progress.GeneratorError() allows for a "clickable" error,
			// manipulating the error list with the routine in 'vsi' do not
			//
			uint line = (uint) (lineIn > -1 ? lineIn - 1 : -1);
			CodeGeneratorProgress.GeneratorError( 0, 0, errorText, line, (uint) column );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void SendWarning( string fileName, string warning, int lineIn = -1, int column = -1 )
		{
			uint line = (uint) (lineIn > -1 ? lineIn - 1 : -1);
			CodeGeneratorProgress.GeneratorError( 1, 0, warning, line, (uint) column );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void SendMessage( string fileName, string message, int line = -1, int column = -1 )
		{
			SendToErrorList( VsErrorMsgType.Message, fileName, message, line, column );
		}


		/////////////////////////////////////////////////////////////////////////////

		public void OutputToWindow( string fmt, params object [] args )
		{
			// http://stackoverflow.com/questions/1094366/how-do-i-write-to-the-visual-studio-output-window-in-my-custom-tool

			//
			// more info, not necessarily used:
			// http://msdn.microsoft.com/en-us/library/bb166236.aspx
			//

			// ******
			//IVsOutputWindow outWindow = Package.GetGlobalService( typeof( SVsOutputWindow ) ) as IVsOutputWindow;
			IVsOutputWindow outWindow = GlobalServiceProvider.GetService( typeof( SVsOutputWindow ) ) as IVsOutputWindow;

			// ******
			//
			// these do not work; best guess: they changed how this works in VS 2010 but it didn't make
			// it into the documentation
			//
			// so, we'll create our own window - we know that works (in VS 2010 at least)
			//			
			//Guid generalPaneGuid = VSConstants.GUID_OutWindowGeneralPane; // P.S. There's also the GUID_OutWindowDebugPane available.
			Guid nmpWindowGuid = new Guid( "{09A9CC84-B600-4FAF-8177-82949048A3A2}" );

			// ******
			IVsOutputWindowPane generalPane;
			outWindow.GetPane( ref nmpWindowGuid, out generalPane );
			if( null == generalPane ) {
				if( 0 == outWindow.CreatePane( ref nmpWindowGuid, "Antlr4", 1, 1 ) ) {
					outWindow.GetPane( ref nmpWindowGuid, out generalPane );
				}
			}

			// ******
			if( null != generalPane ) {
				generalPane.OutputString( Helper.SafeStringFormat( fmt, args ) );
				generalPane.Activate(); // Brings this pane into view
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public void OutputToErrorList( IEnumerable<VsError> errors )
		{
			// ******
			ClearErrorList( andRefresh: false );

			foreach( var err in errors ) {
				SendToErrList( errListProvider, err.MsgType, err.FileName, err.ErrorText, err.Line, err.Column );
			}

			// ******
			errListProvider.Refresh();
			errListProvider.Show();

		}


		/////////////////////////////////////////////////////////////////////////////

		protected void Initialize( string inputFilePath )
		{
			// ******
			if( null == errListProvider ) {
				//
				// custom tool FAILS if we try to do this in the ctor
				//
				errListProvider = new ErrorListProvider( GlobalServiceProvider );
			}

			// ******
			_solution = Dte.Solution;
			if( null == _solution ) {
				return;
			}

			// ******
			_projectItem = _solution.FindProjectItem( inputFilePath );
			_configurationManager = _projectItem.ConfigurationManager;
			_project = _projectItem.ContainingProject;

			//IEnumerable<Project> projects = GetAllProjects( solution );

			// ******
			//
			// is always null - this is references elsewhere (null is ok)
			//
			_projectConfiguration = _project.ConfigurationManager.ActiveConfiguration;
		}


		/////////////////////////////////////////////////////////////////////////////

		public VSInterface( DTE dte, ServiceProvider globalServiceProvider, ServiceProvider siteServiceProvider, IVsGeneratorProgress CodeGeneratorProgress, string inputFileName )
		{
			// ******
			this.Dte = dte;
			this.GlobalServiceProvider = globalServiceProvider;
			this.SiteServiceProvider = siteServiceProvider;
			this.CodeGeneratorProgress = CodeGeneratorProgress;

			// ******
			Initialize( inputFileName );
		}

	}

}
