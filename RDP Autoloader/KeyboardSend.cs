// Decompiled with JetBrains decompiler
// Type: VdsFileUploader.KeyboardSend
// Assembly: VdsFileUploader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E07B404-AFB6-44C8-8A8A-653B83A195EB
// Assembly location: C:\Users\gorno\Desktop\vm\RDP Autoloader.exe

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace VdsFileUploader
{
  internal static class KeyboardSend
  {
    private const int KEYEVENTF_EXTENDEDKEY = 1;
    private const int KEYEVENTF_KEYUP = 2;
    private const byte VK_LWIN = 91;
    private const int WM_KEYDOWN = 256;
    private const int WM_KEYUP = 257;

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern ushort VkKeyScan(char ch);

    public static void SendText(IntPtr window, string text)
    {
      for (int index = 0; index < text.Length; ++index)
      {
        ushort num = KeyboardSend.VkKeyScan(text[index]);
        if ((int) (ushort) ((uint) num & 256U) == 256)
          KeyboardSend.KeyDown(window, Keys.LShiftKey, false);
        KeyboardSend.KeyDown(window, (Keys) (byte) KeyboardSend.VkKeyScan(text[index]), false);
        KeyboardSend.KeyUp(window, (Keys) (byte) KeyboardSend.VkKeyScan(text[index]), false);
        if ((int) (ushort) ((uint) num & 256U) == 256)
          KeyboardSend.KeyUp(window, Keys.LShiftKey, false);
      }
    }

    public static void KeyDown(IntPtr window, Keys vKey, bool special)
    {
      uint num = 16777216;
      if (!special)
        num = 0U;
      KeyboardSend.PostMessage(window, 256U, (uint) (byte) vKey, (uint) ((int) KeyboardSend.MapVirtualKey((uint) vKey, 0U) << 16 | 1) | num);
    }

    public static void KeyUp(IntPtr window, Keys vKey, bool special)
    {
      uint num = 16777216;
      if (!special)
        num = 0U;
      KeyboardSend.PostMessage(window, 257U, (uint) (byte) vKey, (uint) ((int) KeyboardSend.MapVirtualKey((uint) vKey, 0U) << 16 | -1073741823) | num);
    }
  }
}
