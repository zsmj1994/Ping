using System;
using System.Windows.Forms;
using System.Net;
using System.Threading;

namespace thefinal
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textBox1.Text = "www.liurongxin.cn";
            CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread pingTh = new Thread(Ping);
            pingTh.Start();
           // Ping();
        }

        private void Ping()
        {
            button1.Enabled = false;
            string hostName = textBox1.Text;
            richTextBox1.AppendText("Pinging  " + hostName + "...\n");
            int lostCount = 0;
            int totalTime = 0;
            int maximum = 0;
            int minimum = 1000;
            for(int i=0;i<4;i++)
            {
                MyPing p = new MyPing();
                string tmp;
                int spentTime = 0;
                tmp = p.PingHost(hostName, ref spentTime);
                if(tmp=="Time Out")
                {
                    lostCount++;
                }
                richTextBox1.AppendText(tmp + "\n");
                if (spentTime > maximum)
                    maximum = spentTime;
                if (spentTime < minimum)
                    minimum = spentTime;
                totalTime += spentTime;
                Thread.Sleep(1000);
            }
            double lostRate = Convert.ToDouble(lostCount)/4;
            int averageTime = Convert.ToInt32(Convert.ToDouble(totalTime)/4);
            string str = "发送4次，接收" + (4 - lostCount).ToString() + "次，丢失" + lostCount.ToString() + " <" + string.Format("{0:F2}", lostRate * 100) + "%丢失>\n";
            richTextBox1.AppendText(str);
            str = "最大延时:" + maximum.ToString() + " 最小延时:" + minimum.ToString() + "ms 平均:" + averageTime.ToString() + "ms\n";
            richTextBox1.AppendText(str);
            button1.Enabled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string hostName = Dns.GetHostName();   
            IPAddress[] addressList = Dns.GetHostAddresses(hostName);
            toolStripStatusLabel1.Text=("本机IP："+addressList[2].ToString());
        }
    }
}
