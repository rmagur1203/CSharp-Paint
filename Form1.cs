using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace CSharp_그림판
{
    public partial class Form1 : Form
    {
        #region "설정"
        private Pen pen;
        private Pen wpen;
        private Brush wBrush, bBrush;
        private Color customcolor = Color.Red;
        private string brushty = "B";
        private Boolean TXTSE = false;
        private string TXT;
        private Font txtfont;
        private MouseEventArgs Starte;
        private MouseEventArgs Ende;
        private string imagelink;
        private PointF savepoint;
        private Boolean nbbool;
        private Boolean ncbool;
        private int nbw = 5;
        private int ncw = 5;
        private const Int32 WM_PAINT = 15;
        private Point DownP;
        private Keys key;
        private bool ctrldown = false;
        private bool zdown = false;
        private bool ydown = false;
        private bool otherdown = false;
        private bool undod = false;
        private int undoint = 0;
        private string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private int redomax = 0;
        private bool show_line = false;
        private string nickname = "default";
        private Login lg;
        private bool stardown = false;
        private bool Stream = false;
        private bool down = false;
        private int bsize = 5;//브러쉬 사이즈
        private int transparency = 255;
        #endregion

        private static readonly HttpClient client = new HttpClient();
        public static readonly String Root = "System";

        public static string server_ip = "218.147.244.114:3000";
        public static string only_server_ip = server_ip.Split(':')[0];

        enum TernaryRasterOperations : uint
        {
            /// <summary>dest = source</summary>
            SRCCOPY = 0x00CC0020,
            /// <summary>dest = source OR dest</summary>
            SRCPAINT = 0x00EE0086,
            /// <summary>dest = source AND dest</summary>
            SRCAND = 0x008800C6,
            /// <summary>dest = source XOR dest</summary>
            SRCINVERT = 0x00660046,
            /// <summary>dest = source AND (NOT dest)</summary>
            SRCERASE = 0x00440328,
            /// <summary>dest = (NOT source)</summary>
            NOTSRCCOPY = 0x00330008,
            /// <summary>dest = (NOT src) AND (NOT dest)</summary>
            NOTSRCERASE = 0x001100A6,
            /// <summary>dest = (source AND pattern)</summary>
            MERGECOPY = 0x00C000CA,
            /// <summary>dest = (NOT source) OR dest</summary>
            MERGEPAINT = 0x00BB0226,
            /// <summary>dest = pattern</summary>
            PATCOPY = 0x00F00021,
            /// <summary>dest = DPSnoo</summary>
            PATPAINT = 0x00FB0A09,
            /// <summary>dest = pattern XOR dest</summary>
            PATINVERT = 0x005A0049,
            /// <summary>dest = (NOT dest)</summary>
            DSTINVERT = 0x00550009,
            /// <summary>dest = BLACK</summary>
            BLACKNESS = 0x00000042,
            /// <summary>dest = WHITE</summary>
            WHITENESS = 0x00FF0062,
            /// <summary>
            /// Capture window as seen on screen.  This includes layered windows 
            /// such as WPF windows with AllowsTransparency="true"
            /// </summary>
            CAPTUREBLT = 0x40000000
        }

        public Form1(Login login)
        {
            InitializeComponent();
            pen = new Pen(Color.Black);
            wpen = new Pen(Color.White);
            wBrush = new SolidBrush(Color.White);
            bBrush = new SolidBrush(Color.Black);
            nickname = login.nickname;
            lg = login;
        }

        private class User32
        {

            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {

                public int left;

                public int top;

                public int right;

                public int bottom;
            }

            [DllImport("user32.dll", EntryPoint = "SendMessageA")]
            public static extern System.UInt32 SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
        }

        #region ClassFunction
        public static class GIF 
        {
            public static int Gif_Frame_Delay = 500;
            public static int Gif_Repeat = 0;

            public static void Save(List<Image> images, string path)
            {
                AnimatedGif.AnimatedGifCreator e = new AnimatedGif.AnimatedGifCreator(path, Gif_Frame_Delay, Gif_Repeat);
                foreach (Image search in images)
                {
                    e.AddFrame(search);
                }
                e.Dispose();
            }

            public static void Save1(List<Image> images, string path)
            {
                GifBitmapEncoder gEnc = new GifBitmapEncoder();

                foreach (Bitmap bmpImage in images)
                {
                    var bmp = bmpImage.GetHbitmap();
                    var src = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        bmp,
                        IntPtr.Zero,
                        System.Windows.Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                    gEnc.Frames.Add(BitmapFrame.Create(src));
                }
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    gEnc.Save(fs);
                }
            }
        }
        #endregion

        #region Function
        private Rectangle 사각형(Point Down, Point Up)
        {
            Rectangle rect;
            if (Down.X > Up.X)
            {
                rect = new Rectangle(Up.X, Up.Y, Down.X - Up.X, Down.Y - Up.Y);
            }
            else if (Down.X < Up.X)
            {
                rect = new Rectangle(Down.X, Down.Y, Up.X - Down.X, Up.Y - Down.Y);
            }
            else
            {
                rect = new Rectangle(Down.X, Down.Y, 1, 1);
            }
            return rect;
        }
        private void draw바깥라인()
        {
            if (!show_line)
                return;
            Graphics g = panel1.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(10, 10, panel1.Width - 20/*(10+18*20 - 10) + 100*/, panel1.Height - 20/*(10 + 18 * 20 - 10) + 100*/);
            g.DrawRectangle(pen, rect);
            rect = new Rectangle(0, 0, panel1.Width - 1/*(10 + 18 * 20 + 10) + 100*/, panel1.Height - 1/*(10 + 18 * 20 + 10) + 100*/);
            g.DrawRectangle(pen, rect);
        }
        private void Spray1(Graphics g, int ex, int ey, Size size, Pen p, int radius = 15)
        {
            for (int i = 0; i < 100; ++i)
            {
                Random _rnd = new Random();
                // Select random Polar coordinate
                // where theta is a random angle between 0..2*PI
                // and r is a random value between 0..radius
                double theta = _rnd.NextDouble() * (Math.PI * 2);
                double r = _rnd.NextDouble() * radius;

                // Transform the polar coordinate to cartesian (x,y)
                // and translate the center to the current mouse position
                double x = ex + Math.Cos(theta) * r;
                double y = ey + Math.Sin(theta) * r;

                g.DrawEllipse(p, new Rectangle((int)x - 1, (int)y - 1, 1, 1));
            }
        }
        private void Spray2(Graphics g, int ex, int ey, int radius = 15)
        {
            int radius2 = radius * 2;

            double x;
            double y;

            for (int i = 0; i < 100; ++i)
            {
                do
                {
                    Random _rnd = new Random();
                    // Randomy select x,y so that 
                    // x falls between -radius..radius
                    // y falls between -radius..radius
                    x = (_rnd.NextDouble() * radius2) - radius;
                    y = (_rnd.NextDouble() * radius2) - radius;

                    // If x^2 + y^2 > r2 the point is outside the circle
                    // and a new point needs to be selected
                } while ((x * x + y * y) > (radius * radius));

                // Translate the point so that the center is at the mouse
                // position
                x += ex;
                y += ey;

                g.DrawEllipse(Pens.Black, new Rectangle((int)x - 1, (int)y - 1, 1, 1));
            }
        }
        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
        private void panel1_undo()
        {
            if (!can_undo())
                return;
            undoint -= 1;
            Graphics g = panel1.CreateGraphics();
            string FN = appdata + "/CSpaint/" + undoint + ".png";
            g.DrawImage(ImageStream(FN), new PointF(0, 0));

            if (Stream)
                Post("http://" + server_ip + "/Streaming/up/", WebUtility.UrlEncode(Convert.ToBase64String(ImageToByte(ImageStream(FN)))));
        }
        private void panel1_redo()
        {
            if (!can_redo())
                return;
            undoint += 1;
            Graphics g = panel1.CreateGraphics();
            string FN = appdata + "/CSpaint/" + undoint + ".png";
            g.DrawImage(ImageStream(FN), new PointF(0, 0));

            if (Stream)
                Post("http://" + server_ip + "/Streaming/up/", WebUtility.UrlEncode(Convert.ToBase64String(ImageToByte(ImageStream(FN)))));
        }
        private Image ImageStream(string path)
        {
            Stream stream = new FileStream(path, FileMode.Open);
            Image image = Image.FromStream(stream);
            stream.Close();
            return image;
        }
        private bool can_undo()
        {
            if (undoint <= 0)
                return false;
            return true;
        }
        private bool can_redo()
        {
            if (redomax < undoint + 1)
                return false;
            try
            {
                File.ReadAllBytes(appdata + "/CSpaint/" + (undoint + 1) + ".png");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private string Post(string url, string text)
        {
            #region"주석"
            /*string data = "{ \"image\": "+text+" }";
            MessageBox.Show(data);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Timeout = 30 * 1000;
            //request.Headers.Add("Authorization", "BASIC SGVsbG8=");

            // POST할 데이타를 Request Stream에 쓴다
            byte[] bytes = Encoding.ASCII.GetBytes(data);
            request.ContentLength = bytes.Length; // 바이트수 지정

            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(bytes, 0, bytes.Length);
            }

            // Response 처리
            string responseText = string.Empty;
            using (WebResponse resp = request.GetResponse())
            {
                Stream respStream = resp.GetResponseStream();
                using (StreamReader sr = new StreamReader(respStream))
                {
                    responseText = sr.ReadToEnd();
                }
            }

           return responseText;*/
            #endregion
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
        public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }
        private PointF[] Calculate5StarPoints(PointF Orig, float outerradius, float innerradius)
        {
            // Define some variables to avoid as much calculations as possible
            // conversions to radians
            double Ang36 = Math.PI / 5.0;   // 36Â° x PI/180
            double Ang72 = 2.0 * Ang36;     // 72Â° x PI/180
            // some sine and cosine values we need
            float Sin36 = (float)Math.Sin(Ang36);
            float Sin72 = (float)Math.Sin(Ang72);
            float Cos36 = (float)Math.Cos(Ang36);
            float Cos72 = (float)Math.Cos(Ang72);
            // Fill array with 10 origin points
            PointF[] pnts = { Orig, Orig, Orig, Orig, Orig, Orig, Orig, Orig, Orig, Orig };
            pnts[0].Y -= outerradius;  // top off the star, or on a clock this is 12:00 or 0:00 hours
            pnts[1].X += innerradius * Sin36; pnts[1].Y -= innerradius * Cos36; // 0:06 hours
            pnts[2].X += outerradius * Sin72; pnts[2].Y -= outerradius * Cos72; // 0:12 hours
            pnts[3].X += innerradius * Sin72; pnts[3].Y += innerradius * Cos72; // 0:18
            pnts[4].X += outerradius * Sin36; pnts[4].Y += outerradius * Cos36; // 0:24 
            // Phew! Glad I got that trig working.
            pnts[5].Y += innerradius;
            // I use the symmetry of the star figure here
            pnts[6].X += pnts[6].X - pnts[4].X; pnts[6].Y = pnts[4].Y;  // mirror point
            pnts[7].X += pnts[7].X - pnts[3].X; pnts[7].Y = pnts[3].Y;  // mirror point
            pnts[8].X += pnts[8].X - pnts[2].X; pnts[8].Y = pnts[2].Y;  // mirror point
            pnts[9].X += pnts[9].X - pnts[1].X; pnts[9].Y = pnts[1].Y;  // mirror point
            return pnts;
        }
        public static Bitmap RotateImage(Bitmap b, float angle)
        {
            //create a new empty bitmap to hold rotated image
            Bitmap returnBitmap = new Bitmap(b.Width, b.Height);
            //make a graphics object from the empty bitmap
            using (Graphics g = Graphics.FromImage(returnBitmap))
            {
                //move rotation point to center of image
                g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);
                //rotate
                g.RotateTransform(angle);
                //move image back
                g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);
                //draw passed in image onto graphics object
                g.DrawImage(b, new Point(0, 0));
            }
            return returnBitmap;
        }
        public void Undo_Save()
        {
            Graphics g = panel1.CreateGraphics();
            undoint += 1;
            redomax = undoint;
            string FN = appdata + "/CSpaint/" + undoint + ".png";
            var bmp = GraphicsToBitmap(g, Rectangle.Truncate(g.VisibleClipBounds));
            Bitmap snapshot = bmp;
            //snapshot.Save(FN);
            File.WriteAllBytes(FN, ImageToByte(snapshot));
        }
        public static Image byteArrayToImage(byte[] bytesArr)
        {
            try
            {
                MemoryStream memstr = new MemoryStream(bytesArr);
                Image img = Image.FromStream(memstr);
                return img;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private PointF[] StarPoints(int num_points, Rectangle bounds)
        {
            // Make room for the points.
            PointF[] pts = new PointF[num_points];

            double rx = bounds.Width / 2;
            double ry = bounds.Height / 2;
            double cx = bounds.X + rx;
            double cy = bounds.Y + ry;

            // Start at the top.
            double theta = -Math.PI / 2;
            double dtheta = 4 * Math.PI / num_points;
            for (int i = 0; i < num_points; i++)
            {
                pts[i] = new PointF(
                    (float)(cx + rx * Math.Cos(theta)),
                    (float)(cy + ry * Math.Sin(theta)));
                theta += dtheta;
            }

            return pts;
        }
        public static Bitmap GraphicsToBitmap(Graphics g, Rectangle bounds)
        {
            Bitmap bmp = new Bitmap(bounds.Width, bounds.Height);

            using (Graphics bmpGrf = Graphics.FromImage(bmp))
            {
                IntPtr hdc1 = g.GetHdc();
                IntPtr hdc2 = bmpGrf.GetHdc();

                BitBlt(hdc2, 0, 0, bmp.Width, bmp.Height, hdc1, 0, 0, TernaryRasterOperations.SRCCOPY);

                g.ReleaseHdc(hdc1);
                bmpGrf.ReleaseHdc(hdc2);
            }

            return bmp;
        }
        private void Draw_Star1(Point p, Size size)
        {
            Graphics g = panel1.CreateGraphics();
            g.SmoothingMode = SmoothingMode.HighQuality;
            PointF[] Star1 = StarPoints(5, new Rectangle(p, size));
            g.DrawPolygon(new Pen(customcolor), Star1);
        }
        private void Fill_Star1(Point p, Size size)
        {
            Graphics g = panel1.CreateGraphics();
            g.SmoothingMode = SmoothingMode.HighQuality;
            PointF[] Star1 = StarPoints(5, new Rectangle(p, size));
            g.FillPolygon(new SolidBrush(customcolor), Star1);
        }
        private void Fill_Star2(Point p, Size size)
        {
            Graphics g = panel1.CreateGraphics();
            g.SmoothingMode = SmoothingMode.HighQuality;
            PointF[] Star1 = StarPoints(5, new Rectangle(p, size));
            g.FillPolygon(new SolidBrush(customcolor), Star1);
        }
        private void Test_Star()
        {
            Graphics g = panel1.CreateGraphics();
            g.SmoothingMode = SmoothingMode.HighQuality;

            PointF[] Star1 = Calculate5StarPoints(new PointF(100f, 100f), 50f, 20f);
            SolidBrush FillBrush = new SolidBrush(Color.Pink);
            g.FillPolygon(FillBrush, Star1);
            g.DrawPolygon(new Pen(Color.Purple, 5), Star1);

            PointF[] Star3 = Calculate5StarPoints(new PointF(350f, 300f), 200f, 100f);
            LinearGradientBrush lin = new LinearGradientBrush(new Point(350, 100), new Point(350, 500),
            Color.Salmon, Color.Cyan);
            g.FillPolygon(lin, Star3);
        }
        public static bool FtpDirectoryExists(string directoryPath, string ftpUser, string ftpPassword)
        {
            bool IsExists = true;
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(directoryPath);
                request.Credentials = new NetworkCredential(ftpUser, ftpPassword);
                request.Method = WebRequestMethods.Ftp.PrintWorkingDirectory;

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                IsExists = false;
            }
            return IsExists;
        }
        public static List<string> ListFiles(string directoryPath, string ftpUser, string ftpPassword)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(directoryPath);
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                request.Credentials = new NetworkCredential(ftpUser, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string names = reader.ReadToEnd();

                reader.Close();
                response.Close();

                return names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Dialog
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }
        /*public static Decimal NumberBox(string title, string promptText, int value = 100, int min = 0, int max = 100)
        {
            Form form = new Form();
            Label label = new Label();
            NumericUpDown numeric = new NumericUpDown();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            numeric.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            numeric.Anchor = numeric.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, numeric, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            return numeric.Value;
        }*/
        /*public static Decimal NumberBox(string title, string promptText, ref DialogResult result, int min = 0, int max = 100)
        {
            Form form = new Form();
            Label label = new Label();
            NumericUpDown numeric = new NumericUpDown();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            numeric.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            numeric.Anchor = numeric.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, numeric, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            result = form.ShowDialog();
            return numeric.Value;
        }*/
        public static DialogResult DialogNumberBox(string title, string promptText, ref int value, int min = 0, int max = 100)
        {
            Form form = new Form();
            Label label = new Label();
            NumericUpDown numeric = new NumericUpDown();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            numeric.Value = value;

            numeric.Minimum = min;
            numeric.Maximum = max;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            numeric.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            numeric.Anchor = numeric.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, numeric, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = (int)numeric.Value;
            return dialogResult;
        }

        /*public static string RoomSelectDialog(string[] Rooms, ref DialogResult result)
        {
            Form form = new Form();
            RichTextBox richTextBox1 = new RichTextBox();
            ComboBox comboBox1 = new ComboBox();
            Button buttonSelect = new Button();

            foreach (string search in Rooms)
                comboBox1.Items.Add(search);

            buttonSelect.Text = "선택";
            buttonSelect.DialogResult = DialogResult.OK;

            richTextBox1.SetBounds(12, 41, 387, 409);
            comboBox1.SetBounds(12, 12, 306, 23);
            buttonSelect.SetBounds(324, 12, 75, 23);

            comboBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            buttonSelect.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            form.Size = new Size(429, 509);
            form.Controls.AddRange(new Control[] { richTextBox1, comboBox1, buttonSelect });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonSelect;

            result = form.ShowDialog();
            return comboBox1.Text;
        }*/
        /*public static string RoomSelectDialog(string[] Rooms)
        {
            Form form = new Form();
            RichTextBox richTextBox1 = new RichTextBox();
            ComboBox comboBox1 = new ComboBox();
            Button buttonSelect = new Button();

            foreach (string search in Rooms)
                comboBox1.Items.Add(search);

            buttonSelect.Text = "선택";
            buttonSelect.DialogResult = DialogResult.OK;

            richTextBox1.SetBounds(12, 41, 387, 409);
            comboBox1.SetBounds(12, 12, 306, 23);
            buttonSelect.SetBounds(324, 12, 75, 23);

            comboBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            buttonSelect.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            form.Size = new Size(429, 509);
            form.Controls.AddRange(new Control[] { richTextBox1, comboBox1, buttonSelect });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonSelect;

            form.ShowDialog();
            return comboBox1.Text;
        }*/
        /*public static DialogResult RoomSelectDialog2(string[] Rooms, ref string selected)
        {
            Form form = new Form();
            RichTextBox richTextBox1 = new RichTextBox();
            ComboBox comboBox1 = new ComboBox();
            Button buttonSelect = new Button();

            foreach (string search in Rooms)
                comboBox1.Items.Add(search);

            buttonSelect.Text = "선택";
            buttonSelect.DialogResult = DialogResult.OK;

            richTextBox1.SetBounds(12, 41, 387, 409);
            comboBox1.SetBounds(12, 12, 306, 23);
            buttonSelect.SetBounds(324, 12, 75, 23);

            comboBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            buttonSelect.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            form.Size = new Size(429, 509);
            form.Controls.AddRange(new Control[] { richTextBox1, comboBox1, buttonSelect });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonSelect;

            selected = comboBox1.Text;
            return form.ShowDialog();
        }*/
        class InputBoxClass
        {
            public InputBoxClass()
            {
            }

            public static string InputBox(string title, string promptText)
            {
                Form form = new Form();
                Label label = new Label();
                TextBox textBox = new TextBox();
                Button buttonOk = new Button();
                Button buttonCancel = new Button();

                form.Text = title;
                label.Text = promptText;

                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.SetBounds(9, 20, 372, 13);
                textBox.SetBounds(12, 36, 372, 20);
                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);

                label.AutoSize = true;
                textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
                buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                form.ClientSize = new Size(396, 107);
                form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
                form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;

                form.ShowDialog();
                return textBox.Text;
            }

            public static DialogResult ShowDialog(string title, string promptText, ref string value)
            {
                Form form = new Form();
                Label label = new Label();
                TextBox textBox = new TextBox();
                Button buttonOk = new Button();
                Button buttonCancel = new Button();

                form.Text = title;
                label.Text = promptText;
                textBox.Text = value;

                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.SetBounds(9, 20, 372, 13);
                textBox.SetBounds(12, 36, 372, 20);
                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);

                label.AutoSize = true;
                textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
                buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                form.ClientSize = new Size(396, 107);
                form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
                form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                DialogResult dialogResult = form.ShowDialog();
                value = textBox.Text;
                return dialogResult;
            }
        }
        class NumberBox
        {
            public DialogResult dialogResult;
            public string title;
            public string promptText;
            public int value;

            public NumberBox()
            {
            }

            public int ShowDialog()
            {
                Form form = new Form();
                Label label = new Label();
                NumericUpDown numeric = new NumericUpDown();
                Button buttonOk = new Button();
                Button buttonCancel = new Button();

                form.Text = title;
                label.Text = promptText;
                numeric.Value = value;

                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.SetBounds(9, 20, 372, 13);
                numeric.SetBounds(12, 36, 372, 20);
                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);

                label.AutoSize = true;
                numeric.Anchor = numeric.Anchor | AnchorStyles.Right;
                buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                form.ClientSize = new Size(396, 107);
                form.Controls.AddRange(new Control[] { label, numeric, buttonOk, buttonCancel });
                form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                dialogResult = form.ShowDialog();
                return (int)numeric.Value;
            }
        }
        class NumberBoxDialog
        {
            public int value;
            public string title;
            public string promptText;
            public int min = 0;
            public int max = 100;

            public NumberBoxDialog()
            {
            }

            public DialogResult DialogNumberBox()
            {
                Form form = new Form();
                Label label = new Label();
                NumericUpDown numeric = new NumericUpDown();
                Button buttonOk = new Button();
                Button buttonCancel = new Button();

                form.Text = title;
                label.Text = promptText;
                numeric.Value = value;

                numeric.Minimum = min;
                numeric.Maximum = max;

                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.SetBounds(9, 20, 372, 13);
                numeric.SetBounds(12, 36, 372, 20);
                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);

                label.AutoSize = true;
                numeric.Anchor = numeric.Anchor | AnchorStyles.Right;
                buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                form.ClientSize = new Size(396, 107);
                form.Controls.AddRange(new Control[] { label, numeric, buttonOk, buttonCancel });
                form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                DialogResult dialogResult = form.ShowDialog();
                value = (int)numeric.Value;
                return dialogResult;
            }
        }
        class NumberBoxDialog2
        {
            public NumberBoxDialog2()
            {

            }

            public DialogResult Show(int value, int min = 0, int max = 100, string title = "", string promptText = "")
            {
                Form form = new Form();
                Label label = new Label();
                NumericUpDown numeric = new NumericUpDown();
                Button buttonOk = new Button();
                Button buttonCancel = new Button();

                form.Text = title;
                label.Text = promptText;
                numeric.Value = value;

                numeric.Minimum = min;
                numeric.Maximum = max;

                buttonOk.Text = "OK";
                buttonCancel.Text = "Cancel";
                buttonOk.DialogResult = DialogResult.OK;
                buttonCancel.DialogResult = DialogResult.Cancel;

                label.SetBounds(9, 20, 372, 13);
                numeric.SetBounds(12, 36, 372, 20);
                buttonOk.SetBounds(228, 72, 75, 23);
                buttonCancel.SetBounds(309, 72, 75, 23);

                label.AutoSize = true;
                numeric.Anchor = numeric.Anchor | AnchorStyles.Right;
                buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                form.ClientSize = new Size(396, 107);
                form.Controls.AddRange(new Control[] { label, numeric, buttonOk, buttonCancel });
                form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonOk;
                form.CancelButton = buttonCancel;

                DialogResult dialogResult = form.ShowDialog();
                value = (int)numeric.Value;
                return dialogResult;
            }
        }
        class RoomSelectDialog
        {
            public string selected;
            public string[] Rooms;

            public RoomSelectDialog()
            {
            }

            public DialogResult ShowDialog()
            {
                Form form = new Form();
                RichTextBox richTextBox1 = new RichTextBox();
                ComboBox comboBox1 = new ComboBox();
                Button buttonSelect = new Button();

                foreach (string search in Rooms)
                    comboBox1.Items.Add(search);

                buttonSelect.Text = "선택";
                buttonSelect.DialogResult = DialogResult.OK;
                richTextBox1.Enabled = false;

                richTextBox1.SetBounds(12, 41, 387, 409);
                comboBox1.SetBounds(12, 12, 306, 23);
                buttonSelect.SetBounds(324, 12, 75, 23);

                comboBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                buttonSelect.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left;

                form.Size = new Size(429, 509);
                form.Controls.AddRange(new Control[] { richTextBox1, comboBox1, buttonSelect });
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.AcceptButton = buttonSelect;

                selected = comboBox1.Text;
                return form.ShowDialog();
            }
        }
        #endregion



        public Bitmap getSnapShot(System.Windows.Forms.Control control)
        {
            try
            {
                Image image = new Bitmap(control.Width, control.Height);
                //어느 컨트롤로 부터 그래픽 객체를 얻습니다.
                Graphics g = Graphics.FromImage(image);
                IntPtr hDCT = g.GetHdc();
                User32.SendMessage(control.Handle, WM_PAINT, hDCT, IntPtr.Zero);

                g.ReleaseHdc(hDCT);
                g.Dispose();

                return new Bitmap(image);
            }
            catch (Exception)
            {
                return null;
            }
        }
        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);



        #region Form1
        private void Form1_Load(object sender, EventArgs e)
        {
            Directory.CreateDirectory(appdata + "/CSpaint");
            string filepath = appdata + "/CSpaint";
            DirectoryInfo d = new DirectoryInfo(filepath);
            foreach (var file in d.GetFiles("*.png"))
            {
                File.Delete(file.FullName);
            }
            Login.HtmlFromUrl("http://"+server_ip+"/msg/System/"+nickname+" 님이 접속하셨습니다.");
            string result = Login.HtmlFromUrl("http://" + Form1.server_ip + "/Streaming/isstart/" + Login.snickname);
            if (result == "true")
            {
                Stream = true;
            }
            if (Login.snickname == Root)
            {
                관리ToolStripMenuItem.Enabled = true;
                관리ToolStripMenuItem.Visible = true;
            }
        }
        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            lg.Close();
        }
        private void Form1_Closed(object sender, FormClosedEventArgs e)
        {
            Login.HtmlFromUrl("http://"+server_ip+"/msg/System/" + nickname + " 님이 나가셨습니다.");
            Login.HtmlFromUrl("http://" + Form1.server_ip + "/Streaming/end/" + Login.snickname);
        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            CheckBox cb = new CheckBox();
            cb.Text = "선 표시";
            cb.CheckedChanged += new EventHandler(표시선_CheckedChanged);
            cb.AutoSize = true;
            cb.Checked = show_line;
            cb.BackColor = Color.Transparent;
            ToolStripControlHost host = new ToolStripControlHost(cb);
            toolStripDropDownButton1.DropDownItems.Insert(0, host);

            Graphics g = panel1.CreateGraphics();
            g.Clear(Color.White);
            draw바깥라인();
            Directory.CreateDirectory(appdata + "/CSpaint");
            string FN = appdata + "/CSpaint/0.png";
            var bmp = GraphicsToBitmap(g, Rectangle.Truncate(g.VisibleClipBounds));
            Bitmap snapshot = bmp;
            snapshot.Save(FN);
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            /*if (e.KeyCode == Keys.ControlKey)
            {
                ctrldown = true;
            }
            else if (e.KeyCode == Keys.Z)
            {
                zdown = true;
            }
            else if (e.KeyCode == Keys.Y)
            {
                ydown = true;
            }
            else
            {
                otherdown = true;
            }*/
            if (e.KeyCode == Keys.Z/* && ctrldown*/)
            {
                panel1_undo();
            }
            else if (e.KeyCode == Keys.Y /*&& ctrldown*/)
            {
                panel1_redo();
            }
            else if (e.KeyCode == Keys.ControlKey)
            {
                ctrldown = true;
            }
            if (e.KeyCode == Keys.C)
            {
                Graphics g = panel1.CreateGraphics();
                var bmp = GraphicsToBitmap(g, Rectangle.Truncate(g.VisibleClipBounds));
                Bitmap snapshot = bmp;
                Clipboard.SetImage(bmp);
            }
        }
        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            /*if (ctrldown && zdown && (!undod) && !otherdown)
            {
                undod = true;
                panel1_undo();
            }
            else if (ctrldown && ydown && (!undod) && !otherdown)
            {
                undod = true;
                panel1_redo();
            }*/
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                ctrldown = false;
                undod = false;
            }
            else if (e.KeyCode == Keys.Z)
            {
                zdown = false;
                undod = false;
            }
            else if (e.KeyCode == Keys.Y)
            {
                ydown = false;
                undod = false;
            }
            else
            {
                otherdown = false;
            }
        }
        #endregion

        #region panel1
        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            down = true;
            int x, y;
            x = e.X / 20;
            y = e.Y / 20;
            DownP = new Point(e.X, e.Y);
            Rectangle rect = new Rectangle(x, y, 5, 5)
            {
                Location = e.Location
            };
            //if (x < 0 || x >= 24 || y < 0 || y >= 24)
                //return;
            Graphics g = panel1.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (TXTSE)
            {
                Brush bs = new SolidBrush(customcolor);
                //Font _font = new System.Drawing.Font(new FontFamily("돋움"), 10, FontStyle.Bold);
                g.DrawString(TXT, txtfont, bs, new PointF(rect.Location.X, rect.Location.Y));
                TXTSE = false;
            }
            else if (brushty == "이미지")
            {
                Brush bs = new SolidBrush(customcolor);
                g.DrawImage(Image.FromFile(imagelink), new PointF(rect.Location.X, rect.Location.Y));
            }
            else if (brushty == "아이콘")
            {
                Brush bs = new SolidBrush(customcolor);
                g.DrawImage(Image.FromFile(imagelink), new PointF(rect.Location.X, rect.Location.Y));
            }
            else if (brushty == "스프레이")
            {
                MouseSpray.Enabled = true;
            }
            /*else if(brushty == "찬사각형"){
                Starte = e;
            }
            else if (brushty == "빈사각형")
            {

            }*/
        }
        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            int x, y;
            x = e.X / 20;
            y = e.Y / 20;
            Rectangle rect = new Rectangle(x, y, bsize, bsize)
            {
                Location = e.Location
            };
            Rectangle wrect = new Rectangle(x, y, bsize * 4, bsize * 4)
            {
                Location = e.Location
            };
            Graphics g = panel1.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //if (x < 0 || x >= 24 || y < 0 || y >= 24)
            //return;
            if (TXTSE)
            {
                Font _font = new System.Drawing.Font(new FontFamily("돋움"), 10, FontStyle.Bold);
                g.DrawString(TXT, _font, Brushes.Black, new PointF(rect.Location.X, rect.Location.Y));
                TXTSE = false;
            }
            else if (brushty == "B")
            {
                g.FillEllipse(new SolidBrush(Color.FromArgb(transparency, Color.Black.R, Color.Black.G, Color.Black.B)), rect);
            }
            else if (brushty == "NB")
            {
                Pen nbpen = new Pen(Color.FromArgb(transparency, Color.Black.R, Color.Black.G, Color.Black.B), nbw);
                if (nbbool == false)
                {
                    savepoint = new PointF(rect.Location.X, rect.Location.Y);
                    nbbool = true;
                }
                g.DrawLine(nbpen, new PointF(rect.Location.X, rect.Location.Y), savepoint);
                savepoint = new PointF(rect.Location.X, rect.Location.Y);
            }
            else if (brushty == "NC")
            {
                Pen ncpen = new Pen(Color.FromArgb(transparency, customcolor.R, customcolor.G, customcolor.B), ncw);
                if (ncbool == false)
                {
                    savepoint = new PointF(rect.Location.X, rect.Location.Y);
                    ncbool = true;
                }
                g.DrawLine(ncpen, new PointF(rect.Location.X, rect.Location.Y), savepoint);
                savepoint = new PointF(rect.Location.X, rect.Location.Y);
            }
            else if (brushty == "W")
            {
                g.FillEllipse(wBrush, wrect);
            }
            else if (brushty == "C")
            {
                Brush cBrush = new SolidBrush(Color.FromArgb(transparency, customcolor.R, customcolor.G, customcolor.B));
                g.FillEllipse(cBrush, rect);
            }
        }
        private void Panel1_MouseUp(object sender, MouseEventArgs e)
        {
            nbbool = false;
            ncbool = false;
            if (e.Button != MouseButtons.Left)
                return;
            down = false;
            int x, y;
            x = e.X / 20;
            y = e.Y / 20;
            Graphics g = panel1.CreateGraphics();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            if (brushty == "찬사각형")
            {
                Ende = e;
                Pen cpen = new Pen(customcolor);
                Brush cbrush = new SolidBrush(customcolor);
                Rectangle rect = 사각형(DownP, new Point(Ende.X, Ende.Y));
                g.DrawRectangle(cpen, rect);
                g.FillRectangle(cbrush, rect);
            }
            else if (brushty == "빈사각형")
            {
                Ende = e;
                Pen cpen = new Pen(customcolor);
                Rectangle rect = 사각형(DownP, new Point(Ende.X, Ende.Y));
                g.DrawRectangle(cpen, rect);
            }
            else if (brushty == "찬원")
            {
                Ende = e;
                Pen cpen = new Pen(customcolor);
                Brush cbrush = new SolidBrush(customcolor);
                Rectangle rect = 사각형(DownP, new Point(Ende.X, Ende.Y));
                g.DrawEllipse(cpen, rect);
                g.FillEllipse(cbrush, rect);
            }
            else if (brushty == "빈원")
            {
                Ende = e;
                Pen cpen = new Pen(customcolor);
                Rectangle rect = 사각형(DownP, new Point(Ende.X, Ende.Y));
                g.DrawEllipse(cpen, rect);
            }
            else if (brushty == "스프레이")
            {
                MouseSpray.Enabled = false;
            }

            if (brushty == "찬별1")
            {
                int ix;
                int iy;
                int ex;
                int ey;
                int im;
                if (e.X > DownP.X)
                {
                    ex = e.X - DownP.X;
                    ix = DownP.X;
                }
                else
                {
                    ex = DownP.X - e.X;
                    ix = e.X;
                }

                if (e.Y > DownP.Y)
                {
                    ey = e.Y - DownP.Y;
                    iy = DownP.Y;
                }
                else
                {
                    ey = DownP.Y - e.Y;
                    iy = e.Y;
                }

                Fill_Star1(new Point(ix, iy), new Size(ex, ey));
            }
            if (brushty == "빈별1")
            {
                int ix;
                int iy;
                int ex;
                int ey;
                int im;
                if (e.X > DownP.X)
                {
                    ex = e.X - DownP.X;
                    ix = DownP.X;
                }
                else
                {
                    ex = DownP.X - e.X;
                    ix = e.X;
                }

                if (e.Y > DownP.Y)
                {
                    ey = e.Y - DownP.Y;
                    iy = DownP.Y;
                }
                else
                {
                    ey = DownP.Y - e.Y;
                    iy = e.Y;
                }

                Draw_Star1(new Point(ix, iy), new Size(ex, ey));
            }

            if (brushty == "찬별2")
            {
                int ix;
                int iy;
                int ex;
                int ey;
                int im;
                if (e.X > DownP.X)
                {
                    ex = e.X - DownP.X;
                    ix = DownP.X;
                }
                else
                {
                    ex = DownP.X - e.X;
                    ix = e.X;
                }

                if (e.Y > DownP.Y)
                {
                    ey = e.Y - DownP.Y;
                    iy = DownP.Y;
                }
                else
                {
                    ey = DownP.Y - e.Y;
                    iy = e.Y;
                }

                PointF[] Star1 = Calculate5StarPoints(new PointF(ix + (ex / 2), iy + (ey / 2)), ex, ex / 2);
                g.FillPolygon(new SolidBrush(customcolor), Star1);

                //Fill_Star1(new Point(ix, iy), new Size(ex, ey));
            }
            if (brushty == "빈별2")
            {
                int ix;
                int iy;
                int ex;
                int ey;
                int im;
                if (e.X > DownP.X)
                {
                    ex = e.X - DownP.X;
                    ix = DownP.X;
                }
                else
                {
                    ex = DownP.X - e.X;
                    ix = e.X;
                }

                if (e.Y > DownP.Y)
                {
                    ey = e.Y - DownP.Y;
                    iy = DownP.Y;
                }
                else
                {
                    ey = DownP.Y - e.Y;
                    iy = e.Y;
                }

                PointF[] Star1 = Calculate5StarPoints(new PointF(ix + (ex / 2), iy + (ey / 2)), ex / 2, ex / 4);
                g.DrawPolygon(new Pen(customcolor), Star1);

                //Draw_Star1(new Point(ix, iy), new Size(ex, ey));
            }
            undoint += 1;
            redomax = undoint;
            string FN = appdata + "/CSpaint/" + undoint + ".png";
            var bmp = GraphicsToBitmap(g, Rectangle.Truncate(g.VisibleClipBounds));
            Bitmap snapshot = bmp;
            //snapshot.Save(FN);
            File.WriteAllBytes(FN, ImageToByte(snapshot));

            if (Stream)
                Post("http://" + server_ip + "/Streaming/up/"+Login.snickname, WebUtility.UrlEncode(Convert.ToBase64String(ImageToByte(snapshot))));
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            draw바깥라인();
        }
        private void Panel1_SizeChanged(object sender, EventArgs e)
        {
            
        }
        private void Panel1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Z)
            {
                panel1_undo();
            }
            if(e.KeyCode == Keys.Y)
            {
                panel1_redo();
            }
            if (e.KeyCode == Keys.C)
            {
                Graphics g = panel1.CreateGraphics();
                var bmp = GraphicsToBitmap(g, Rectangle.Truncate(g.VisibleClipBounds));
                Bitmap snapshot = bmp;
                Clipboard.SetImage(bmp);
            }
        }
        private void Panel1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Z)
            {
                panel1_undo();
            }
            if (e.KeyCode == Keys.Y)
            {
                panel1_redo();
            }
            if (e.KeyCode == Keys.C)
            {
                Graphics g = panel1.CreateGraphics();
                var bmp = GraphicsToBitmap(g, Rectangle.Truncate(g.VisibleClipBounds));
                Bitmap snapshot = bmp;
                Clipboard.SetImage(bmp);
            }
        }
        #endregion


        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = panel1.CreateGraphics();
            g.Clear(Color.White);
            draw바깥라인();
            string FN = appdata + "/CSpaint/" + undoint + ".png";
            g.DrawImage(ImageStream(FN), new PointF(0, 0));
        }

        private void 표시선_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked == true)
            {
                show_line = true;
            }
            else
            {
                show_line = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            draw바깥라인();
        }
        private void MousePush_Tick(object sender, EventArgs e)
        {
            Point MousePos = panel1.PointToClient(MousePosition);
            Spray1(panel1.CreateGraphics(), MousePos.X, MousePos.Y, new Size(5, 5), new Pen(customcolor));
        }


        #region ToolStripMenuItem_Click
        private void 모두지우기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Graphics g = panel1.CreateGraphics();
            g.Clear(Color.White);
            draw바깥라인();
            undoint += 1;
            redomax = undoint;
            string FN = appdata + "/CSpaint/" + undoint + ".png";
            var bmp = GraphicsToBitmap(g, Rectangle.Truncate(g.VisibleClipBounds));
            Bitmap snapshot = bmp;
            File.WriteAllBytes(FN, ImageToByte(snapshot));
        }
        private void 흑색ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushty = "B";
        }
        private void 지우개ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushty = "W";
        }
        private void 색선택ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            customcolor = colorDialog1.Color;
        }
        private void 커스텀ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }
        private void 브러쉬로선택ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushty = "C";
        }
        private void 텍스트ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TXTSE = true;
            FontDialog fd = new FontDialog();
            fd.ShowColor = true;
            if (fd.ShowDialog() == DialogResult.OK)
            {
                txtfont = fd.Font;
                customcolor = fd.Color;
            }
            else
            {
                MessageBox.Show("폰트 누락!");
                TXTSE = false;
            }
            string isave = "";
            if (InputBox("TEXT", "New label text:", ref isave) == DialogResult.OK)
            {
                TXT = isave;
            }
            else
            {
                MessageBox.Show("텍스트 누락!");
                TXTSE = false;
            }
        }
        private void 빈사각형ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushty = "빈사각형";
        }
        private void 찬사각형ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushty = "찬사각형";
        }
        private void 찬원ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushty = "찬원";
        }
        private void 빈원ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushty = "빈원";
        }
        private void 이미지ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "*.jpg|*.jpg|*.jpeg|*.jpeg|*.png|*.png|*.bmp|*.bmp";
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                imagelink = ofd.FileName;
            }
            brushty = "이미지";
        }
        private void 아이콘ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "*.ico|*.ico";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                imagelink = ofd.FileName;
            }
            brushty = "아이콘";
        }
        private void 불러오기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Graphics g = panel1.CreateGraphics();
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "*.jpg|*.jpg|*.jpeg|*.jpeg|*.png|*.png|*.bmp|*.bmp";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                g.DrawImage(Image.FromFile(ofd.FileName), new PointF(0,0));
            }
        }
        private void 저장ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Graphics g = panel1.CreateGraphics();
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*.jpg|*.jpg|*.jpeg|*.jpeg|*.png|*.png|*.bmp|*.bmp";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var bmp = GraphicsToBitmap(g, Rectangle.Truncate(g.VisibleClipBounds));
                Bitmap snapshot = bmp;
                snapshot.Save(sfd.FileName);
            }
        }
        private void 복사ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Graphics g = panel1.CreateGraphics();
            var bmp = GraphicsToBitmap(g, Rectangle.Truncate(g.VisibleClipBounds));
            Bitmap snapshot = bmp;
            Clipboard.Clear();
            Clipboard.SetImage(snapshot);
        }
        private void 흑색ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            brushty = "NB";
        }
        private void 커스텀ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            brushty = "NC";
        }
        private void 크기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string isave1 = "";
            int isave2 = 0;
            if (InputBox("TEXT", "New label text:", ref isave1) == DialogResult.OK)
            {
                isave2 = int.Parse(isave1);
                nbw = isave2;
                ncw = isave2;
                bsize = isave2;
            }
            else
            {
                MessageBox.Show("크기 누락!");
            }
        }
        private void 종료ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void 사진업로드ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new Upload_Image(panel1)).ShowDialog();
        }
        private void 사진다운로드ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Image> imgs = new List<Image>();
            List<string> texts = new List<string>();
            foreach (string search in Login.HtmlFromUrl("http://"+server_ip+"/images").Split(new[] { "\n" }, StringSplitOptions.None))
            {
                if (search != "")
                {
                    //Clipboard.SetText((Login.HtmlFromUrl("http://"+server_ip+"/image/down/" + search)));
                    imgs.Add(byteArrayToImage(Convert.FromBase64String((Login.HtmlFromUrl("http://"+server_ip+"/image/down/"+search)))));
                    texts.Add(search);
                }
            }
            (new ImageViewList(imgs, texts, panel1, this)).ShowDialog();
            /*Graphics g = panel1.CreateGraphics();
            g.Clear(Color.White);
            //Clipboard.SetText(WebUtility.UrlDecode(Login.HtmlFromUrl("http://main1.run.goorm.io/image/down")));
            g.DrawImage(byteArrayToImage(Convert.FromBase64String((Login.HtmlFromUrl("http://"+server_ip+"/image/down")))), new Point(0,0));

            Undo_Save();*/
        }
        private void 사진미리보기ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Image img = byteArrayToImage(Convert.FromBase64String((Login.HtmlFromUrl("http://"+server_ip+"/image/down"))));
            (new ImageViewer(img)).Show();
        }
        private void 업데이트ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new Update()).Show();
        }
        private void 스프레이ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushty = "스프레이";
        }
        private void 도회전ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void 빈별ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushty = "빈별1";
        }
        private void 찬별ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushty = "찬별1";
        }
        private void 새빈별ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushty = "빈별2";
        }
        private void 새찬별ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushty = "찬별2";
        }
        private void 채팅ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }
        private void 채팅서버연결ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new RoomSelect()).Show();
        }
        private void 채팅서버추가ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new RoomAdd()).Show();
        }
        private void 접속ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new Streaming_Select()).Show();
        }
        private void 스트리밍ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            
        }
        private void 시작ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string result = Login.HtmlFromUrl("http://" + Form1.server_ip + "/Streaming/isstart/" + Login.snickname);
            if (result == "true")
            {
                MessageBox.Show("이미 스트리밍 중입니다.");
                return;
            }
            else if (result == "false") { }
            else
            {
                MessageBox.Show("서버에서 오류가 생겼습니다.");
                return;
            }
            string code = "";
            InputBox("인증코드 입력", "", ref code);
            if (code == "스트리밍1")
            {
                Stream = true;
                result = Login.HtmlFromUrl("http://" + Form1.server_ip + "/Streaming/start/" + Login.snickname);
                if (result == "true")
                    MessageBox.Show("스트리밍을 시작합니다.");
                else
                    MessageBox.Show("스트리밍을 시작하지 못했습니다.\n사유 : "+result);
            }
            else
                MessageBox.Show("인증코드가 틀렸습니다.");
        }

        private void 투명도ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int transp = transparency;
            if (DialogNumberBox("투명도 설정", "0~255 입니다.", ref transp, 0, 255) == DialogResult.OK)
            {
                transparency = transp;
            }
        }

        private void 금지ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Login.HtmlFromUrl("http://"+Form1.server_ip+ "/Streaming/enable/false");
            MessageBox.Show("이제 스트리밍이 금지됩니다.");
        }

        private void 방삭제ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //RoomSelectDialog(Login.HtmlFromUrl("http://" + Form1.server_ip + "/rooms").Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries));
            RoomSelectDialog rsd = new RoomSelectDialog();
            rsd.Rooms = Login.HtmlFromUrl("http://" + Form1.server_ip + "/rooms").Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (rsd.ShowDialog() == DialogResult.OK)
            {

            }
        }

        private void 강제중지ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RoomSelectDialog rsd = new RoomSelectDialog();
            rsd.Rooms = Login.HtmlFromUrl("http://" + Form1.server_ip + "/Streaming/list").Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (rsd.ShowDialog() == DialogResult.OK)
            {

            }
        }

        private void 금지해제ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Login.HtmlFromUrl("http://" + Form1.server_ip + "/Streaming/enable/true");
            MessageBox.Show("스트리밍 금지가 풀렸습니다.");
        }

        private void 채팅금지ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Login.HtmlFromUrl("http://" + Form1.server_ip + "/Chat/enable/false");
            MessageBox.Show("채팅 금지가 적용되었습니다.");
        }

        private void 채팅허용ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Login.HtmlFromUrl("http://" + Form1.server_ip + "/Chat/enable/true");
            MessageBox.Show("채팅 금지가 풀렸습니다.");
        }

        private void gif로저장ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Image> images = new List<Image>();
            for (int i = 0; i <= redomax; i++)
            {
                string FN = appdata + "/CSpaint/" + i + ".png";
                images.Add(ImageStream(FN));
            }
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "GIF|*.gif";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                GIF.Save(images, sfd.FileName);
            }
        }

        private void 딜레이설정ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NumberBoxDialog nbd = new NumberBoxDialog();
            nbd.max = int.MaxValue;
            nbd.min = 0;
            if (nbd.DialogNumberBox() == DialogResult.OK)
            {
                GIF.Gif_Frame_Delay = nbd.value;
            }
        }

        private void 반복횟수설정ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NumberBoxDialog nbd = new NumberBoxDialog();
            nbd.max = int.MaxValue;
            nbd.min = 0;
            if (nbd.DialogNumberBox() == DialogResult.OK)
            {
                GIF.Gif_Repeat = nbd.value;
            }
        }

        private void 종료ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            string result = Login.HtmlFromUrl("http://" + Form1.server_ip + "/Streaming/isstart/" + Login.snickname);
            if (result == "false")
                MessageBox.Show("스트리밍 중이 아닙니다.");
            else if (result == "true")
            {
                Stream = false;
                MessageBox.Show("스트리밍이 정상적으로 종료되었습니다.");
                Login.HtmlFromUrl("http://" + Form1.server_ip + "/Streaming/end/" + Login.snickname);
            }
            else
                MessageBox.Show("서버에서 오류가 생겼습니다.");
            return;
        }
        #endregion
    }
}
