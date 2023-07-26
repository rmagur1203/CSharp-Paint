using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharp_그림판
{
    public partial class Login : Form
    {
        public static signup sign = new signup();
        public string nickname = "default";
        public static string snickname = "default";

        public Login()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string s = HtmlFromUrl("http://"+Form1.server_ip+"/login/" + id.Text+"/"+Hash(password.Text));
            if (s == "null")
                MessageBox.Show("로그인 실패");
            else
            {
                MessageBox.Show("로그인 성공");
                nickname = s;
                snickname = nickname;
                this.Hide();
                (new Form1(this)).Show();
            }
        }

        public static string Hash(string input)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        public static String HtmlFromUrl(string Url)
        {
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(Url);
                myRequest.Method = "GET";
                WebResponse myResponse = myRequest.GetResponse();
                StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
                string result = sr.ReadToEnd();
                sr.Close();
                myResponse.Close();

                return result;
            }
            catch (WebException ex)
            {
                return "null";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sign.ShowDialog();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            ContextMenuStrip cms = new ContextMenuStrip();
            cms.Items.Add("서버 주소 설정");
            cms.ItemClicked += new ToolStripItemClickedEventHandler(this.cms_ItemClicked);
            this.ContextMenuStrip = cms;
        }

        private void cms_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "서버 주소 설정")
            {
                (new Server_Select()).Show();
            }
        }
    }
}
