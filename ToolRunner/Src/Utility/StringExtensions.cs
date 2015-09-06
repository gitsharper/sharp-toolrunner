using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utilities {

	public static class StringExtensions {

		/////////////////////////////////////////////////////////////////////////////

		public static string AssureNoLeadingDot( this string str )
		{
			var strOut = str?.Trim() ?? string.Empty;
			while( !string.IsNullOrEmpty( strOut ) && '.' == strOut.First() ) {
				strOut = strOut.Substring( 1 );
			}
			return strOut;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string AssureLeadingDot( this string str, bool addForEmptyStr = false )
		{
			var strOut = str?.Trim() ?? string.Empty;

			if( string.IsNullOrEmpty(strOut) && addForEmptyStr ) {
				return ".";
			}

			if( !string.IsNullOrEmpty(strOut) && '.' != strOut.First() ) {
				strOut = "." + strOut;
			}

			return strOut;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string Combine( this IEnumerable<string> strs, string jointStr )
		{
			var sb = new StringBuilder { };

			var lastValidIndex = strs.Count() - 1;
			for( var i = 0; i <= lastValidIndex; i += 1 ) {
				var value = strs.ElementAt( i );
				sb.Append( string.IsNullOrEmpty( value ) ? string.Empty : value );
				if( i < lastValidIndex ) {
					sb.Append( jointStr );
				}
			}

			return sb.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		public static string RemoveTrailingSeparator( this string path )
		{
			// ******
			if( string.IsNullOrEmpty( path ) ) {
				return string.Empty;
			}

			// ******
			var lastCh = path [ path.Length - 1 ];
			return '\\' == lastCh || '/' == lastCh ? path.Substring( 0, path.Length - 1 ) : path;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static int CountWhile( this string str, Func<char, bool> predicate )
		{
			// ******
			for( int i = 0; i < str.Length; i += 1 ) {
				if( !predicate( str [ i ] ) ) {
					return i;
				}
			}

			// ******
			return str.Length;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static bool Contains( this string str, char c, params char [] otherChars )
		{
			// ******
			if( str.IndexOf( c ) >= 0 ) {
				return true;
			}
			else if( otherChars.Length > 0 && str.IndexOfAny( otherChars ) >= 0 ) {
				return true;
			}

			// ******
			return false;
		}


		/////////////////////////////////////////////////////////////////////////////

		//
		// returns string (not IEnumerable) after skipping
		//

		public static string SkipWhile( this string str, Func<char, bool> predicate )
		{
			// ******
			for( int i = 0; i < str.Length; i += 1 ) {
				if( !predicate( str [ i ] ) ) {
					//
					// remainder of string
					//
					return str.Substring( i );
				}
			}

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		//
		// returns string (not IEnumerable) of matching
		//

		public static string TakeWhile( this string str, Func<char, bool> predicate )
		{
			// ******
			for( int i = 0; i < str.Length; i += 1 ) {
				if( !predicate( str [ i ] ) ) {
					//
					// start to previous position in string
					//
					return str.Substring( 0, i );
				}
			}

			// ******
			return string.Empty;
		}


		/////////////////////////////////////////////////////////////////////////////

		public static bool GetInt32( this string str, out int value )
		{
			int charCount;
			return GetInt32( str, out charCount, out value );
		}


		/////////////////////////////////////////////////////////////////////////////

		public static bool GetInt32( string str, out int charCount, out int value )
		{
			// ******
			var sb = new StringBuilder { };

			var index = 0;
			while( char.IsDigit( str [ index ] ) ) {
				sb.Append( str [ index ] );
				index += 1;
			}

			// ******
			charCount = index;
			if( 0 == sb.Length ) {
				value = 0;
				return false;
			}
			else {
				value = Convert.ToInt32( sb.ToString() );
				return true;
			}
		}


	}
}
