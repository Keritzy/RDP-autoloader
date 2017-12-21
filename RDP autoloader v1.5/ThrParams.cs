// Decompiled with JetBrains decompiler
// Type: VdsFileUploader.ThrParams
// Assembly: VdsFileUploader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 82549BFD-A902-4CEB-96B1-21572FE5F771
// Assembly location: C:\Users\gorno\Desktop\vm\RDP AutoLoader v1.5\RDP autoloader v1.5.exe

using AxMSTSCLib;
using System.Threading;

namespace VdsFileUploader
{
  internal class ThrParams
  {
    private ManualResetEvent _ts;
    private AxMsRdpClient6NotSafeForScripting _rdp;

    public ManualResetEvent Ts
    {
      get
      {
        return this._ts;
      }
      set
      {
        this._ts = value;
      }
    }

    public AxMsRdpClient6NotSafeForScripting Rdp
    {
      get
      {
        return this._rdp;
      }
      set
      {
        this._rdp = value;
      }
    }
  }
}
