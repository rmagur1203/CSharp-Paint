using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharp_그림판
{
    public partial class Upload_Image : Form
    {
        private Panel panel1;

        public Upload_Image(Panel p)
        {
            InitializeComponent();
            panel1 = p;
        }

        private void Upload_Image_Load(object sender, EventArgs e)
        {
            Graphics g = panel1.CreateGraphics();
            var bmp = Form1.GraphicsToBitmap(g, Rectangle.Truncate(g.VisibleClipBounds));
            pictureBox1.Image = bmp;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox1.Text.Replace(" ","") == "")
            {
                MessageBox.Show("이름을 정해주세요.");
                return;
            }
            if (CheckingSpecialText(textBox1.Text))
            {
                MessageBox.Show("특수문자는 사용이 불가능합니다.");
                return;
            }
            if (Login.HtmlFromUrl("http://"+Form1.server_ip+"/image/is/"+textBox1.Text) == "true")
            {
                MessageBox.Show("이미 같은 이름으로 등록이 되있습니다.");
                return;
            }
            if (textBox1.Lines.Length > 1)
            {
                MessageBox.Show("이름은 한줄만 가능합니다.");
                return;
            }
            if (textBox1.Text.Length > 25)
            {
                MessageBox.Show("이름이 너무 깁니다.");
                return;
            }
            if (!CheckEnglishHangulNumber(textBox1.Text))
            {
                MessageBox.Show("한글과 영어, 숫자 만 사용해주세요.");
                return;
            }

            //MessageBox.Show(PostBase64(Convert.ToBase64String(ImageToByte(snapshot))).Result);
            //Clipboard.SetText(Convert.ToBase64String(Form1.ImageToByte(pictureBox1.Image)));
            Login.HtmlFromUrl("http://"+Form1.server_ip+"/msg/System/" + Login.snickname + " 님이 새 사진 [" + textBox1.Text + "] 를 올리셨습니다.");
            MessageBox.Show(Post("http://"+Form1.server_ip+"/image/up/"+textBox1.Text+"/"+Login.snickname, WebUtility.UrlEncode(Convert.ToBase64String(Form1.ImageToByte(pictureBox1.Image)))));
            this.Close();
        }

        public static string Post(string url, string text)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            var postData = "image=" + text;
            var data = Encoding.ASCII.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return responseString;
        }

        public bool CheckingSpecialText(string txt)
        {
            string str = @"[~!@\#$%^&*\()\=+|\\/:;?""<>']";
            System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(str);
            return rex.IsMatch(txt);
        }
        public bool isEnKoNum(string txt)
        {
            string str = @"[ㄱ-ㅎ가힣]";
            System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(str);
            return rex.IsMatch(txt);
        }
        public static bool CheckEnglishHangulNumber(string letter)
        {
            bool IsCheck = true;

            Regex engRegex = new Regex(@"[a-zA-Z]");
            Boolean ismatch = engRegex.IsMatch(letter);
            Regex korRegex = new Regex(@"[ㄱ-ㅎ가힣]");
            Boolean ismatchKor = korRegex.IsMatch(letter);
            Regex numRegex = new Regex(@"[0-9]");
            Boolean ismatchNum = numRegex.IsMatch(letter);
            Regex spaceRegex = new Regex(@" ");
            Boolean ismatchSpace = spaceRegex.IsMatch(letter);

            if (!ismatch && !ismatchNum && !ismatchKor && !ismatchSpace)
            {
                IsCheck = false;
            }

            return IsCheck;
        }
    }
}
