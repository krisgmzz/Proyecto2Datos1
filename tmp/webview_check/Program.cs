using System;
using Microsoft.Web.WebView2.Core;

class Program {
  static int Main(){
    try{
      var v = CoreWebView2Environment.GetAvailableBrowserVersionString(null);
      Console.WriteLine("AvailableBrowserVersion: " + (v ?? "<null>"));
      var v2 = CoreWebView2Environment.GetAvailableBrowserVersionString("C:\\Program Files\\Microsoft\\EdgeWebView\\Application");
      Console.WriteLine("CheckPath: " + (v2 ?? "<null>"));
      var v3 = CoreWebView2Environment.GetAvailableBrowserVersionString("C:\\Program Files (x86)\\Microsoft\\EdgeWebView\\Application");
      Console.WriteLine("CheckPath x86: " + (v3 ?? "<null>"));
      return 0;
    }catch(Exception ex){
      Console.WriteLine("Exception: " + ex);
      return 2;
    }
  }
}
