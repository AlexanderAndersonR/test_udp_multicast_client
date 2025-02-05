using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.Sockets;
using System.Net;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace test_udp_multicast
{
    public partial class Form1 : Form
    {
        bool flag = false;
        public Form1()
        {
            InitializeComponent();
            textBox2.Text = Properties.Settings.Default.ip;
            textBox3.Text = Properties.Settings.Default.port.ToString();

        }
        //public void SendMessage(string message)
        //{
        //    var data = Encoding.Default.GetBytes(message);
        //    using (var udpClient = new UdpClient(AddressFamily.InterNetwork))
        //    {
        //        var address = IPAddress.Parse("192.168.2.232");
        //        var ipEndPoint = new IPEndPoint(address, 1234);
        //        udpClient.Send(data, data.Length, ipEndPoint);
        //        udpClient.Close();
        //    }
        //}
        delegate void SetTextDeleg(string text);
        private void si_DataReceived(string data)
        {
            textBox1.Text += data + "\r\n";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            flag = true;
            Task.Run(ReceiveMessageAsync);
        }
        async Task ReceiveMessageAsync()
        {
            try
            {
                //int test = Properties.Settings.Default.port;
                UdpClient receiver = new UdpClient(Properties.Settings.Default.port);
                receiver.JoinMulticastGroup(IPAddress.Parse(Properties.Settings.Default.ip));
                //string teststr = Properties.Settings.Default.ip;
                receiver.MulticastLoopback = false;
                while (flag)
                {
                    var result = await receiver.ReceiveAsync();
                    string message = Encoding.UTF8.GetString(result.Buffer);
                    BeginInvoke(new SetTextDeleg(si_DataReceived), message);
                }
                receiver.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                flag = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            flag = false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            flag = false;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.port = Convert.ToInt32(textBox3.Text);
            Properties.Settings.Default.Save();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ip = textBox2.Text;
            Properties.Settings.Default.Save();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 30000)
                textBox1.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SendMessageAsync();
        }
        async Task SendMessageAsync()
        {
            using var sender = new UdpClient();
            string? message = textBox4.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                // и отправляем в группу
                await sender.SendAsync(data, new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.ip), Properties.Settings.Default.port));
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedItem.ToString())
            {
                case "SATD":
                    textBox2.Text = "239.192.0.3";
                    textBox3.Text = "60003";
                    break;
                case "NAVD":
                    textBox2.Text = "239.192.0.4";
                    textBox3.Text = "60004";
                    break;
                case "BAM1":
                    textBox2.Text = "239.192.0.17";
                    textBox3.Text = "60017";
                    break;
                case "BAM2":
                    textBox2.Text = "239.192.0.18";
                    textBox3.Text = "60018";
                    break;
                default:
                    break;
            }
        }
    }
}
