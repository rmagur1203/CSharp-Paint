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
    public partial class RoomAdd : Form
    {
        public RoomAdd()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (var search in Login.HtmlFromUrl("http://" + Form1.server_ip + "/rooms").Split(new[] { "\n" }, StringSplitOptions.None))
            {
                if (search == textBox1.Text)
                {
                    MessageBox.Show("채팅서버 이름이 중복됬습니다.");
                    return;
                }
            }
            if (textBox1.Text == "서버이름 쓰는 칸" || textBox1.Text == "")
            {
                MessageBox.Show("서버이름을 써주세요.");
                return;
            }
            /*if (richTextBox1.Text == "설명 쓰는칸" || richTextBox1.Text == "")
            {
                MessageBox.Show("설명을 써주세요.");
                return;
            }*/

            if (Login.HtmlFromUrl("http://"+Form1.server_ip+"/room/add/"+textBox1.Text) == "true")
            {
                MessageBox.Show("방이 생성되었습니다.");
                Close();
            }
            else
            {
                MessageBox.Show("방 생성에 실패하였습니다.");
            }
        }
    }
}
