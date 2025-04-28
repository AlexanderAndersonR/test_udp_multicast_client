using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test_udp_multicast
{
    public partial class Form_terminal : Form
    {
        Form1 _form1 { get; set; }
        public Form_terminal(Form1 form1)
        {
            _form1 = form1;
            InitializeComponent();
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
        }
        public TextBox GetTextBox() { return textBox1; }

        private void Form_terminal_FormClosing(object sender, FormClosingEventArgs e)
        {
            _form1._form_Terminal = null;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 30000)
                textBox1.Clear();
        }
    }
}
