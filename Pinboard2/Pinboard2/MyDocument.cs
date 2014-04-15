using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Pinboard2
{
	public partial class MyDocument : MonoMac.AppKit.NSDocument
	{
		// Called when created from unmanaged code
		public MyDocument (IntPtr handle) : base (handle)
		{
		}
		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public MyDocument (NSCoder coder) : base (coder)
		{
		}

		public override void WindowControllerDidLoadNib (NSWindowController windowController)
		{
			base.WindowControllerDidLoadNib (windowController);
			
			// Add code to here after the controller has loaded the document window
		}
		// 
		// Save support:
		//    Override one of GetAsData, GetAsFileWrapper, or WriteToUrl.
		//
		
		// This method should store the contents of the document using the given typeName
		// on the return NSData value.
		public override NSData GetAsData (string documentType, out NSError outError)
		{
			outError = NSError.FromDomain (NSError.OsStatusErrorDomain, -4);
			return null;
		}
		// 
		// Load support:
		//    Override one of ReadFromData, ReadFromFileWrapper or ReadFromUrl
		//
		public override bool ReadFromData (NSData data, string typeName, out NSError outError)
		{
			outError = NSError.FromDomain (NSError.OsStatusErrorDomain, -4);
			return false;
		}
		// If this returns the name of a NIB file instead of null, a NSDocumentController 
		// is automatically created for you.
		public override string WindowNibName { 
			get {
				return "MyDocument";
			}
		}
	}
}

