using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCPClient
{
    public partial class Form1 : Form
    {
        private string myIP;
        private Socket Sock;
        private Byte[] Data = new Byte[1024];
        private bool OnConnect = false;

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    myIP = ip.ToString();
                }
            }
            textBox1.Text = myIP;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(OnConnect == false)
            {
                try
                {
                    OnConnect = true;
                    button1.Text = "離線";
                    Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IAsyncResult rs = Sock.BeginConnect(IPAddress.Parse(textBox1.Text), int.Parse(textBox2.Text), null, null);
                    bool rt = rs.AsyncWaitHandle.WaitOne(5000);
                    if (!rt)
                    {
                        button1.Text = "連線";
                        listBox1.Items.Add("連線超時..");
                        OnConnect = false;
                    }
                    else if (Sock.Connected)
                    {
                        Sock.BeginReceive(Data, 0, 1024, SocketFlags.None, EndRead, null);
                    }
                    else
                    {
                        button1.Text = "連線";
                        listBox1.Items.Add("連線失敗..");
                        OnConnect = false;
                    }
                }
                catch { }
            }
            else
            {
                OnConnect = false;
                button1.Text = "連線";
                Sock.Shutdown(SocketShutdown.Send);
                Sock.Close();
                listBox1.Items.Add("您已離線..");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (OnConnect && textBox3.Text.Length > 0)
            {
                Send(textBox3.Text);
            }
            textBox3.Text = "";
        }

        public void EndRead(IAsyncResult I)
        {
            try
            {
                int len = Sock.EndReceive(I);
                Chat chat = JsonConvert.DeserializeObject<Chat>(Encoding.UTF8.GetString(Data, 0, len));
                listBox1.Items.Add(chat.User + ": " + chat.Message);
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
                Sock.BeginReceive(Data, 0, Data.Length, SocketFlags.None, EndRead, null);
            }
            catch { }
        }

        public void Send(String msg)
        {
            Chat chat = new Chat();
            chat.User = textBox4.Text;
            chat.Message = msg;
            string json = JsonConvert.SerializeObject(chat);
            Byte[] Buffer = Encoding.UTF8.GetBytes(json);
            Sock.BeginSend(Buffer, 0, Buffer.Length, 0, EndSend, Sock);
        }

        private void EndSend(IAsyncResult Result)
        {
            Sock.EndSend(Result);
        }

        private void OnClosed(object sender, FormClosedEventArgs e)
        {
            if (OnConnect)
            {
                OnConnect = false;
                Sock.Shutdown(SocketShutdown.Send);
                Sock.Close();
                listBox1.Items.Add("離線..");
            }
        }
    }

    class Chat
    {
        public string User { get; set; }

        public string Message { get; set; }
    }
}