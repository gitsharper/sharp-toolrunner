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
using System.Reflection;
using System.IO;
using System.Text;

namespace Utilities {

	///////////////////////////////////////////////////////////////////////////

	public static class LibInfo {

		//public static byte [] publicKey;

		/*
				version = 1.2.3.4
				
				1	-> major
				2	-> minor
				3	-> build
				4 -> revision
						major revision	-> high order 16 bits of revision
						minor revision	-> low order 16 bits of revision
		
		*/

		///////////////////////////////////////////////////////////////////////////

		//public static Assembly GetAssembly { get { return Assembly.GetExecutingAssembly(); } }
		public static Assembly GetAssembly { get { return Assembly.GetCallingAssembly(); } }

		public static string Location { get { return GetAssembly.Location; } }
		public static string CodeBase { get { return GetAssembly.CodeBase.Substring( 8 ); } }
		public static string CodeBasePath { get { return Path.GetDirectoryName( GetAssembly.CodeBase.Substring( 8 ) ); } }

		///////////////////////////////////////////////////////////////////////////

		public static string Version
		{
			get
			{
				Assembly asm = GetAssembly;
				string[] parts = asm.FullName.Split( ',' );
				return parts [ 1 ];
			}
		}


		///////////////////////////////////////////////////////////////////////////

		public static byte [] PublicKeyBlob
		{
			get
			{
				Assembly asm = GetAssembly;

				// ******	
				byte [] data = asm.GetName().GetPublicKey();

				int blobSize = data.Length - 12;
				byte[] blob = new byte [ blobSize ];
				Buffer.BlockCopy( data, 12, blob, 0, blobSize );

				// ******	
				return blob;
			}
		}


		///////////////////////////////////////////////////////////////////////////

		static LibInfo()
		{
			byte [] publicKey = GetAssembly.GetName().GetPublicKey();
		}

	}



}
