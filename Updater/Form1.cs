using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        WebClient client;

        public string GetWebPage(string URL)
        {
            System.Net.HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(new Uri(URL));
            {
                var withBlock = Request;
                withBlock.Method = "GET";
                withBlock.MaximumAutomaticRedirections = 4;
                withBlock.MaximumResponseHeadersLength = 4;
                withBlock.ContentLength = 0;
            }

            StreamReader ReadStream = null;
            HttpWebResponse Response = null/* TODO Change to default(_) if this is not a reference type */;
            string ResponseText = string.Empty;

            try
            {
                Response = (HttpWebResponse)Request.GetResponse();
                Stream ReceiveStream = Response.GetResponseStream();
                ReadStream = new StreamReader(ReceiveStream, System.Text.Encoding.UTF8);
                ResponseText = ReadStream.ReadToEnd();
                Response.Close();
                ReadStream.Close();
            }
            catch (Exception)
            {
                ResponseText = string.Empty;
            }
            return ResponseText;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            client = new WebClient();
            client.DownloadProgressChanged += Client_DownloadProgressChanged;
            client.DownloadFileCompleted += Client_DownloadFileCompleted;
        }

        private void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Invoke(new MethodInvoker(delegate ()
            {
                label1.Text = "Extracting update...";
                label1.Refresh();
                progressBar1.Value = 100;
                progressBar1.Refresh();
                try
                {
                    using (FileStream zipStream = new FileStream("PSASBRT.zip", FileMode.Open))
                    {
                        using (ZipArchive updat = new ZipArchive(zipStream))
                        {
                            ZipArchiveExtensions.ExtractToDirectory(updat, ".\\", true);
                        }
                    }
                    File.Delete("PSASBRT.zip");
                    label1.Text = "Done!";
                    label1.Refresh();
                    progressBar1.Value = 100;
                    progressBar1.Refresh();
                    button1.Enabled = true;
                }
                catch (Exception ex)
                {
                    File.Delete("PSASBRT.zip");
                    if (ex is IOException)
                    {
                        MessageBox.Show("An error has occurred while updating files. Be sure to have access to the files, and that the tool is closed.", "Extraction error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("An error has occurred\n\n" + ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    //button1.Enabled = true;
                    label1.Text = "Update failed";
                    label1.Refresh();
                    progressBar1.Value = 0;
                    progressBar1.Refresh();
                }
            }));
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Invoke(new MethodInvoker(delegate ()
            {
                progressBar1.Minimum = 0;
                double received = double.Parse(e.BytesReceived.ToString());
                double total = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = received / total * 100;
                progressBar1.Value = int.Parse(Math.Truncate(percentage).ToString());
            }));
        }

        private void checkUpdate()
        {
            XmlDocument versionInfo = new XmlDocument();
            try
            {
                versionInfo.LoadXml(GetWebPage("https://golbot.altervista.org/PSASBRT/version.xml"));
                Version latestVersion = new Version(versionInfo.SelectSingleNode("//latest").InnerText);
                Version currentVersion = new Version(FileVersionInfo.GetVersionInfo("PSASBR Tool.exe").FileVersion);
                //MessageBox.Show(currentVersion.ToString());
                if (latestVersion > currentVersion)
                {
                    label1.Text = "Downloading update...";
                    label1.Refresh();
                    string url = versionInfo.SelectSingleNode("//url").InnerText;
                    string fileName = versionInfo.SelectSingleNode("//filename").InnerText;
                    //WebClient webClient = new WebClient();
                    //webClient.Headers.Add("Accept: text/html, application/xhtml+xml, */*");
                    /*webClient.Headers.Add("User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)");
                    webClient.DownloadFile(new Uri(url), fileName);*/
                    Thread thread = new Thread(() =>
                    {
                        client.DownloadFileAsync(new Uri(url), fileName);
                    });
                    thread.Start();                 
                }
                else
                {
                    progressBar1.Value = 100;
                    label1.Text = "No updates found";
                    button1.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                if(ex is XmlException)
                {
                    MessageBox.Show("Couldn't retrieve Updates. Check your internet connection", "Connection error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if(ex is FileNotFoundException)
                {
                    MessageBox.Show("Missing PSASBR Tool.exe\n\nBe sure the updater is in the same folder of the tool", "Missing Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("An error has occurred\n\n"+ex, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                //button1.Enabled = true;
                label1.Text = "Update failed";
                label1.Refresh();
                progressBar1.Value = 0;
                progressBar1.Refresh();
            }            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start("PSASBR Tool.exe");
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            //ControlBox = false;
            label1.Text = "Looking for Updates...";
            label1.Refresh();
            checkUpdate();
        }
    }

    public static class ZipArchiveExtensions
    {
        public static void ExtractToDirectory(ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                string completeFileName = Path.Combine(destinationDirectoryName, file.FullName);
                string directory = Path.GetDirectoryName(completeFileName);

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                if (file.Name != "")
                    file.ExtractToFile(completeFileName, true);
            }
        }
    }
}
