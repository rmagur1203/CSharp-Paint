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
    public partial class Chat : Form
    {
        public static string room_ip;

        public Chat(string ip)
        {
            InitializeComponent();
            room_ip = ip;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*ListViewItem LVI = new ListViewItem();
            LVI.Text = nickname;
            LVI.SubItems.Add(MessageText.Text);
            listView1.Items.Add(LVI);*/
            string result = Login.HtmlFromUrl("http://" + room_ip + "/msg/" + Login.snickname + "/" + System.Net.WebUtility.UrlEncode(MessageText.Text));
            if (result != "true")
            {
                MessageBox.Show("메세지 전송 실패!\n사유 : " + result);
            }
            MessageText.Clear();
        }

        private void Chat_GotFocus(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void Chat_LostFocus(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            string s = Login.HtmlFromUrl("http://"+room_ip+ "/log");
            foreach (string search in s.Split(new[] { "\n" }, StringSplitOptions.None))
            {
                try
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = search.Split(new[] { "]: [" }, StringSplitOptions.None)[0];
                    lvi.SubItems.Add(search.Split(new[] { "]: [" }, StringSplitOptions.None)[1]);
                    listView1.Items.Add(lvi);
                }
                catch (Exception) { }
            }
            if (listView1.Items.Count > 0)
                listView1.Items[listView1.Items.Count - 1].EnsureVisible();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void MessageText_TextChanged(object sender, EventArgs e)
        {

        }

        private void MessageText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string result = Login.HtmlFromUrl("http://" + room_ip + "/msg/" + Login.snickname + "/" + System.Net.WebUtility.UrlEncode(MessageText.Text));
                if (result != "true")
                {
                    MessageBox.Show("메세지 전송 실패!\n사유 : " + result);
                }
                MessageText.Clear();
            }
        }

        private void Chat_Load(object sender, EventArgs e)
        {

        }
    }
}
