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
    public partial class ImageViewList : Form
    {
        private List<Image> images;
        private List<string> texts;
        private Panel panel1;
        private Form1 form1;

        private PictureBox selectpic = null;

        public ImageViewList(List<Image> imgs, List<string> vs, Panel p, Form1 frm1)
        {
            InitializeComponent();
            images = imgs;
            texts = vs;
            panel1 = p;
            form1 = frm1;
        }

        private void ImageViewList_Load(object sender, EventArgs e)
        {
            for (int i = 0; i<images.Count; i++)
            {
                Panel p = new Panel();
                PictureBox pic = new PictureBox();
                Label label = new Label();
                label.AutoSize = false;
                label.Dock = DockStyle.Bottom;
                label.TextAlign = ContentAlignment.MiddleCenter;
                label.Text = texts[i];
                p.Controls.Add(label);
                pic.Dock = DockStyle.Fill;
                pic.Image = images[i];
                pic.SizeMode = PictureBoxSizeMode.Zoom;
                pic.Name = texts[i];
                pic.MouseClick += new MouseEventHandler(this.Image_MouseClick);
                p.Controls.Add(pic);
                p.BackColor = Color.White;
                p.Size = new Size(250, 250);
                flowLayoutPanel1.Controls.Add(p);
                string result = Login.HtmlFromUrl("http://" + Form1.server_ip + "/creater/" + pic.Name);
                if (result == Login.snickname || Login.snickname == Form1.Root)
                {
                    ContextMenuStrip picmenuStrip = new ContextMenuStrip();
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem();
                        item.Text = "삭제";
                        item.Name = pic.Name;
                        item.Click += new EventHandler(this.item_Click);
                        picmenuStrip.Items.Add(item);
                    }
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem();
                        item.Text = "미리보기";
                        item.Name = pic.Name;
                        item.Click += new EventHandler(this.item_Click);
                        picmenuStrip.Items.Add(item);
                    }
                    pic.ContextMenuStrip = picmenuStrip;
                }
                else
                {
                    ContextMenuStrip picmenuStrip = new ContextMenuStrip();
                    ToolStripMenuItem item = new ToolStripMenuItem();
                    item.Text = "미리보기";
                    item.Name = pic.Name;
                    item.Click += new EventHandler(this.item_Click);
                    picmenuStrip.Items.Add(item);
                    picmenuStrip.Text = "picmenuStrip1";
                    pic.ContextMenuStrip = picmenuStrip;
                    pic.ContextMenuStrip.Show(Cursor.Position.X, Cursor.Position.Y);
                }
            }
        }

        private void Image_MouseClick(object sender, MouseEventArgs e)
        {
            /*if (e.Button == MouseButtons.Right)
            {
                PictureBox picture = (PictureBox)sender;
                string result = Login.HtmlFromUrl("http://"+Form1.server_ip+"/creater/" + picture.Name);
                selectpic = picture;
                if (result == Login.snickname || Login.snickname == Form1.Root)
                {
                    ContextMenuStrip picmenuStrip = new ContextMenuStrip();
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem();
                        item.Text = "삭제";
                        item.Name = picture.Name;
                        item.Click += new EventHandler(this.item_Click);
                        picmenuStrip.Items.Add(item);
                    }
                    {
                        ToolStripMenuItem item = new ToolStripMenuItem();
                        item.Text = "미리보기";
                        item.Name = picture.Name;
                        item.Click += new EventHandler(this.item_Click);
                        picmenuStrip.Items.Add(item);
                    }
                    picture.ContextMenuStrip = picmenuStrip;
                    picture.ContextMenuStrip.Show(Cursor.Position.X, Cursor.Position.Y);
                }
                else
                {
                    ContextMenuStrip picmenuStrip = new ContextMenuStrip();
                    ToolStripMenuItem item = new ToolStripMenuItem();
                    item.Text = "미리보기";
                    item.Name = picture.Name;
                    item.Click += new EventHandler(this.item_Click);
                    picmenuStrip.Items.Add(item);
                    picmenuStrip.Text = "picmenuStrip1";
                    picture.ContextMenuStrip = picmenuStrip;
                    picture.ContextMenuStrip.Show(Cursor.Position.X, Cursor.Position.Y);
                } 
            }else */
            if (e.Button == MouseButtons.Left)
            {
                Image img = ((PictureBox)sender).Image;
                Graphics g = panel1.CreateGraphics();
                g.DrawImage(img, new Point(0, 0));
                form1.Undo_Save();
                this.Close();
            }
        }

        private void item_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            if (item.Text == "삭제")
            {
                MessageBox.Show(Login.HtmlFromUrl("http://"+Form1.server_ip+"/del/pic/" + item.Name));
                Login.HtmlFromUrl("http://"+Form1.server_ip+"/msg/System/" + Login.snickname + " 님이 사진 [" + item.Name + "] 를 삭제 하셨습니다.");
                this.Close();
            }
            else if(item.Text == "미리보기")
            {
                ContextMenuStrip menuStrip = item.Owner as ContextMenuStrip;
                if (selectpic != null)
                {
                    (new ImageViewer(selectpic.Image)).ShowDialog();
                }
            }
        }
    }
}
