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
    public partial class signup : Form
    {
        public signup()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string s = Login.HtmlFromUrl("http://"+Form1.server_ip+"/signup/" + nickname.Text + "/" + id.Text + "/" + Login.Hash(password.Text));
            if (s == "true")
            {
                MessageBox.Show("회원가입 성공");
                this.Close();
            }
            else
            {
                MessageBox.Show("회원가입 실패\n사유 : "+s);
            }
        }
    }
}
