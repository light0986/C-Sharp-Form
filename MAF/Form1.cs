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
using System.Reflection;

namespace Auto_Fishing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private static Graphics graphics; //螢幕截圖用的畫布
        private static Bitmap bitmap; //pic1用的
        private static Bitmap bitmap2; //pic2用的
        private static Size s; //畫布的大小定義
        private static int count = 0; //時間誤差
        private static int count2 = 0; //顏色跑掉校正回歸
        private static Color first_Color;

        CGlobalKeyboardHook _kbdHook = new CGlobalKeyboardHook(); //Class1

        //[DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //private static extern bool SetCursorPos(int x, int y); 
        //滑鼠強制移動到畫面該位置，目前沒有用

        [DllImport("user32", CharSet = CharSet.Unicode)]
        private static extern int mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo); //模擬滑鼠訊號 
        const int MOUSEEVENTF_MOVE = 0x0001;
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;

            s = new Size(pictureBox1.Size.Width, pictureBox1.Size.Height); //graphics.CopyFromScreen使用
            hScrollBar1.Maximum = Screen.PrimaryScreen.Bounds.Width; //0~螢幕的寬
            vScrollBar1.Maximum = Screen.PrimaryScreen.Bounds.Height; //0~螢幕的高

            _kbdHook.hook(); //開啟鍵盤訊號監視
            _kbdHook.KeyDown += key_D; //監視系統訊號是否可觸發CTRL + F12 = button1
            _kbdHook.KeyDown += K2_P; //監視系統訊號是否可觸發CTRL + F11 = button2

            numericUpDown1.Value = timer2.Interval;
            comboBox1.Items.Add("右鍵");
            comboBox1.Items.Add("左鍵");
            comboBox1.SelectedIndex = 0;

            bitmap2 = new Bitmap(global::Auto_Fishing.Properties.Resources.IMG_1833,pictureBox3.Size.Width, pictureBox3.Size.Height); //內建圖檔
            pictureBox3.Image = bitmap2;//防呆
            label4.Text = "A=" + pictureBox2.BackColor.A.ToString() + ",R=" + pictureBox2.BackColor.R.ToString()+ ",G=" + pictureBox2.BackColor.G.ToString()+ ",B=" + pictureBox2.BackColor.B.ToString();
        }

        private void timer1_click(object sender, EventArgs e) //螢幕截圖 每100毫秒計算一次
        {
            try
            {
                bitmap = new Bitmap(pictureBox1.Size.Width, pictureBox1.Size.Height); //定義新bmp大小
                graphics = Graphics.FromImage(bitmap); //畫布指向bmp
                graphics.CopyFromScreen(hScrollBar1.Value, vScrollBar1.Value, 0, 0, s, CopyPixelOperation.SourceCopy); //畫布是拷貝至螢幕(x,y,0,0,size,色塊複製方式)
                pictureBox1.Image = bitmap; //pic1顯示畫布後的bmp

                if (button2.Text == "開始(Ctrl+F11)") //尚未開始工作時
                {
                    count = 0;
                    if (get_bool(bitmap)) //還沒開始工作測試用
                    {
                        label3.Text = "有";
                    }
                    else
                    {
                        label3.Text = "無";
                    }
                }
                else //開始工作時
                {
                    if (get_bool(bitmap)) //開始工作時的判斷
                    {
                        label3.Text = "有";
                        count = 0;
                        count2 = 0;
                    }
                    else
                    {
                        label3.Text = "無";
                        count++;
                        if (count == 4) // 連續4個100毫秒做判定為不存在
                        {
                            if (timer2.Enabled == false) //當timer2不在使用狀態，避免每0,5秒拋竿一次
                            {
                                timer2.Enabled = true; //使用timer2
                                if (comboBox1.Text == "右鍵")
                                {
                                    mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                                }
                                else if (comboBox1.Text == "左鍵")
                                {
                                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                                }
                            }
                        }

                        if(count > 7) //不讓你無限上綱加上去，防呆 {7 = 0111}
                        {
                            count = 6; //{6 = 0110} 沒有任何意義，只是我爽
                        }
                    }
                }
            }
            catch
            {

            }
        }

        private bool get_bool(Bitmap bit) //整張圖判斷 = 個別pixel累積
        {
            bool b = false;
            for(int j = 0; j < bit.Height; j++)
            {
                for(int i = 0; i < bit.Width; i++)
                {
                    if (ColorAEqualColorB(bit.GetPixel(i, j), pictureBox2.BackColor)) //個別pixel判斷
                    {
                        pictureBox2.BackColor = bit.GetPixel(i, j);
                        label4.Text = "A=" + pictureBox2.BackColor.A.ToString() + ",R=" + pictureBox2.BackColor.R.ToString() + ",G=" + pictureBox2.BackColor.G.ToString() + ",B=" + pictureBox2.BackColor.B.ToString();
                        b = true;
                        break;
                    }
                }
            }
            return b;
        }

        private bool ColorAEqualColorB(Color colorA, Color colorB, byte errorRange = 10) //找到的顏色,標準的顏色,容許誤差
        {
            return colorA.A <= colorB.A + errorRange && colorA.A >= colorB.A - errorRange &&
                colorA.R <= colorB.R + errorRange && colorA.R >= colorB.R - errorRange &&
                colorA.G <= colorB.G + errorRange && colorA.G >= colorB.G - errorRange &&
                colorA.B <= colorB.B + errorRange && colorA.B >= colorB.B - errorRange;

        }

        private void M_D(object sender, MouseEventArgs e) //pic2按下時
        {
            if (bitmap2 != null)
            {
                if (e.Button == MouseButtons.Left) //必須為左鍵
                {
                    first_Color = bitmap2.GetPixel(e.X, e.Y);
                    pictureBox2.BackColor = first_Color;
                    label4.Text = "A=" + pictureBox2.BackColor.A.ToString() + ",R=" + pictureBox2.BackColor.R.ToString() + ",G=" + pictureBox2.BackColor.G.ToString() + ",B=" + pictureBox2.BackColor.B.ToString();
                }
            }
        }

        private void M_M(object sender, MouseEventArgs e) //pic2按住並移動時
        {
            if(bitmap2 != null)
            {
                if (e.Button == MouseButtons.Left) //必須為左鍵
                {
                    first_Color = bitmap2.GetPixel(e.X, e.Y);
                    pictureBox2.BackColor = first_Color;
                    label4.Text = "A=" + pictureBox2.BackColor.A.ToString() + ",R=" + pictureBox2.BackColor.R.ToString() + ",G=" + pictureBox2.BackColor.G.ToString() + ",B=" + pictureBox2.BackColor.B.ToString();
                }
            }
        }

        private void num_vc(object sender, EventArgs e) //numericUpDown1調整時
        {
            try
            {
                timer2.Interval = Convert.ToInt32(numericUpDown1.Value);
            }
            catch { numericUpDown1.Value = 1500; } //防呆，強迫可轉int32的資料型態
        }

        private void key_D(object sender, KeyEventArgs e) //鍵盤按鍵控制
        {
            if(e.Control == true && e.KeyCode == Keys.F12) //CTRL + F12 
            {
                button1_Click(null,null); //button1觸發
            }
        }

        private void button1_Click(object sender, EventArgs e) //我是buttton1
        {
            bitmap2 = new Bitmap(bitmap);
            pictureBox3.Image = bitmap2; //截圖
            label2.Text = "鎖定顏色:";
        }

        private void K2_P(object sender, KeyEventArgs e) //鍵盤按鍵控制button2觸發
        {
            if (e.Control == true && e.KeyCode == Keys.F11) //CTRL + F11
            {
                button2_Click(null, null); //button2觸發
            }
        }

        private void button2_Click(object sender, EventArgs e) //我是buttton2
        {
            if(button2.Text == "開始(Ctrl+F11)") //開始工作
            {
                button2.Text = "停止(Ctrl+F11)";
                timer2.Enabled = true;
            }
            else
            {
                button2.Text = "開始(Ctrl+F11)"; //結束工作
                timer2.Enabled = false;
            }
        }

        private void timer2_Tick(object sender, EventArgs e) //每(numericUpDown1.Value)毫秒觸發一次，1000毫秒 = 1秒
        {
            if (label3.Text == "有")
            {
                count = 0; //時間誤差歸零
                count2 = 0;
                timer2.Enabled = false; //自我關閉
            }
            else
            {
                if (comboBox1.Text == "右鍵")
                {
                    mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                }
                else if (comboBox1.Text == "左鍵")
                {
                    mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                }
                count2++;
                if(count2 == 5) //4次拋竿皆無
                {
                    pictureBox2.BackColor = first_Color;
                }
            }
        }

        private void F_Closing(object sender, FormClosedEventArgs e) //關閉時timer1跟2都停止，沒有意義，寫爽的
        {
            timer1.Enabled = false;
            timer2.Enabled = false;
        }
    }
}
