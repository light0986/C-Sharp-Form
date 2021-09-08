using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using CopyAndPaste;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace Auto_Fishing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static Graphics graphics;
        private static Bitmap bitmap;
        private static Bitmap bitmap2;
        private static Size s;

        CGlobalKeyboardHook _kbdHook = new CGlobalKeyboardHook();

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        private static extern int GetPixel(IntPtr hdc, Point p);

        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32", CharSet = CharSet.Unicode)]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        //移动鼠标 
        const int MOUSEEVENTF_MOVE = 0x0001;
        //模拟鼠标左键按下 
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        //模拟鼠标左键抬起 
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        //模拟鼠标右键按下 
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        //模拟鼠标右键抬起 
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        //模拟鼠标中键按下 
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        //模拟鼠标中键抬起 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        //标示是否采用绝对坐标 
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;

            s = new Size(pictureBox1.Size.Width, pictureBox1.Size.Height);
            hScrollBar1.Maximum = Screen.PrimaryScreen.Bounds.Width;
            vScrollBar1.Maximum = Screen.PrimaryScreen.Bounds.Height;

            _kbdHook.hook();
            _kbdHook.KeyDown += key_D;
            _kbdHook.KeyDown += K2_P;

            comboBox1.Items.Add("右鍵");
            comboBox1.Items.Add("左鍵");
            comboBox1.SelectedIndex = 0;
            timer1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bitmap2 = new Bitmap(bitmap);
            pictureBox3.Image = bitmap2;
        }

        private void timer1_click(object sender, EventArgs e)
        {
            bitmap = new Bitmap(pictureBox1.Size.Width,pictureBox1.Size.Height);
            graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(hScrollBar1.Value , vScrollBar1.Value, 0,0, s);
            pictureBox1.Image = bitmap;

            if (button2.Text == "停止(Ctrl+F11)")
            {
                if (get_bool(bitmap) == false)
                {
                    timer2.Enabled = true;
                }
            }
        }

        private bool get_bool(Bitmap bit)
        {
            bool b = false;
            for(int j = 0; j < bit.Height; j++)
            {
                for(int i = 0; i < bit.Width; i++)
                {
                    if(bit.GetPixel(i,j) == pictureBox2.BackColor)
                    {
                        b = true;
                    }
                }
            }
            if (b)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void M_D(object sender, MouseEventArgs e)
        {
            if(pictureBox3.Image != null)
            {
                pictureBox2.BackColor = bitmap2.GetPixel(e.X, e.Y);
            }
        }

        private void M_M(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                pictureBox2.BackColor = bitmap2.GetPixel(e.X, e.Y);
            }
        }

        private void key_D(object sender, KeyEventArgs e)
        {
            if(e.Control == true && e.KeyCode == Keys.F12)
            {
                button1_Click(null,null);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(button2.Text == "開始(Ctrl+F11)")
            {
                button2.Text = "停止(Ctrl+F11)";
                if (get_bool(bitmap) == false)
                {
                    timer2.Enabled = true;
                }
            }
            else
            {
                button2.Text = "開始(Ctrl+F11)";
                timer2.Enabled = false;
            }
        }

        private void K2_P(object sender, KeyEventArgs e)
        {
            if (e.Control == true && e.KeyCode == Keys.F11)
            {
                button2_Click(null, null);
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (get_bool(bitmap) == false)
            {
                if(comboBox1.Text == "右鍵")
                {
                    mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                }
                else if(comboBox1.Text == "左鍵")
                {
                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                }
            }
            else
            {
                timer2.Enabled = false;
            }
        }
    }
}
