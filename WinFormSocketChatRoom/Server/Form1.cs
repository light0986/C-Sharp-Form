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

namespace TCPTest
{
    public partial class Form1 : Form
    {
        private string myIP;
        private Socket Sock;
        private List<User> users = new List<User>();
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
                    button1.Text = "關閉";
                    Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPEndPoint EP = new IPEndPoint(IPAddress.Parse(textBox1.Text), int.Parse(textBox2.Text));
                    Sock.Bind(EP);
                    Sock.Listen(1000);
                    Sock.BeginAccept(newConnection, null);
                    listBox2.Items.Add("Server Start");
                    listBox2.SelectedIndex = listBox2.Items.Count - 1;
                }
                catch { }
            }
            else
            {
                OnConnect = false;
                button1.Text = "開啟";
                listBox2.Items.Add("Server Stop");
                listBox2.SelectedIndex = listBox2.Items.Count - 1;
                Sock.Close();
            }
        }

        private void newConnection(IAsyncResult Result)
        {
            try
            {
                Socket newSock = Sock.EndAccept(Result);
                Sock.BeginAccept(newConnection, newSock);
                User X = new User(newSock);
                users.Add(X);
                ListBox01Refresh();
                listBox2.Items.Add("有人連線");
                listBox2.SelectedIndex = listBox2.Items.Count - 1;
                X.Sock.BeginReceive(X.Data, 0, X.Data.Length, SocketFlags.None, EndRead, X);
            }
            catch { }
        }

        private void EndRead(IAsyncResult Result)
        {
            if (users.Count > 0)
            {
                try
                {
                    User X = (User)Result.AsyncState;
                    int Len = X.Sock.EndReceive(Result);
                    if (Len > 0)
                    {
                        String MSG = Encoding.UTF8.GetString(X.Data, 0, Len);
                        foreach (User Q in users)
                        {
                            Send(Q.Sock, MSG);
                        }
                        X.Sock.BeginReceive(X.Data, 0, X.Data.Length, SocketFlags.None, EndRead, X);
                    }
                    else
                    {
                        users.Remove(X);
                        listBox2.Items.Add("某人結束連線!");
                        listBox2.SelectedIndex = listBox2.Items.Count - 1;
                        ListBox01Refresh();

                        foreach (User Q in users)
                        {
                            Send(Q.Sock, "某人結束連線!");
                        }
                    }
                }
                catch { }
            }
        }

        private void ListBox01Refresh()
        {
            listBox1.Items.Clear();
            for (int i = 0; i < users.Count; i++)
            {
                listBox1.Items.Add(users[i].Sock.RemoteEndPoint);
            }
        }

        private void Send(Socket Sock, String msg)
        {
            Byte[] Buffer = Encoding.UTF8.GetBytes(msg);
            Sock.BeginSend(Buffer, 0, Buffer.Length, SocketFlags.None, EndSend, Sock);
        }

        private void EndSend(IAsyncResult Result)
        {
            ((Socket)Result.AsyncState).EndSend(Result);
        }

        private void OnClosed(object sender, FormClosingEventArgs e)
        {
            if(OnConnect)
            {
                listBox2.Items.Add("關閉伺服器後才可關閉");
                listBox2.SelectedIndex = listBox2.Items.Count - 1;
                e.Cancel = true;
            }
        }
    }

    class User
    {
        public Socket Sock;

        public Byte[] Data = new Byte[1024];

        public User(Socket s)
        {
            Sock = s;
        }
    }
}
