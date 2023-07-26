using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharp_그림판
{
    public partial class Server_Select : Form
    {
        public Server_Select()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                if (comboBox1.SelectedIndex == 0)
                {
                    Form1.server_ip = "218.147.244.114:3000";
                    Form1.only_server_ip = "218.147.244.114";
                }
                if (comboBox1.SelectedIndex == 1)
                {
                    Form1.server_ip = "main1.run.goorm.io";
                    Form1.only_server_ip = "main1.run.goorm.io";
                }
            }
            else
            {
                comboBox1.Text = comboBox1.Text.Replace("http://", "").Replace("https://", "");
                if (comboBox1.Text.Last() == '/')
                    comboBox1.Text = comboBox1.Text.Remove(comboBox1.Text.Length - 1);
                MessageBox.Show(comboBox1.Text);
                if (numericUpDown1.Value == 0)
                {
                    Form1.server_ip = comboBox1.Text;
                    Form1.only_server_ip = comboBox1.Text;
                }
                else
                {
                    Form1.server_ip = comboBox1.Text + ":" + numericUpDown1.Value.ToString();
                    Form1.only_server_ip = comboBox1.Text;
                }
            }
            Close();
        }
    }
}
