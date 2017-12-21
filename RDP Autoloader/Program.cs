// Decompiled with JetBrains decompiler
// Type: VdsFileUploader.Program
// Assembly: VdsFileUploader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E07B404-AFB6-44C8-8A8A-653B83A195EB
// Assembly location: C:\Users\gorno\Desktop\vm\RDP Autoloader.exe

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
