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

using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
//using Microsoft.VisualStudio.Shell.im;


using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Designer.Interfaces;

using EnvDTE;
using EnvDTE80;
using EnvDTE100;
using VSLangProj;
using VSLangProj80;


using System.CodeDom.Compiler;
using System.CodeDom;
using System.IO;
using Microsoft.Win32;

using CustomToolBase;
using ToolRunner;


// http://code.msdn.microsoft.com/SingleFileGenerator/Project/ProjectRss.aspx

namespace CustomTool {


	//	vsContextGuidVCSProject = "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}";
	//	vsContextGuidVBProject = "{164B10B9-B200-11D0-8C61-00A0C91E29D5}";
	//	vsContextGuidVJSProject = "{E6FDF8B0-F3D1-11D4-8576-0002A516ECE8}"; 


	class GuidList {

		public const string vsContextGuidVCSProject = "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}";
		public const string vsContextGuidVBProject = "{164B10B9-B200-11D0-8C61-00A0C91E29D5}";
		public const string vsContextGuidVJSProject = "{E6FDF8B0-F3D1-11D4-8576-0002A516ECE8}"; 

		public const string generatorGuid = "1E57D28D-0BE4-4E06-BACA-DE3CC8E49125";

	}

	/////////////////////////////////////////////////////////////////////////////

	// to get package to allways load: https://mhusseini.wordpress.com/2013/12/11/automatically-loading-vsix-packages/
	/*


	[PackageRegistration(UseManagedResourcesOnly = true, RegisterUsing = RegistrationMethod.Assembly)]
[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
[Guid(GuidList.guidDebuggerAutoAttachPkgString)]

		>>> [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string)]

		public sealed class MyTopnotchPackage : Package
{
    // maybe you'd want some code inside here...
}
*/

	//
	// identifies this generator
	//
	[Guid( GuidList.generatorGuid )]

	//[ComVisible( true )]
	[CodeGeneratorRegistration( typeof( SharpToolRunner ), "SharpToolRunner", GuidList.vsContextGuidVCSProject, GeneratesDesignTimeSource = true )]
	[CodeGeneratorRegistration( typeof( SharpToolRunner ), "SharpToolRunner", GuidList.vsContextGuidVBProject, GeneratesDesignTimeSource = true )]
	[ProvideObject( typeof( SharpToolRunner ) )]
	public class SharpToolRunner : CodeGeneratorWithSite {

		string defaultExtension = string.Empty;

		/////////////////////////////////////////////////////////////////////////////

		public override string GetDefaultExtension()
		{
			return defaultExtension;
		}


		/////////////////////////////////////////////////////////////////////////////

		protected override string GenerateCode( string inputFileName, string inputFileContent )
		{
			// ******
			string result = string.Empty;
			var vsi = new VSInterface( Dte, GlobalServiceProvider, SiteServiceProvider, CodeGeneratorProgress, inputFileName ) { };

			// ******
			//
			// we implement code generation to Runner so that it can be tested without
			// having to execute the CustomTool	inside Visual Studio
			//
			//var runner = new Runner( inputFileName, inputFileContent, vsi, CodeGeneratorProgress );
			var runner = new Runner( inputFileName, inputFileContent, vsi );
			if( runner.Generate( true, out result, out defaultExtension ) ) {
				return result;
			}
			else {
				return string.Empty;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public SharpToolRunner()
		{
		}

	}

}
