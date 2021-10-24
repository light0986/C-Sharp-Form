using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Random_Choice
{
    public partial class Form1 : Form
    {
        Random random;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) //讀檔
        {
            if (timer1.Enabled == false)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "讀取";
                openFileDialog.Filter = "(*.txt)|*.txt";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filepath = Path.GetFullPath(openFileDialog.FileName);
                    StreamReader streamReader = new StreamReader(filepath, Encoding.GetEncoding("Big5"));
                    {
                        string line;
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            if (line.ToString() != null)
                            {
                                if (line.Length != 0)
                                {
                                    listBox1.Items.Add(line);
                                }
                            }
                        }
                    }
                    streamReader.Close();
                }
                int count = listBox1.Items.Count;
                label2.Text = "總數: " + count;
                numericUpDown1.Maximum = count;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(timer1.Enabled == false && listBox1.Items.Count != 0)
            {
                team_num = 0;
                progressBar1.Maximum = listBox1.Items.Count;
                timer1.Enabled = true;
                numericUpDown1.Enabled = false;
            }
        }

        private int team_num;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (team_num != (int)numericUpDown1.Value)
            {
                random = new Random();
                int num = random.Next(0, listBox1.Items.Count);
                listBox2.Items.Add(listBox1.Items[num]);
                listBox1.Items.RemoveAt(num);
                progressBar1.Value = progressBar1.Value + 1;
                team_num++;
            }
            else
            {
                listBox2.Items.Add("====================");
                team_num = 0;
            }

            listBox2.SelectedIndex = listBox2.Items.Count - 1;

            if (progressBar1.Value == progressBar1.Maximum)
            {
                timer1.Enabled = false;
                numericUpDown1.Enabled = true;
                create_txt();
            }
        }

        private void create_txt()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "存檔";
            saveFileDialog.Filter = "(*.txt)|*.txt";
            saveFileDialog.FileName = System.DateTime.Now.ToString("yyyy_MM_dd");
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (saveFileDialog.FileName != "")
                {
                    if (File.Exists(saveFileDialog.FileName))
                    {
                        File.Delete(saveFileDialog.FileName);
                    }
                    int count_listbox2 = listBox2.Items.Count;
                    int count2 = 0;
                    string path = Path.GetFullPath(saveFileDialog.FileName);
                    StreamWriter file = new StreamWriter(path, true, Encoding.GetEncoding("Big5"));
                    {
                        do
                        {
                            if (listBox2.Items[count2].ToString() != "")
                            {
                                file.WriteLine(listBox2.Items[count2].ToString());
                            }
                            count2++;
                        }
                        while (count2 < count_listbox2);
                        file.Close();
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e) //重新開始
        {
            if (timer1.Enabled == false)
            {
                listBox1.Items.Clear();
                listBox2.Items.Clear();
                progressBar1.Value = 0;
            }
        }

        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            if(timer1.Enabled == true)
            {
                e.Cancel = true;
            }
        }
    }
}
