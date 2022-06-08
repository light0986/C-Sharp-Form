using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CopyAndPaste
{
    class CGlobalKeyboardHook
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);
        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);
        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref keyboardHookStruct IParam);
        [DllImport("user32.dll")]
        static extern short GetKeyState(int nCode);
        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string IpFileName);

        public delegate int keyboardHookProc(int code, int wParam, ref keyboardHookStruct IParam);

        public struct keyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        const int VK_SHIFT = 0x10;
        const int VK_CONTROL = 0x11;
        const int VK_MENU = 0x12;

        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_SYSKEYDOWN = 0x104;
        const int WM_SYSKEYUP = 0x105;

        private keyboardHookProc khp;
        IntPtr hhook = IntPtr.Zero;

        public event KeyEventHandler KeyDown;
        public event KeyEventHandler KeyUp;


        public CGlobalKeyboardHook()
        {
            khp = new keyboardHookProc(hookproc);
        }

        public void hook()
        {
            IntPtr hInstance = LoadLibrary("User32");
            hhook = SetWindowsHookEx(WH_KEYBOARD_LL, khp, hInstance, 0);
        }

        public void unhook()
        {
            UnhookWindowsHookEx(hhook);
        }

        public int hookproc(int code, int wParam, ref keyboardHookStruct IParam)
        {
            if (code >= 0)
            {
                Keys key = (Keys)IParam.vkCode;
                if ((GetKeyState(VK_CONTROL) & 0x80) != 0)
                    key |= Keys.Control;
                if ((GetKeyState(VK_MENU) & 0x80) != 0)
                    key |= Keys.Alt;
                if ((GetKeyState(VK_SHIFT) & 0x80) != 0)
                    key |= Keys.Shift;

                KeyEventArgs kea = new KeyEventArgs(key);
                if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null))
                {
                    KeyDown(this, kea);
                }
                else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null))
                {
                    KeyUp(this, kea);
                }
                if (kea.Handled)
                    return 1;

            }

            return CallNextHookEx(hhook, code, wParam, ref IParam);
        }

    }

    class ColorType
    {
        public string TypeName(Color color)
        {
            int rg = color.R - color.G; int rb = color.R - color.B; int gb = color.G - color.B;
            if (rg < 0) { rg = rg * -1; }
            if (rb < 0) { rb = rb * -1; }
            if (gb < 0) { gb = gb * -1; }
            int a = rg + rb + gb; //避免RGB太過相似的誤差

            if (color.R > color.G && color.R > color.B && color.G <= 128 && color.B <= 128 && a >= 30) { return "紅棕"; }
            else if (color.G > color.R && color.G > color.B && color.R <= 128 && color.B <= 128 && a >= 30) { return "茶綠"; }
            else if (color.B > color.R && color.B > color.G && color.R <= 128 && color.G <= 128 && a >= 30) { return "藍靛"; }
            else if (color.R < color.G && color.R < color.B && color.G >= 128 && color.B >= 128 && a >= 30) { return "青綠"; }
            else if (color.G < color.R && color.G < color.B && color.R >= 128 && color.B >= 128 && a >= 30) { return "桃紫"; }
            else if (color.B < color.R && color.B < color.G && color.R >= 128 && color.G >= 128 && a >= 30) { return "澄黃"; }
            else { return "二質"; }
        }
    }

    class AppUdate
    {
        public bool PingTest() //Ping Github
        {
            System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
            System.Net.NetworkInformation.PingReply pingStatus = ping.Send(IPAddress.Parse("140.82.114.4"), 1000);
            if (pingStatus.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object Check()
        {
            try
            {
                object version = "";
                WebRequest request = WebRequest.Create("https://raw.githubusercontent.com/light0986/C-Sharp-Form/main/MAF/Version.txt");
                WebResponse response = request.GetResponse();
                Stream datastream = response.GetResponseStream(); 
                Encoding ec = Encoding.UTF8;
                StreamReader reader = new StreamReader(datastream, ec);
                version = reader.ReadLine();
                reader.Close();
                datastream.Close();
                response.Close();
                return version;
            }
            catch { return "err"; }
        }

        private string sources_file = Application.ExecutablePath; //完整路徑
        private string defualt_path = Application.StartupPath + "\\App_DownLoad.exe";


        public void AutoRun()
        {
            try
            {
                WebClient wc = new WebClient();
                wc.DownloadFile("https://github.com/light0986/C-Sharp-Form/raw/main/MAF/App_DownLoad.exe", defualt_path); //下載安裝檔

                Process P_new = new Process();
                P_new.StartInfo = new ProcessStartInfo("cmd.exe", "/C choice /C Y /N /D Y /T 1 & " + "\"" + defualt_path + "\""); //排成執行安裝檔
                P_new.StartInfo.CreateNoWindow = true;
                P_new.StartInfo.UseShellExecute = false;
                P_new.Start();

                Process P_sources = new Process();
                P_sources.StartInfo = new ProcessStartInfo("cmd.exe", "/C choice /C Y /N /D Y /T 1 & Del " + "\"" + sources_file + "\""); //舊檔案自殺
                P_sources.StartInfo.CreateNoWindow = true;
                P_sources.StartInfo.UseShellExecute = false;
                P_sources.Start();

                Application.Exit();
            }
            catch
            {

            }
        }
    }
}
