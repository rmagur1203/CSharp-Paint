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
    public partial class ImageViewer : Form
    {
        private Image image;

        public ImageViewer(Image img)
        {
            InitializeComponent();
            image = img;
        }

        private void ImageViewer_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = image;
            pictureBox1.Size = image.Size;
            Size = image.Size;
            MaximumSize = image.Size;
            MinimumSize = image.Size;
        }
    }
}
