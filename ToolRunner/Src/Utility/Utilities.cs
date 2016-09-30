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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Utilities {

	/////////////////////////////////////////////////////////////////////////////

	public class Helpers {

		/////////////////////////////////////////////////////////////////////////////

		public static string SafeStringFormat( string fmt, params object [] args )
		{
			// ******
			if( null == fmt ) {
				return string.Empty;
			}

			// ******
			if( args.Length > 0 ) {
				try {
					return string.Format( fmt, args );
				}
				catch {
					///return string.Format( "[SafeStringFormat: exception calling string.Format()] format string: {0}", fmt );
					return string.Format( "{0}", fmt );
				}
			}

			// ******
			return fmt;
		}


	}
}
