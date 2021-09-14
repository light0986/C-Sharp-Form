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
        private static int count3 = 0; //呆滯太久校正回歸
        private static string first_Color_Type;

        CGlobalKeyboardHook _kbdHook = new CGlobalKeyboardHook(); //監聽

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

            numericUpDown1.Value = timer2.Interval; // 1.5秒
            numericUpDown2.Value = 30000; //30秒
            numericUpDown3.Value = 5; //5次

            comboBox1.Items.Add("右鍵");
            comboBox1.Items.Add("左鍵");
            comboBox1.SelectedIndex = 0;

            bitmap2 = new Bitmap(global::Auto_Fishing.Properties.Resources.IMG_1833,pictureBox2.Size.Width, pictureBox2.Size.Height); //內建圖檔
            pictureBox2.Image = bitmap2;//防呆
            pictureBox4.Image = P4_Pro(count);

            pictureBox3.BackColor = Properties.Settings.Default.C;
            pictureBox5.BackColor = Properties.Settings.Default.C; //存檔點讀取，關機調程式後，下次打開，紀錄還在。

            ColorType colorType = new ColorType();
            first_Color_Type = colorType.TypeName(pictureBox3.BackColor);
            label4.Text = "色系: " + first_Color_Type + " R: " + pictureBox5.BackColor.R + ",G: " + pictureBox5.BackColor.G + ",B: " + pictureBox5.BackColor.B;
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
                    if (get_bool(bitmap,pictureBox3.BackColor)) //還沒開始工作測試用
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
                    if (get_bool(bitmap,pictureBox5.BackColor)) //開始工作時的判斷
                    {
                        if (timer2.Enabled)
                        {
                            count--;
                        }
                        else
                        {
                            count = 0;
                        }
                        label3.Text = "有";
                        count2 = 0;
                        count3 = count3 + 100;
                        progressBar1.Value = count3;
                        pictureBox4.Image = P4_Pro(count); //人生跑馬燈龜苓膏


                    }
                    else
                    {
                        label3.Text = "無";
                        count++;
                        if (count >= 4) { count = 4; }
                        pictureBox4.Image = P4_Pro(count); //人生跑馬燈不是龜苓膏

                        if (count == 4) // 連續4個100毫秒做判定為不存在
                        {
                            if (timer2.Enabled == false) //當timer2不在使用狀態，避免timer1與timer2產生衝突
                            {
                                count2 = 0;
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
                    }

                    if(count3 == (int)numericUpDown2.Value) //呆滯校正
                    {
                        pictureBox5.BackColor = pictureBox3.BackColor;
                        count3 = 0;
                        progressBar1.Value = count3;
                    }
                }
            }
            catch
            {

            }
        }

        private bool get_bool(Bitmap bit , Color color) //整張圖判斷 = 個別pixel累積
        {
            bool b = false;
            ColorType colorType = new ColorType();
            for(int j = 0; j < bit.Height; j++)
            {
                for(int i = 0; i < bit.Width; i++)
                {
                    if (ColorAEqualColorB(bit.GetPixel(i, j), color)) //個別pixel判斷
                    {
                        if (button2.Text == "停止(Ctrl+F11)")
                        {
                            pictureBox5.BackColor = bit.GetPixel(i, j);
                            if ( colorType.TypeName(pictureBox5.BackColor) != first_Color_Type) { pictureBox5.BackColor = pictureBox3.BackColor; } //避免顏色被帶太遠
                        }
                        label4.Text = "色系: " + first_Color_Type + " R: " + pictureBox5.BackColor.R + ",G: " + pictureBox5.BackColor.G + ",B: " + pictureBox5.BackColor.B;
                        b = true;
                        break;
                    }
                }
            }
            return b;
        }

        private bool ColorAEqualColorB(Color colorA, Color colorB, byte errorRange = 15) //找到的顏色,標準的顏色,容許誤差
        {
            return colorA.A <= colorB.A + errorRange && colorA.A >= colorB.A - errorRange &&
                colorA.R <= colorB.R + errorRange && colorA.R >= colorB.R - errorRange &&
                colorA.G <= colorB.G + errorRange && colorA.G >= colorB.G - errorRange &&
                colorA.B <= colorB.B + errorRange && colorA.B >= colorB.B - errorRange;

        }

        private Bitmap P4_Pro(int num) //人生跑馬燈
        {
            Bitmap b = new Bitmap(pictureBox4.Size.Width,pictureBox4.Size.Height);
            if (num == 1) { b = global::Auto_Fishing.Properties.Resources.P01; }
            else if (num == 2) { b = global::Auto_Fishing.Properties.Resources.P02; }
            else if (num == 3) { b = global::Auto_Fishing.Properties.Resources.P03; }
            else if (num == 4) { b = global::Auto_Fishing.Properties.Resources.P04; }
            else { b = global::Auto_Fishing.Properties.Resources.P00; }
            return b;
        }

        private void M_D(object sender, MouseEventArgs e) //pic2按下時
        {
            if (bitmap2 != null && button2.Text == "開始(Ctrl+F11)")
            {
                if (e.Button == MouseButtons.Left) //必須為左鍵
                {
                    Properties.Settings.Default.C = bitmap2.GetPixel(e.X, e.Y);
                    Properties.Settings.Default.Save();//存檔點讀取，關機調程式後，下次打開，紀錄還在。

                    pictureBox3.BackColor = Properties.Settings.Default.C;
                    pictureBox5.BackColor = Properties.Settings.Default.C;

                    ColorType colorType = new ColorType();
                    first_Color_Type = colorType.TypeName(pictureBox3.BackColor);
                    label4.Text = "色系: " + first_Color_Type + " R: " + pictureBox5.BackColor.R + ",G: " + pictureBox5.BackColor.G + ",B: " + pictureBox5.BackColor.B;
                }
            }
        }

        private void M_M(object sender, MouseEventArgs e) //pic2按住並移動時
        {
            if(bitmap2 != null && button2.Text == "開始(Ctrl+F11)")
            {
                if (e.Button == MouseButtons.Left) //必須為左鍵
                {
                    int i = e.X; int j = e.Y; //超出範圍的防呆
                    if(i < 0) { i = 0; }
                    if(i > pictureBox2.Width -1) { i = pictureBox2.Width - 1; }
                    if (j < 0) { j = 0; }
                    if (j > pictureBox2.Height - 1) { j = pictureBox2.Height - 1; }

                    pictureBox3.BackColor = bitmap2.GetPixel(e.X, e.Y);
                    pictureBox5.BackColor = bitmap2.GetPixel(e.X, e.Y);

                    ColorType colorType = new ColorType();
                    first_Color_Type = colorType.TypeName(pictureBox3.BackColor);
                    label4.Text = "色系: " + first_Color_Type + " R: " + pictureBox5.BackColor.R + ",G: " + pictureBox5.BackColor.G + ",B: " + pictureBox5.BackColor.B;
                }
            }
        }

        private void M_U(object sender, MouseEventArgs e)
        {
            Properties.Settings.Default.C = bitmap2.GetPixel(e.X, e.Y);
            Properties.Settings.Default.Save(); //存檔點讀取，關機調程式後，下次打開，紀錄還在。

            pictureBox3.BackColor = Properties.Settings.Default.C;
            pictureBox5.BackColor = Properties.Settings.Default.C;
        }

        private void num_vc(object sender, EventArgs e) //numericUpDown1調整時
        {
            try
            {
                timer2.Interval = Convert.ToInt32(numericUpDown1.Value);
            }
            catch { numericUpDown1.Value = 1500; } //防呆，強迫可轉int32的資料型態
        }

        private void num2_valuechange(object sender, EventArgs e) //numericUpDown2調整時 
        {
            progressBar1.Maximum = (int)numericUpDown2.Value;
        }

        private void num3_valuechange(object sender, EventArgs e) //numericUpDown3調整時 
        {
            progressBar2.Maximum = (int)numericUpDown3.Value;
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
            pictureBox2.Image = bitmap2; //截圖
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
                count = 0;
                count2 = 0;
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
            if (count < 1)
            {
                count2 = 0;
                progressBar2.Value = count2;
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

                if (count2 == numericUpDown3.Value) //N次空拋竿表示還不穩定
                {
                    pictureBox5.BackColor = pictureBox3.BackColor;
                    count2 = 0;
                }
                else
                {
                    count2++;
                }
                progressBar2.Value = count2;
            }
        }

        private void label3_textchange(object sender, EventArgs e) //沒有呆滯時
        {
            if(button2.Text == "停止(Ctrl+F11)") //關注開始工作後
            {
                count3 = 0;
                progressBar1.Value = count3;
            }
        }

        private void F_Closing(object sender, FormClosedEventArgs e) //關閉時timer1跟2都停止，沒有意義，寫爽的
        {
            timer1.Enabled = false;
            timer2.Enabled = false;
        }
    }
}
