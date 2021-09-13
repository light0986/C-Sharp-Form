using CopyAndPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Auto_Fishing
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            AppUdate appUdate = new AppUdate();
            if (appUdate.PingTest())
            {
                if (appUdate.Check().ToString() != global::Auto_Fishing.Properties.Resources.version_string)
                {
                    appUdate.AutoRun();
                }
            }
            Application.Run(new Form1());
        }
    }
}
