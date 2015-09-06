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

using EnvDTE;
using EnvDTE100;
//using VSLangProj;

using System.Runtime.InteropServices;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;



namespace CustomToolBase {

	/////////////////////////////////////////////////////////////////////////////

	public abstract class CodeGeneratorWithSite : BaseCodeGenerator, IObjectWithSite {

		private Microsoft.VisualStudio.Shell.ServiceProvider globalProvider;
		private Microsoft.VisualStudio.Shell.ServiceProvider siteServiceProvider;
		private object site;


		/////////////////////////////////////////////////////////////////////////////

		protected Microsoft.VisualStudio.Shell.ServiceProvider SiteServiceProvider
		{
			get
			{
				if( this.siteServiceProvider == null ) {
					Microsoft.VisualStudio.OLE.Interop.IServiceProvider site = this.site as Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
					this.siteServiceProvider = new ServiceProvider( site );
				}
				return this.siteServiceProvider;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected object GetService( Guid service )
		{
			return this.SiteServiceProvider.GetService( service );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected object GetService( Type service )
		{
			return this.SiteServiceProvider.GetService( service );
		}


		/////////////////////////////////////////////////////////////////////////////

		protected Microsoft.VisualStudio.Shell.ServiceProvider GlobalServiceProvider
		{
			get
			{
				if( this.globalProvider == null ) {
					IVsHierarchy service = this.GetService( typeof( IVsHierarchy ) ) as IVsHierarchy;
					if( null != service ) {
						Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP = null;
						ErrorHandler.ThrowOnFailure( service.GetSite( out ppSP ) );
						if( ppSP != null ) {
							this.globalProvider = new ServiceProvider( ppSP );
						}
					}
				}

				return this.globalProvider;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected DTE Dte
		{
			get
			{
				DTE objectForIUnknown = null;
				IVsHierarchy service = this.GetService( typeof( IVsHierarchy ) ) as IVsHierarchy;
				if( service != null ) {
					Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP = null;

					//if (!NativeMethods.Failed(service.GetSite(out ppSP)) && (ppSP != null))
					if( 0 == service.GetSite( out ppSP ) && (ppSP != null) ) {
						Guid gUID = typeof( DTE ).GUID;
						IntPtr zero = IntPtr.Zero;
						ErrorHandler.ThrowOnFailure( ppSP.QueryService( ref gUID, ref gUID, out zero ) );
						if( zero != IntPtr.Zero ) {
							objectForIUnknown = Marshal.GetObjectForIUnknown( zero ) as DTE;
							Marshal.Release( zero );
						}
					}
				}
				return objectForIUnknown;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected IVsErrorList ErrorList
		{
			get
			{
				IVsErrorList objectForIUnknown = null;
				IVsHierarchy service = this.GetService( typeof( IVsHierarchy ) ) as IVsHierarchy;
				if( service != null ) {
					Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP = null;

					//if (!NativeMethods.Failed(service.GetSite(out ppSP)) && (ppSP != null))
					if( 0 == service.GetSite( out ppSP ) && (ppSP != null) ) {
						Guid gUID = typeof( SVsErrorList ).GUID;
						Guid riid = typeof( IVsErrorList ).GUID;
						IntPtr zero = IntPtr.Zero;
						ErrorHandler.ThrowOnFailure( ppSP.QueryService( ref gUID, ref riid, out zero ) );
						if( zero != IntPtr.Zero ) {
							objectForIUnknown = Marshal.GetObjectForIUnknown( zero ) as IVsErrorList;
							Marshal.Release( zero );
						}
					}
				}
				return objectForIUnknown;
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		protected virtual string CreateExceptionMessage( Exception exception )
		{
			StringBuilder builder = new StringBuilder( (exception.Message != null) ? exception.Message : string.Empty );
			for( Exception exception2 = exception.InnerException; exception2 != null; exception2 = exception2.InnerException ) {
				string message = exception2.Message;
				if( (message != null) && (message.Length > 0) ) {
					builder.AppendLine( " " + message );
				}
			}
			return builder.ToString();
		}


		/////////////////////////////////////////////////////////////////////////////

		protected override void Dispose( bool disposing )
		{
			try {
				if( this.siteServiceProvider != null ) {
					this.siteServiceProvider.Dispose();
					this.siteServiceProvider = null;
				}
				if( this.globalProvider != null ) {
					this.globalProvider.Dispose();
					this.globalProvider = null;
				}
			}
			finally {
				base.Dispose( disposing );
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual void GetSite( ref Guid riid, out IntPtr ppvSite )
		{
			// ******
			if( this.site == null ) {
				Marshal.ThrowExceptionForHR( -2147467259 );
			}

			// ******
			IntPtr iUnknown = Marshal.GetIUnknownForObject( this.site );

			try {
				ppvSite = IntPtr.Zero;
				Marshal.QueryInterface( iUnknown, ref riid, out ppvSite );
				if( ppvSite == IntPtr.Zero ) {
					Marshal.ThrowExceptionForHR( -2147467262 );
				}
			}
			//catch( Exception Exception ) {
			//	throw Exception;
			//}
			finally {

			// 1 Feb '15 - this is new 

				if( IntPtr.Zero != iUnknown ) {
					Marshal.Release( iUnknown );
				}
			}
		}


		/////////////////////////////////////////////////////////////////////////////

		public virtual void SetSite( object pUnkSite )
		{
			this.site = pUnkSite;
			this.siteServiceProvider = null;
		}


		/////////////////////////////////////////////////////////////////////////////

		//protected void SetWaitCursor()
		//{
		//	if (this.site != null)
		//	{
		//		IVsHierarchy service = this.GetService(typeof(IVsHierarchy)) as IVsHierarchy;
		//		if (service != null)
		//		{
		//			Microsoft.VisualStudio.OLE.Interop.IServiceProvider ppSP = null;
		//			if (service.GetSite(out ppSP) == 0)
		//			{
		//				IntPtr ptr;
		//				Guid gUID = typeof(SVsUIShell).GUID;
		//				Guid riid = typeof(IVsUIShell).GUID;
		//				if (ErrorHandler.Succeeded(ppSP.QueryService(ref gUID, ref riid, out ptr)))
		//				{
		//					IVsUIShell objectForIUnknown = Marshal.GetObjectForIUnknown(ptr) as IVsUIShell;
		//					if (objectForIUnknown != null)
		//					{
		//						objectForIUnknown.SetWaitCursor();
		//					}
		//					Marshal.Release(ptr);
		//				}
		//			}
		//		}
		//	}
		//}


		/////////////////////////////////////////////////////////////////////////////

		protected CodeGeneratorWithSite()
		{
		}


	}






}
