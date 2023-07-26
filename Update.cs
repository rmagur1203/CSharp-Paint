using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharp_그림판
{
    public partial class Update : Form
    {
        public Update()
        {
            InitializeComponent();
        }

        private void Update_Load(object sender, EventArgs e)
        {
            int i = 0;
            while (true)
            {
                i++;
                try
                {
                    FtpDownload("ftp://"+Form1.only_server_ip+"/%EA%B7%B8%EB%A6%BC%ED%8C%90/" + i + "/CSharp%20%EA%B7%B8%EB%A6%BC%ED%8C%90.exe", "pi", "raspberry");
                }
                catch (Exception) { break; }
            }
            for (int j = 1; j < i; j++)
                comboBox1.Items.Add("C# 그림판 업데이트 버전 "+j);
        }

        #region Error
        public static string ErrorFtpDownload(string ftpPath, string user = "anonymous",string pwd = "")
        {
            // WebRequest.Create로 Http,Ftp,File Request 객체를 모두 생성할 수 있다.
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(ftpPath);
            // FTP 다운로드한다는 것을 표시
            req.Method = WebRequestMethods.Ftp.DownloadFile;
            // 익명 로그인이 아닌 경우 로그인/암호를 제공해야
            req.Credentials = new NetworkCredential(user, pwd);

            // FTP Request 결과를 가져온다.
            using (FtpWebResponse resp = (FtpWebResponse)req.GetResponse())
            {
                // FTP 결과 스트림
                Stream stream = resp.GetResponseStream();

                // 결과를 문자열로 읽기 (바이너리로 읽을 수도 있다)
                string data;
                using (StreamReader reader = new StreamReader(stream))
                {
                    data = reader.ReadToEnd();
                }

                // return으로 출력
                return data;
            }
        }

        public static string ErrorFtpDownloadData(string ftpPath, string user = "anonymous", string pwd = "")
        {
            // WebRequest.Create로 Http,Ftp,File Request 객체를 모두 생성할 수 있다.
            FtpWebRequest req = (FtpWebRequest)WebRequest.Create(ftpPath);
            // FTP 다운로드한다는 것을 표시
            req.Method = WebRequestMethods.Ftp.DownloadFile;
            // 익명 로그인이 아닌 경우 로그인/암호를 제공해야
            req.Credentials = new NetworkCredential(user, pwd);

            // FTP Request 결과를 가져온다.
            using (FtpWebResponse resp = (FtpWebResponse)req.GetResponse())
            {
                // FTP 결과 스트림
                Stream stream = resp.GetResponseStream();

                // 결과를 문자열로 읽기 (바이너리로 읽을 수도 있다)
                string data;
                using (StreamReader reader = new StreamReader(stream))
                {
                    data = reader.ReadToEnd();
                }

                // 로컬 파일로 출력
                return data;
            }
        }
        #endregion

        public static byte[] FtpDownload(string url, string user = "anonymous", string pwd = "")
        {
            WebClient client = new WebClient();
            client.Credentials = new NetworkCredential(user, pwd);
            return client.DownloadData(url);
        }

        public static byte[] FtpDownloadData(string url, string user = "anonymous", string pwd = "")
        {
            WebClient client = new WebClient();
            client.Credentials = new NetworkCredential(user, pwd);
            return client.DownloadData(url);
        }

        public static string FtpDownloadString(string url, string user = "anonymous", string pwd = "")
        {
            WebClient client = new WebClient();
            client.Credentials = new NetworkCredential(user, pwd);
            return client.DownloadString(url);
        }

        public static void FtpDownloadFile(string url, string outputFile, string user = "anonymous", string pwd = "")
        {
            WebClient client = new WebClient();
            client.Credentials = new NetworkCredential(user, pwd);
            client.DownloadFile(url, outputFile);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int filenum = int.Parse(comboBox1.Text.Split(new[] { "버전 " }, StringSplitOptions.None).Last());
            if (filenum < 32)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.InitialDirectory = Application.StartupPath;
                sfd.Filter = "실행파일|*.exe";
                sfd.FileName = "CSharp_그림판_ver_" + filenum + ".exe";
                string url = "ftp://" + Form1.only_server_ip + "/%EA%B7%B8%EB%A6%BC%ED%8C%90/" + filenum + "/CSharp%20%EA%B7%B8%EB%A6%BC%ED%8C%90.exe";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (Form1.FtpDirectoryExists("ftp://" + Form1.only_server_ip + "/%EA%B7%B8%EB%A6%BC%ED%8C%90/" + filenum + "/DLL", "pi", "raspberry"))
                    {
                        foreach (string search in Form1.ListFiles("ftp://" + Form1.only_server_ip + "/%EA%B7%B8%EB%A6%BC%ED%8C%90/" + filenum + "/DLL", "pi", "raspberry"))
                            FtpDownloadFile(search, new FileInfo(sfd.FileName).DirectoryName + @"\" + search.Split('/').Last(), "pi", "raspberry");
                    }
                    FtpDownloadFile(url, sfd.FileName, "pi", "raspberry");
                }
            }
            else
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    string path = fbd.SelectedPath;
                    string[] FileArray = GetFileList("ftp://" + Form1.only_server_ip + "/%EA%B7%B8%EB%A6%BC%ED%8C%90/" + filenum + "/CSharp_%EA%B7%B8%EB%A6%BC%ED%8C%90", "pi", "raspberry");
                    List<string> FileList = FileArray.ToList();
                    foreach (string FileName in FileList)
                    {
                        MessageBox.Show(FileName);
                    }
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                string filenum = comboBox1.Text.Split(new[] { "버전 " }, StringSplitOptions.None).Last();
                richTextBox1.Text = FtpDownloadString("ftp://"+Form1.only_server_ip+"/%EA%B7%B8%EB%A6%BC%ED%8C%90/" + filenum + "/Patch.txt", "pi", "raspberry");
            }
        }

        public string[] GetFileList(string url, string ftpUserName = "anonymous", string ftpPassWord = "")
        {
            string[] downloadFiles;
            StringBuilder result = new StringBuilder();
            WebResponse response = null;
            StreamReader reader = null;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.UseBinary = true;
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(ftpUserName, ftpPassWord);
                request.KeepAlive = false;
                request.UsePassive = false;
                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream());
                string line = reader.ReadLine();
                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }
                result.Remove(result.ToString().LastIndexOf('\n'), 1);
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
                downloadFiles = null;
                return downloadFiles;
            }
        }
    }
}
