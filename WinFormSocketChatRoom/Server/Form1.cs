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

namespace TCPTest
{
    public partial class Form1 : Form
    {
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
            /*
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    textBox1.Text = ip.ToString();
                }
            }
            */
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
                    IPEndPoint EP = new IPEndPoint(IPAddress.Any, int.Parse(textBox2.Text));
                    Sock.Bind(EP);
                    Sock.Listen(1000);
                    Sock.BeginAccept(newConnection, null);
                    listBox2.Items.Add("Server Start");
                    listBox2.SelectedIndex = listBox2.Items.Count - 1;
                }
                catch
                {
                    OnConnect = false;
                    button1.Text = "開啟";
                    Sock.Close();
                }
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

                ServerTalk("Welcome!" + newSock.RemoteEndPoint);
                X.Sock.BeginReceive(X.Data, 0, X.Data.Length, 0, EndRead, X);
            }
            catch { }
        }

        private void EndRead(IAsyncResult Result)
        {
            User X = (User)Result.AsyncState;
            int Len = X.Sock.EndReceive(Result);
            if (Len > 0)
            {
                String MSG = Encoding.UTF8.GetString(X.Data, 0, Len);
                Chat chat = JsonConvert.DeserializeObject<Chat>(MSG);
                listBox2.Items.Add(chat.User + ": " + chat.Message);
                listBox2.SelectedIndex = listBox2.Items.Count - 1;

                foreach (User Q in users)
                {
                    Send(Q.Sock, MSG);
                }
                X.Sock.BeginReceive(X.Data, 0, X.Data.Length, 0, EndRead, X);
            }
            else
            {
                if(users.IndexOf(X) != -1)
                {
                    string Name = X.Sock.RemoteEndPoint.ToString();
                    X.Sock.Close();
                    users.Remove(X);
                    ListBox01Refresh();
                    ServerTalk("Farewell!" + Name);
                }
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

        private void button2_Click(object sender, EventArgs e)
        {
            if (OnConnect && textBox3.Text.Length > 0)
            {
                ServerTalk(textBox3.Text);
            }
            textBox3.Text = "";
        }

        private void ServerTalk(string Msg)
        {
            Chat chat = new Chat();
            chat.User = "System:";
            chat.Message = Msg;
            string json = JsonConvert.SerializeObject(chat);

            listBox2.Items.Add("System:" + ": " + Msg);
            listBox2.SelectedIndex = listBox2.Items.Count - 1;

            foreach (User Q in users)
            {
                Send(Q.Sock, json);
            }
        }

        public void Send(Socket s, String msg)
        {
            Byte[] Buffer = Encoding.UTF8.GetBytes(msg);
            s.BeginSend(Buffer, 0, Buffer.Length, SocketFlags.None, EndSend, s);
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

    class Chat
    {
        public string User { get; set; }

        public string Message { get; set; }
    }
}
