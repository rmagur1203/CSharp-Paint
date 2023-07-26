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
    public partial class StreamView : Form
    {
        private static string Streamer;

        public StreamView(string name)
        {
            InitializeComponent();
            Streamer = name;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string result = Login.HtmlFromUrl("http://" + Form1.server_ip + "/Streaming/isstart/" + Streamer);
            if (result == "true")
                pictureBox1.Image = Form1.byteArrayToImage(Convert.FromBase64String((Login.HtmlFromUrl("http://" + Form1.server_ip + "/Streaming/down/"+Streamer))));
            else if (result == "false")
            {
                Close();
                MessageBox.Show(Streamer + " 님의 스트리밍이 종료되었습니다.");
            }
            else
            {
                Close();
                MessageBox.Show("서버에서 오류가 생겼습니다.");
            }
        }

        private void StreamView_Load(object sender, EventArgs e)
        {

        }
    }
}
