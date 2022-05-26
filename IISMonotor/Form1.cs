using Microsoft.Web.Administration;
using NUnit.Framework.Internal;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private string WebSiteName = "", nowStatus = "", URL = "";
        private int SleepTime;
        private ServerManager? sm;
        private bool OnWork = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(button1.Text == "Start")
            {
                OnStart();
            }
            else
            {
                OnStop();
            }
        }

        private void OnStart()
        {
            WebSiteName = textBox1.Text;
            SleepTime = (int)numericUpDown1.Value;
            URL = textBox2.Text;

            textBox1.Enabled = false;
            textBox2.Enabled = false;
            numericUpDown1.Enabled = false;
            button1.Text = "Stop";
            OnWork = true;
            Working();
        }

        private async void Working()
        {
            sm = new();
            var site = sm.Sites.First(website => website.Name.ToUpperInvariant() == WebSiteName.ToUpperInvariant());

            do
            {
                try
                {
                    label4.Text = "連線至 -> " + URL;
                    var request = await Connecting();

                    if (OnWork != true){ break; }

                    if (request != "OK")
                    {
                        request = "error";
                        label4.Text = "發現錯誤，重新啟動中...";
                        site.Stop();
                        await Task.Delay(1000);
                    }

                    if (!(site.State == ObjectState.Started || site.State == ObjectState.Starting))
                    {
                        site.Start();
                        nowStatus = "重新啟動完成";
                    }
                    else
                    {
                        nowStatus = request + "...";
                    }

                    for (int i = 0; i < SleepTime; i++)
                    {
                        label4.Text = nowStatus + "(" + i.ToString() + ")";
                        await Task.Delay(1000);
                        if (OnWork != true) { break; }
                    }
                }
                catch 
                {
                    label4.Text = "WebSite Not Found";
                    OnStop();
                    break;
                }
            }
            while (OnWork);
        }

        private void OnStop()
        {
            OnWork = false;
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            numericUpDown1.Enabled = true;
            button1.Text = "Start";
        }

        private async Task<string> Connecting()
        {
            try
            {
                HttpClientHandler clientHandler = new HttpClientHandler();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                { return true; };

                using (HttpClient httpClient = new HttpClient(clientHandler))
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(10);
                    using (HttpResponseMessage re = await httpClient.GetAsync(URL + "/API/CheckServer"))
                    {
                        using (HttpContent co = re.Content)
                        {
                            return await co.ReadAsStringAsync();
                        }
                    }
                }
            }
            catch
            {
                return "error";
            }
        }
    }
}