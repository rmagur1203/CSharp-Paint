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
    public partial class Streaming_Select : Form
    {
        public Streaming_Select()
        {
            InitializeComponent();
        }

        private void Streaming_Select_Load(object sender, EventArgs e)
        {
            if (Login.HtmlFromUrl("http://" + Form1.server_ip + "/Streaming/list") == "null")
            {
                MessageBox.Show("스트리밍 서버에 접속할 수 없습니다.");
                return;
            }
            foreach (var search in Login.HtmlFromUrl("http://" + Form1.server_ip + "/Streaming/list").Split(new[] { "\n" }, StringSplitOptions.None))
            {
                if (search != "")
                    comboBox1.Items.Add(search);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            (new StreamView(comboBox1.Text)).Show();
        }
    }
}
