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
    public partial class RoomSelect : Form
    {
        public RoomSelect()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            (new Chat(Form1.server_ip + "/" + comboBox1.Text.Trim())).Show();
            Close();
        }

        private void RoomSelect_Load(object sender, EventArgs e)
        {
            foreach (var search in Login.HtmlFromUrl("http://"+Form1.server_ip+"/rooms").Split(new[] { "\n" }, StringSplitOptions.None))
            {
                if (search != "")
                    comboBox1.Items.Add(search);
            }
        }
    }
}
