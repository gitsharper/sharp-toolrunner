using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace UnitTests {

	/////////////////////////////////////////////////////////////////////////////
	
	static class Helpers {

		/////////////////////////////////////////////////////////////////////////////
		public static Assembly GetAssembly { get { return Assembly.GetExecutingAssembly(); } }
		public static string Location { get { return GetAssembly.Location; } }
		public static string CodeBase { get { return GetAssembly.CodeBase.Substring( 8 ); } }
		public static string CodeBasePath { get { return Path.GetDirectoryName( GetAssembly.CodeBase.Substring( 8 ) ); } }
	}
}
