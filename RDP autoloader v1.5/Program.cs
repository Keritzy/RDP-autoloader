// Decompiled with JetBrains decompiler
// Type: VdsFileUploader.Program
// Assembly: VdsFileUploader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 82549BFD-A902-4CEB-96B1-21572FE5F771
// Assembly location: C:\Users\gorno\Desktop\vm\RDP AutoLoader v1.5\RDP autoloader v1.5.exe

using System;
using System.Windows.Forms;

namespace VdsFileUploader
{
  internal static class Program
  {
    [STAThread]
    private static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run((Form) new Form1());
    }
  }
}
