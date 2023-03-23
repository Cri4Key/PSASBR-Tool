using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;
using System.Net;
using Scarlet.IO;
using Scarlet.IO.ImageFormats;
using PSASBR_Tool.FileTypes;

namespace PSASBR_Tool
{
    public partial class Form1 : Form
    {
        private Image previewImage;
        public Form1()
        {
            InitializeComponent();
            treeView1.MouseDown += (sender, args) => treeView1.SelectedNode = treeView1.GetNodeAt(args.X, args.Y);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LinkLabel.Link link = new LinkLabel.Link();
            link.LinkData = "https://github.com/Cri4Key";
            linkLabel1.Links.Add(link);
            
#if DEBUG
            linkLabel1.Text = "DEBUG";
            checkUpdate();
#endif
            if (!Directory.Exists(Path.Combine(Path.GetTempPath(), "PSAST")))
                Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "PSAST"));
        }

        // TreeView methods
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var directory = new FolderBrowserDialog())
            {
                directory.Description = "Select the folder to open under the File Viewer of the program.";
                directory.ShowNewFolderButton = false;
                DialogResult result = directory.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(directory.SelectedPath))
                {
                    PopulateTreeView(directory.SelectedPath);
                    if (previewImage != null)
                    {
                        previewImage.Dispose();
                        previewImage = null;
                    }
                    groupBox1.Invalidate();
                }
            }
        }
        
        private void PopulateTreeView(string path)
        {
            repackCurrentFolderToolStripMenuItem.Enabled = true;
            treeView1.Nodes.Clear();
            label2.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            label5.Visible = false;
            label6.Visible = false;
            label7.Visible = false;
            TreeNode rootNode;

            DirectoryInfo info = new DirectoryInfo(path);
            if (info.Exists)
            {
                rootNode = new TreeNode(info.Name);
                rootNode.Tag = info;
                rootNode.ImageKey = "folder";
                GetDirectoriesAndFiles(info.GetDirectories(), info.GetFiles(), rootNode);
                treeView1.Nodes.Add(rootNode);
            }
        }

        private void GetDirectoriesAndFiles(DirectoryInfo[] subDirs, FileInfo[] subFiles, TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            FileInfo[] subSubFiles;

            foreach (FileInfo subFile in subFiles)
            {
                switch (Path.GetExtension(subFile.FullName))
                {
                    case ".ctxr":
                        aNode = new TreeNode(subFile.Name);
                        aNode.Tag = subFile;
                        aNode.ImageKey = "texture";
                        aNode.SelectedImageKey = "texture";
                        nodeToAddTo.Nodes.Add(aNode);
                        break;

                    case ".cskn":
                    case ".cmdl":
                        aNode = new TreeNode(subFile.Name);
                        aNode.Tag = subFile;
                        aNode.ImageKey = "model";
                        aNode.SelectedImageKey = "model";
                        nodeToAddTo.Nodes.Add(aNode);
                        break;

                    case ".xani":
                        aNode = new TreeNode(subFile.Name);
                        aNode.Tag = subFile;
                        aNode.ImageKey = "archive";
                        aNode.SelectedImageKey = "archive";
                        nodeToAddTo.Nodes.Add(aNode);
                        break;
                }
            }
            foreach (DirectoryInfo subDir in subDirs)
            {
                aNode = new TreeNode(subDir.Name);
                aNode.ImageKey = "folder";
                aNode.SelectedImageKey = "folder";
                aNode.Tag = subDir;
                subSubDirs = subDir.GetDirectories();
                subSubFiles = subDir.GetFiles();
                if (subSubDirs.Length != 0 || subSubFiles.Length != 0)
                {
                    GetDirectoriesAndFiles(subSubDirs, subSubFiles, aNode);
                }
                nodeToAddTo.Nodes.Add(aNode);
            }
        }

        void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode selectedNode = e.Node;

                if (selectedNode.ImageKey != "folder" && selectedNode.ImageKey != "folder_open")
                {
                    FileInfo file = (FileInfo)selectedNode.Tag;
                    string extension = Path.GetExtension(file.FullName);
                    if (extension == ".ctxr")
                        ctxCTXR.Show(treeView1, new Point(e.X, e.Y));

                    else if (extension == ".cmdl" || extension == ".cskn")
                        ctxMDL.Show(treeView1, new Point(e.X, e.Y));

                    else if (extension == ".xani")
                        ctxXANI.Show(treeView1, new Point(e.X, e.Y));
                }
            }
        }

        private void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            TreeNode selectedNode = e.Node;

            if (selectedNode.ImageKey != "folder" && selectedNode.ImageKey != "folder_open")
            {
                FileInfo file = (FileInfo)selectedNode.Tag;
                string extension = Path.GetExtension(file.FullName);
                if (extension == ".ctxr")
                {
                    FileInfo image = Extract_CTXR(file.FullName, false);
                    if (image != null)
                    {
                        if (Path.GetExtension(image.FullName) == ".gxt")
                        {
                            try
                            {
                                previewImage = DecodeGXT(image);
                            }
                            catch (FileNotFoundException)
                            {
                                MessageBox.Show("The Scarlet library is missing. Be sure that both Scarlet.dll and " +
                                    "Scarlet.IO.ImageFormats.dll are placed inside the \"Resources\" folder.",
                                    "Missing Scarlet Lib", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            label2.Text = "Type: GXT Texture";
                            // Retrieves GXT Texture info
                            try
                            {
                                using (FileStream fs = new FileStream(@image.FullName, FileMode.Open, FileAccess.Read))
                                {
                                    using (var reader = new BinaryReader(fs, new ASCIIEncoding()))
                                    {
                                        fs.Seek(0x30, SeekOrigin.Begin);
                                        uint type = reader.ReadUInt32();
                                        uint baseFormat = reader.ReadUInt32();
                                        ushort width = reader.ReadUInt16();
                                        ushort height = reader.ReadUInt16();
                                        ushort mipmaps = reader.ReadUInt16();

                                        switch (type)
                                        {
                                            case 0x00000000:
                                                label6.Text = "Type: Swizzled";
                                                break;

                                            case 0x40000000:
                                                label6.Text = "Type: Cube";
                                                break;

                                            case 0x60000000:
                                                label6.Text = "Type: Linear";
                                                break;

                                            case 0x80000000:
                                                label6.Text = "Type: Tiled";
                                                break;

                                            case 0xA0000000:
                                                label6.Text = "Type: Swizzled Arbitrary";
                                                break;

                                            case 0xC0000000:
                                                label6.Text = "Type: Linear Strided";
                                                break;

                                            case 0xE0000000:
                                                label6.Text = "Type: Cube Arbitrary";
                                                break;

                                            default:
                                                label6.Text = "Type: Unknown";
                                                break;
                                        }

                                        switch (baseFormat)
                                        {
                                            case 0x85000000:
                                                label5.Text = "Format: UBC1 (DXT1)";
                                                break;

                                            case 0x86000000:
                                                label5.Text = "Format: UBC2 (DXT3)";
                                                break;

                                            case 0x87000000:
                                                label5.Text = "Format: UBC3 (DXT5)";
                                                break;

                                            case 0x88000000:
                                                label5.Text = "Format: UBC4 (RGTc1)";
                                                break;

                                            case 0x89000000:
                                                label5.Text = "Format: SBC4 (Signed RGTc1)";
                                                break;

                                            case 0x8A000000:
                                                label5.Text = "Format: UBC5 (RGTc2)";
                                                break;

                                            case 0x8B000000:
                                            case 0x8B001000:
                                                label5.Text = "Format: SBC5 (Signed RGTc2)";
                                                break;

                                            case 0xC001000:
                                                label5.Text = "Format: A8R8G8B8 (No Compression)";
                                                break;

                                            case 0xC005000:
                                                label5.Text = "Format: X8U8U8U8_1RGB (No Compression)";
                                                break;

                                            default:
                                                label5.Text = "Format: Unknown";
                                                break;
                                        }

                                        label3.Text = "Width: " + width.ToString();
                                        label4.Text = "Height: " + height.ToString();
                                        if (mipmaps > 1)
                                            label7.Text = "MIP maps: Yes (" + mipmaps.ToString() + ")";
                                        else
                                            label7.Text = "MIP maps: No";

                                        label2.Visible = true;
                                        label3.Visible = true;
                                        label4.Visible = true;
                                        label5.Visible = true;
                                        label6.Visible = true;
                                        label7.Visible = true;
                                    }
                                }
                            }
                            catch (IOException)
                            {
                                string error = "Access to " + Path.GetFileName(image.FullName) + " denied. Be sure the file isn't opened by some other program and that the tool has got enough privileges to open it.";
                                MessageBox.Show(error, "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }
                        else
                        {
                            previewImage = Image.FromFile(image.FullName);
                            label2.Text = "Type: JPEG Texture";
                            label3.Text = "Width: " + previewImage.Width;
                            label4.Text = "Height: " + previewImage.Height;
                            label5.Text = "Format: JFIF";
                            label2.Visible = true;
                            label3.Visible = true;
                            label4.Visible = true;
                            label5.Visible = true;
                            label6.Visible = false;
                            label7.Visible = false;
                        }
                    }
                    groupBox1.Invalidate();
                }
                else if (extension == ".cmdl" || extension == ".cskn")
                {
                    if (previewImage != null)
                    {
                        previewImage.Dispose();
                        previewImage = null;
                    }
                    groupBox1.Invalidate();
                    label2.Text = "Type: Model";
                    if (extension == ".cskn")
                        label3.Text = "Rigged: Yes";
                    else
                        label3.Text = "Rigged: No";
                    label2.Visible = true;
                    label3.Visible = true;
                    label4.Visible = false;
                    label5.Visible = false;
                    label6.Visible = false;
                    label7.Visible = false;
                }
                else if (extension == ".xani")
                {
                    if (previewImage != null)
                    {
                        previewImage.Dispose();
                        previewImage = null;
                    }
                    groupBox1.Invalidate();
                    label2.Text = "Type: Animation Bundle";
                    label3.Text = "Number of Animations: ";
                    using (FileStream fs = new FileStream(@file.FullName, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = new BinaryReader(fs))
                        {
                            label3.Text += reader.ReadUInt32().ToString();
                        }
                    }
                    label2.Visible = true;
                    label3.Visible = true;
                    label4.Visible = false;
                    label5.Visible = false;
                    label6.Visible = false;
                    label7.Visible = false;
                }
            }
        }


        // PSARC extraction methods
        private void extractPSARCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!File.Exists("Resources\\psp2psarc.exe"))
            {
                MessageBox.Show("Missing psp2psarc.exe\nRetrieve it online and put it inside the \"Resources\" folder to use the PSARC Tools", "Missing file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (OpenFileDialog psarc = new OpenFileDialog())
            {
                psarc.Title = "Select a PSARC file";
                psarc.Filter = "PSARC Archive|*.psarc|All files|*.*";
                psarc.Multiselect = false;

                if (psarc.ShowDialog() == DialogResult.OK)
                {
                    bool isValid = false;

                    using (FileStream fs = new FileStream(@psarc.FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = new BinaryReader(fs, new ASCIIEncoding()))
                        {
                            if (reader.ReadUInt32() == 0x52415350)
                                isValid = true;
                        }
                    }

                    if (isValid)
                    {
                        string destinationDir = "EXTRACTED PSARC\\" + Path.GetFileNameWithoutExtension(psarc.FileName);
                        destinationDir = destinationDir.Replace(".", null);
                        bgWorker = new BackgroundWorker();
                        bgWorker.WorkerReportsProgress = true;
                        bgWorker.DoWork += new DoWorkEventHandler(BgWorker_ExtractPSARC);
                        bgWorker.ProgressChanged += new ProgressChangedEventHandler(BgWorker_ExtractProgress);

                        if (!Directory.Exists(destinationDir))
                        {
                            bgWorker.RunWorkerAsync(new string[] { psarc.FileName, destinationDir });
                        }
                        else
                        {
                            string message = "There is already a folder containing an extracted \"" + 
                                Path.GetFileNameWithoutExtension(psarc.FileName) + "\" PSARC archive. If you choose to" +
                                " continue, all conflicting files in that folder will be overwritten with the ones that" +
                                " will be now extracted.";
                            DialogResult risultato = MessageBox.Show(message, "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (risultato == DialogResult.Yes)
                            {
                                bgWorker.RunWorkerAsync(new string[] { psarc.FileName, destinationDir });
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("The chosen file is not a valid PSARC!", "Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BgWorker_ExtractPSARC(object sender, DoWorkEventArgs e)
        {
            string[] paths = e.Argument as string[];
            bgWorker.ReportProgress(50);
            ExtractPSARC(paths[0], paths[1]);
            DialogResult result = MessageBox.Show("Done!\n\nDo you want to open the extracted PSARC under the file viewer?", "Completed", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
                bgWorker.ReportProgress(0, paths[1]);
            else
                bgWorker.ReportProgress(0);

        }

        private void BgWorker_ExtractProgress(object sender, ProgressChangedEventArgs e)
        {  
            if (e.ProgressPercentage != 0)
            {
                label1.Text = "Extracting the PSARC...";
                progressBar1.Style = ProgressBarStyle.Marquee;
                progressBar1.MarqueeAnimationSpeed = e.ProgressPercentage;
                menuStrip1.Enabled = false;
            }
            else
            {
                label1.Text = "Ready";
                progressBar1.MarqueeAnimationSpeed = e.ProgressPercentage;
                progressBar1.Style = ProgressBarStyle.Blocks;
                progressBar1.Value = progressBar1.Minimum;
                menuStrip1.Enabled = true;
                if (!string.IsNullOrWhiteSpace((string)e.UserState))
                    PopulateTreeView((string)e.UserState);
            }
        }

        private void ExtractPSARC(string psarc, string destinationDir)
        {
            string codeLine = "extract -y \"" + psarc + "\" --to=\"" + Path.GetFullPath(destinationDir) + "\"";
            codeLine = codeLine.Replace("\\", "/");

            string path = "Resources\\psp2psarc.exe";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                FileName = path,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = codeLine
            };

            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                    exeProcess.WaitForExit();
            }
#if !DEBUG
            catch
            {
                MessageBox.Show("Something went wrong while trying to run the process... Be sure that psp2psarc.exe is inside the Resources folder", "Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#else
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
#endif
        }

        // PSARC repack methods
        private void repackPSARCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!File.Exists("Resources\\psp2psarc.exe"))
            {
                MessageBox.Show("Missing psp2psarc.exe\nRetrieve it online and put it inside the \"Resources\" folder to use the psarc Tools", "Missing file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string destinationDir = "REPACKED PSARC\\";

            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            using (var psarc = new FolderBrowserDialog())
            {
                psarc.Description = "All the files contained inside the folder selected will be repacked inside a PSARC Archive.";
                psarc.SelectedPath = Path.GetFullPath("EXTRACTED PSARC\\");
                psarc.ShowNewFolderButton = false;
                DialogResult result = psarc.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(psarc.SelectedPath))
                {
                    string[] files = Directory.GetDirectories(psarc.SelectedPath);

                    string codeLine = "";
                    for (int i = 0; i < files.Length; i++)
                    {
                        codeLine = codeLine + "\"" + files[i] + "\" ";
                    }

                    string psarcName = Path.GetFileName(psarc.SelectedPath) + ".psarc";
                    bgWorker = new BackgroundWorker();
                    bgWorker.WorkerReportsProgress = true;
                    bgWorker.DoWork += new DoWorkEventHandler(BgWorker_RepackPSARC);
                    bgWorker.ProgressChanged += new ProgressChangedEventHandler(BgWorker_RepackProgress);

                    if (File.Exists(Path.Combine(destinationDir, psarcName)))
                    {
                        DialogResult risultato = MessageBox.Show("There is a PSARC archive named \"" + psarcName + "\" inside the REPACKED PSARC folder. If you choose to continue, it will be overwritten with the new one.", "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (risultato == DialogResult.Yes)
                        {
                            bgWorker.RunWorkerAsync(new string[] { codeLine, psarcName, destinationDir, psarc.SelectedPath });
                            //CreatePSARC(codeLine, psarcName, destinationDir, psarc.SelectedPath);
                        }
                    }
                    else
                    {
                        bgWorker.RunWorkerAsync(new string[] { codeLine, psarcName, destinationDir, psarc.SelectedPath });
                    }
                }
            }
        }

        private void BgWorker_RepackPSARC(object sender, DoWorkEventArgs e)
        {
            string[] paths = e.Argument as string[];
            bgWorker.ReportProgress(50);
            CreatePSARC(paths[0], paths[1], paths[2], paths[3]);
            bgWorker.ReportProgress(0);
            MessageBox.Show("Done!", "Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Process.Start(Path.GetFullPath(paths[2]));
        }

        private void BgWorker_RepackProgress(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage != 0)
            {
                label1.Text = "Repacking the PSARC...";
                progressBar1.Style = ProgressBarStyle.Marquee;
                progressBar1.MarqueeAnimationSpeed = e.ProgressPercentage;
                menuStrip1.Enabled = false;
            }
            else
            {
                label1.Text = "Ready";
                progressBar1.MarqueeAnimationSpeed = e.ProgressPercentage;
                progressBar1.Style = ProgressBarStyle.Blocks;
                progressBar1.Value = progressBar1.Minimum;
                menuStrip1.Enabled = true;
            }
        }

        private void CreatePSARC(string directories, string name, string destinationDir, string strip)
        {
            string codeLine = "-y -a " + directories + " --strip=\"" + strip + "\" --output=\"" + Path.GetFullPath(destinationDir) + name + "\"";
            codeLine = codeLine.Replace("\\", "/");

            string path = "Resources\\psp2psarc.exe";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                FileName = path,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = codeLine
            };

            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                    exeProcess.WaitForExit();
            }
#if !DEBUG
            catch
            {
                MessageBox.Show("Something went wrong while trying to run the process... Be sure that psp2psarc.exe is inside the Resources folder", "Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#else
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
#endif
        }

        // Textures methods
        private byte[] CreateTXTR(string image)
        {
            var chars = "abdcefghijklmnopqrstuvwxyz0123456789";
            var stringChars2 = new char[8];
            var random = new Random();
            for (int i = 0; i < stringChars2.Length; i++)
            {
                stringChars2[i] = chars[random.Next(chars.Length)];
            }
            var txtrName = new String(stringChars2);

            string txtrPath = Path.Combine(Path.GetTempPath(), "PSAST", txtrName);
            string codeLine = "-i \"" + image + "\" -o \"" + txtrPath + "\"";
            codeLine = codeLine.Replace("\\", "/");

            string path = "Resources\\psp2gxt.exe";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = false,
                FileName = path,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = codeLine
            };

            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                    exeProcess.WaitForExit();
                byte[] txtr = File.ReadAllBytes(txtrPath);
                return txtr;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private static Bitmap DecodeGXT(FileInfo inputFile)
        {
            try
            {
                char[] directorySeparators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };

                using (FileStream inputStream = new FileStream(inputFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var instance = FileFormat.FromFile<FileFormat>(inputStream);
                    if (instance != null)
                    {
                        if (instance is ImageFormat)
                        {
                            var imageInstance = (instance as ImageFormat);

                            int imageCount = imageInstance.GetImageCount();
                            int paletteCount = imageInstance.GetPaletteCount();

                            string imageName = imageInstance.GetImageName(0);
                            if (imageInstance is GXT && (imageInstance as GXT).BUVChunk != null)
                            {
                                var gxtInstance = (imageInstance as GXT);

                                List<Bitmap> buvImages = gxtInstance.GetBUVBitmaps().ToList();
                                Bitmap image = buvImages[0];
                                return image;
                            }
                            else
                            {
                                Bitmap image = imageInstance.GetBitmap(0, 0);
                                return image;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return null;
        }

        private FileInfo Extract_CTXR(string ctxr, bool update)
        {
            if (previewImage != null)
            {
                previewImage.Dispose();
                previewImage = null;
            } 
            string final_jfif = Path.Combine(Path.GetTempPath(), "PSAST", Path.GetFileNameWithoutExtension(ctxr) + ".jpg");
            string final_gxt = Path.Combine(Path.GetTempPath(), "PSAST", Path.GetFileNameWithoutExtension(ctxr) + ".gxt");
            if (File.Exists(final_gxt) && !update)
                return new FileInfo(final_gxt);
            if (File.Exists(final_jfif) && !update)
                return new FileInfo(final_jfif);

            try
            {
                using (FileStream fs = new FileStream(@ctxr, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new BinaryReader(fs, new ASCIIEncoding()))
                    {
                        uint txtrHeader = reader.ReadUInt32();
                        byte[] txtrData = null;
                        bool isValid = false;
                        uint signature;

                        // Check if the file is a valid CTXR file
                        if (txtrHeader == 0x52545854)
                        {
                            while ((isValid == false) && (reader.PeekChar() != -1)) // Search for GXT Magic and removes TXTR data
                            {
                                signature = reader.ReadUInt32();

                                if (signature == 0x545847)
                                {
                                    isValid = true;
                                    long finalpos = fs.Position;
                                    finalpos = fs.Length - (finalpos - 4);
                                    fs.Position = fs.Position - 4;

                                    txtrData = reader.ReadBytes((int)finalpos);
                                    try
                                    {
                                        File.WriteAllBytes(final_gxt, txtrData);
                                        return new FileInfo(final_gxt);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.ToString(), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                                // Eventually checks for JFIF
                                else if (signature == 0xE0FFD8FF)
                                {
                                    if (reader.ReadUInt64() == 0x1004649464A1000)
                                    {
                                        // If JFIF found, straight extract the data and save it
                                        isValid = true;
                                        long finalpos = fs.Position;
                                        finalpos = fs.Length - (finalpos - 12);
                                        fs.Position = fs.Position - 12;

                                        txtrData = reader.ReadBytes((int)finalpos);
                                        try
                                        {
                                            File.WriteAllBytes(final_jfif, txtrData);
                                            return new FileInfo(final_jfif);
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show(ex.ToString(), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("The file " + Path.GetFileName(ctxr) + " is not valid!", "Bad Texture", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (IOException)
            {
                string error = "Access to " + Path.GetFileName(ctxr) + " denied. Be sure the file isn't opened by some other program and that the tool has got enough privileges to open it.";
                MessageBox.Show(error, "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException)
            {
                string error = "Unable to load the" + Path.GetFileName(ctxr) + " file. This CTXR format is not supported.";
                MessageBox.Show(error, "Not valid CTXR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        private byte[] Load_TXTR(string ctxr)
        {
            try
            {
                using (FileStream fs = new FileStream(@ctxr, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = new BinaryReader(fs, new ASCIIEncoding()))
                    {
                        long length = fs.Length;
                        uint signature;
                        long finalpos;

                        if (reader.ReadUInt32() == 0x52545854)
                        {
                            // Search for GXT Magic and returns TXTR data
                            while (reader.PeekChar() != -1)
                            {
                                signature = reader.ReadUInt32();
                                if (signature == 0x545847)
                                {
                                    finalpos = fs.Position;
                                    finalpos = finalpos - 4;
                                    fs.Position = 0;

                                    return reader.ReadBytes((int)finalpos);
                                }
                                else if (signature == 0xE0FFD8FF) // Eventually checks for JFIF
                                {
                                    if (reader.ReadUInt64() == 0x1004649464A1000)
                                    {
                                        // If JFIF found, returns its TXTR data
                                        finalpos = fs.Position;
                                        finalpos = finalpos - 12;
                                        fs.Position = 0;

                                        return reader.ReadBytes((int)finalpos);
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("The file " + Path.GetFileName(ctxr) + " is not valid!", "Bad CTXR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (IOException)
            {
                string error = "Access to " + Path.GetFileName(ctxr) + " denied. Be sure the file isn't opened by some other program and that the tool has got enough privileges to open it.";
                MessageBox.Show(error, "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IndexOutOfRangeException)
            {
                string error = "Unable to load the" + Path.GetFileName(ctxr) + " file. This CTXR format is not supported.";
                MessageBox.Show(error, "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("PSASBR Tool v2.0.0\n© 2020 Cri4Key\n\nThis program uses the following libraries:\nScarlet\n© 2016 xdaniel (Daniel R.) / DigitalZero Domain");
        }

        private void Viewer_Paint(object sender, PaintEventArgs e)
        {
            if (previewImage == null)
                return;
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            int width = previewImage.Width;
            int height = previewImage.Height;
            Rectangle client = groupBox1.ClientRectangle;
            Rectangle bounds = new Rectangle(22, 25, width, height);
            float ratio = (float)width / height;
            float newRatio = (float)client.Width / client.Height;
            float scale = (newRatio > ratio) ? (float)client.Height / height : (float)client.Width / width;

            bounds.Width = (int)(width * scale);
            bounds.Height = (int)(height * scale);
            bounds.X = (client.Width - bounds.Width) >> 1;
            bounds.Y = (client.Height - bounds.Height) >> 1;

            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.CompositingMode = CompositingMode.SourceOver;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            HatchBrush brush = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.LightGray, Color.GhostWhite);
            g.FillRectangle(brush, bounds);
            g.DrawImage(previewImage, bounds);
            g.Flush();
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.ImageKey = "folder_open";
            e.Node.SelectedImageKey = "folder_open";
        }

        private void treeView1_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.ImageKey = "folder";
            e.Node.SelectedImageKey = "folder";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData as string);
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "Select where you want to save the exported image";
                dialog.FileName = Path.GetFileNameWithoutExtension(treeView1.SelectedNode.Tag.ToString());
                dialog.OverwritePrompt = true;

                if (label2.Text.Equals("Type: JPEG Texture"))
                {
                    dialog.Filter = "JPEG Image|*.jpg";
                    
                    if (dialog.ShowDialog() == DialogResult.OK && dialog.FileName != "")
                    {
                        using (FileStream fs = (FileStream)dialog.OpenFile())
                        {
                            previewImage.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                        }
                    }
                }
                else
                {
                    dialog.Filter = "PNG Image|*.png";

                    if (dialog.ShowDialog() == DialogResult.OK && dialog.FileName != "")
                    {
                        using (FileStream fs = (FileStream)dialog.OpenFile())
                        {
                            previewImage.Save(fs, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }
            }
        }

        private void repackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo ctxrFile = (FileInfo)treeView1.SelectedNode.Tag;
            
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Title = "Select the image to repack in the selected CTXR";
                bool isGXT = !label2.Text.Equals("Type: JPEG Texture");
                if(isGXT && !File.Exists("Resources\\psp2gxt.exe"))
                {
                    MessageBox.Show("Missing psp2gxt.exe\nRetrieve it online and put it inside the \"Resources\" folder in order to repack the GXT Texture type.", "Missing file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (isGXT)
                    dialog.Filter = "DDS Image|*.dds";
                else
                    dialog.Filter = "JPEG Image|*.jpg; *.jfif";

                if(dialog.ShowDialog() == DialogResult.OK && dialog.FileName != "")
                {
                    byte[] txtrData = Load_TXTR(ctxrFile.FullName);
                    byte[] imageData;
                    if (isGXT)
                        imageData = CreateTXTR(dialog.FileName);
                    else
                        imageData = File.ReadAllBytes(dialog.FileName);

                    if (imageData == null)
                        return;

                    byte[] finalTxtr = new byte[txtrData.Length + imageData.Length];
                    Buffer.BlockCopy(txtrData, 0, finalTxtr, 0, txtrData.Length);
                    Buffer.BlockCopy(imageData, 0, finalTxtr, txtrData.Length, imageData.Length);
                    File.WriteAllBytes(Path.GetFullPath(ctxrFile.FullName), finalTxtr);
                    MessageBox.Show("Done!", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Update the preview with the new repacked CTXR
                    FileInfo image = Extract_CTXR(ctxrFile.FullName, true);
                    if (Path.GetExtension(image.FullName) == ".gxt")
                    {
                        try
                        {
                            previewImage = DecodeGXT(image);
                        }
                        catch (FileNotFoundException)
                        {
                            MessageBox.Show("The Scarlet library is missing. Be sure that both Scarlet.dll and " +
                                "Scarlet.IO.ImageFormats.dll are placed inside the \"Resources\" folder.",
                                "Missing Scarlet Lib", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        label2.Text = "Type: GXT Texture";
                        // Retrieves GXT Texture info
                        try
                        {
                            using (FileStream fs = new FileStream(@image.FullName, FileMode.Open, FileAccess.Read))
                            {
                                using (var reader = new BinaryReader(fs, new ASCIIEncoding()))
                                {
                                    fs.Seek(0x30, SeekOrigin.Begin);
                                    uint type = reader.ReadUInt32();
                                    uint baseFormat = reader.ReadUInt32();
                                    ushort width = reader.ReadUInt16();
                                    ushort height = reader.ReadUInt16();
                                    ushort mipmaps = reader.ReadUInt16();


                                    switch (type)
                                    {
                                        case 0x00000000:
                                            label6.Text = "Type: Swizzled";
                                            break;

                                        case 0x40000000:
                                            label6.Text = "Type: Cube";
                                            break;

                                        case 0x60000000:
                                            label6.Text = "Type: Linear";
                                            break;

                                        case 0x80000000:
                                            label6.Text = "Type: Tiled";
                                            break;

                                        case 0xA0000000:
                                            label6.Text = "Type: Swizzled Arbitrary";
                                            break;

                                        case 0xC0000000:
                                            label6.Text = "Type: Linear Strided";
                                            break;

                                        case 0xE0000000:
                                            label6.Text = "Type: Cube Arbitrary";
                                            break;

                                        default:
                                            label6.Text = "Type: Unknown";
                                            break;
                                    }

                                    switch (baseFormat)
                                    {
                                        case 0x85000000:
                                            label5.Text = "Format: UBC1 (DXT1)";
                                            break;

                                        case 0x86000000:
                                            label5.Text = "Format: UBC2 (DXT3)";
                                            break;

                                        case 0x87000000:
                                            label5.Text = "Format: UBC3 (DXT5)";
                                            break;

                                        case 0x88000000:
                                            label5.Text = "Format: UBC4 (RGTc1)";
                                            break;

                                        case 0x89000000:
                                            label5.Text = "Format: SBC4 (Signed RGTc1)";
                                            break;

                                        case 0x8A000000:
                                            label5.Text = "Format: UBC5 (RGTc2)";
                                            break;

                                        case 0x8B000000:
                                        case 0x8B001000:
                                            label5.Text = "Format: SBC5 (Signed RGTc2)";
                                            break;

                                        case 0xC001000:
                                            label5.Text = "Format: A8R8G8B8 (No Compression)";
                                            break;

                                        case 0xC005000:
                                            label5.Text = "Format: X8U8U8U8_1RGB (No Compression)";
                                            break;

                                        default:
                                            label5.Text = "Format: Unknown";
                                            break;
                                    }

                                    label3.Text = "Width: " + width.ToString();
                                    label4.Text = "Height: " + height.ToString();
                                    if (mipmaps > 1)
                                        label7.Text = "MIP maps: Yes (" + mipmaps.ToString() + ")";
                                    else
                                        label7.Text = "MIP maps: No";

                                    label2.Visible = true;
                                    label3.Visible = true;
                                    label4.Visible = true;
                                    label5.Visible = true;
                                    label6.Visible = true;
                                    label7.Visible = true;
                                }
                            }
                        }
                        catch (IOException)
                        {
                            string error = "Access to " + Path.GetFileName(image.FullName) + " denied. Be sure the file isn't opened by some other program and that the tool has got enough privileges to open it.";
                            MessageBox.Show(error, "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else
                    {
                        previewImage = Image.FromFile(image.FullName);
                        label2.Text = "Type: JPEG Texture";
                        label3.Text = "Width: " + previewImage.Width;
                        label4.Text = "Height: " + previewImage.Height;
                        label5.Text = "Format: JFIF";
                        label2.Visible = true;
                        label3.Visible = true;
                        label4.Visible = true;
                        label5.Visible = true;
                        label6.Visible = false;
                        label7.Visible = false;
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (previewImage != null)
            {
                previewImage.Dispose();
                previewImage = null;
            }
            Directory.Delete(Path.Combine(Path.GetTempPath(), "PSAST"), true);
        }

        private void extXANIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo xaniFile = (FileInfo)treeView1.SelectedNode.Tag;
            XANI loadedXani;
            try
            {
                loadedXani = new XANI(xaniFile.FullName);
                using (var psarc = new FolderBrowserDialog())
                {
                    psarc.Description = "Select where to extract the XANI bundle.";
                    psarc.ShowNewFolderButton = true;
                    DialogResult result = psarc.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(psarc.SelectedPath))
                    {
                        bgWorker = new BackgroundWorker();
                        bgWorker.WorkerReportsProgress = true;
                        bgWorker.DoWork += new DoWorkEventHandler(BgWorker_ExtractXANI);
                        bgWorker.ProgressChanged += new ProgressChangedEventHandler(BgWorker_XaniExProgress);

                        object[] parameters = new object[] { loadedXani, psarc.SelectedPath };
                        bgWorker.RunWorkerAsync(parameters);
                    }
                }
            }
            catch (ArgumentException)
            {
                MessageBox.Show("The selected XANI is empty. It cannot be extracted.", "Empty XANI", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BgWorker_ExtractXANI(object sender, DoWorkEventArgs e)
        {
            object[] paths = e.Argument as object[];
            bgWorker.ReportProgress(50);
            XANI loaded = (XANI)paths[0];
            loaded.ExtractXANI((string)paths[1]);
            bgWorker.ReportProgress(0);
            MessageBox.Show("Done!", "Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Process.Start(Path.GetFullPath((string)paths[1]));
        }

        private void BgWorker_XaniExProgress(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage != 0)
            {
                label1.Text = "Extracting the XANI Bundle...";
                progressBar1.Style = ProgressBarStyle.Marquee;
                progressBar1.MarqueeAnimationSpeed = e.ProgressPercentage;
                menuStrip1.Enabled = false;
            }
            else
            {
                label1.Text = "Ready";
                progressBar1.MarqueeAnimationSpeed = e.ProgressPercentage;
                progressBar1.Style = ProgressBarStyle.Blocks;
                progressBar1.Value = progressBar1.Minimum;
                menuStrip1.Enabled = true;
            }
        }

        private void repackCurrentFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!File.Exists("Resources\\psp2psarc.exe"))
            {
                MessageBox.Show("Missing psp2psarc.exe\nRetrieve it online and put it inside the \"Resources\" folder to use the psarc Tools", "Missing file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string destinationDir = "REPACKED PSARC\\";

            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            TreeNode root = treeView1.SelectedNode;
            while (root.Parent != null)
                root = root.Parent;

            DirectoryInfo selectedDir = (DirectoryInfo)root.Tag;

            string[] files = Directory.GetDirectories(selectedDir.FullName);

            string codeLine = "";
            for (int i = 0; i < files.Length; i++)
            {
                codeLine = codeLine + "\"" + files[i] + "\" ";
            }

            string psarcName = Path.GetFileName(selectedDir.FullName) + ".psarc";
            bgWorker = new BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;
            bgWorker.DoWork += new DoWorkEventHandler(BgWorker_RepackPSARC);
            bgWorker.ProgressChanged += new ProgressChangedEventHandler(BgWorker_RepackProgress);

            if (File.Exists(Path.Combine(destinationDir, psarcName)))
            {
                DialogResult risultato = MessageBox.Show("There is a PSARC archive named \"" + psarcName + "\" inside the REPACKED PSARC folder. If you choose to continue, it will be overwritten with the new one.", "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (risultato == DialogResult.Yes)
                {
                    bgWorker.RunWorkerAsync(new string[] { codeLine, psarcName, destinationDir, selectedDir.FullName });
                }
            }
            else
            {
                bgWorker.RunWorkerAsync(new string[] { codeLine, psarcName, destinationDir, selectedDir.FullName });
            }
        }

        private void openInNoesisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists("Noesis"))
            {
                string message = "PSASBR Tool doesn't have native support for PSASBR Models. However, you can " +
                "view and export models from the game using Noesis and the PSASBR Noesis Script.\n\nFor more" +
                " info, check the GitHub page for the script.\n\nPlace the Noesis files inside a" +
                " \"Noesis\" folder, which should be located in the same folder of this program in order to " +
                "use the shortcuts for opening/exporting the models.\n\nDo you want to visit the page now?";
                DialogResult result = MessageBox.Show(message, "About models...", MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                    Process.Start("https://github.com/Cri4Key/PSAS-Vita-Noesis");
                return;
            }

            FileInfo model = (FileInfo)treeView1.SelectedNode.Tag;
            try
            {
                Process.Start("Noesis\\Noesis.exe", model.FullName);
            }
            catch (Win32Exception)
            {
                MessageBox.Show("Error while trying to open Noesis.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void exportMDLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists("Noesis"))
            {
                string message = "PSASBR Tool doesn't have native support for PSASBR Models. However, you can " +
                "view and export models from the game using Noesis and the PSASBR Noesis Script.\n\nFor more" +
                " info, check the GitHub page for the script.\n\nPlace the Noesis files inside a" +
                " \"Noesis\" folder, which should be located in the same folder of this program in order to " +
                "use the shortcuts for opening/exporting the models.\n\nDo you want to visit the page now?";
                DialogResult result = MessageBox.Show(message, "About models...", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                    Process.Start("https://github.com/Cri4Key/PSAS-Vita-Noesis");
                return;
            }

            FileInfo model = (FileInfo)treeView1.SelectedNode.Tag;
            try
            {
                using (var destDir = new FolderBrowserDialog())
                {
                    destDir.Description = "Select the folder where to export the model.";
                    destDir.ShowNewFolderButton = true;
                    DialogResult result = destDir.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(destDir.SelectedPath))
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            CreateNoWindow = true,
                            FileName = "Noesis\\Noesis.exe",
                            WindowStyle = ProcessWindowStyle.Hidden,
                            Arguments = "?cmode \"" + model.FullName + "\" \"" + destDir.SelectedPath + "\\"
                            + Path.GetFileNameWithoutExtension(model.FullName) + ".fbx\""
                        };
                        /*using (Process exeProcess = Process.Start(startInfo))
                            exeProcess.WaitForExit();*/
                        Process.Start(startInfo);
                        MessageBox.Show(startInfo.Arguments);
                    }
                }
            }
            catch (Win32Exception)
            {
                MessageBox.Show("Error while trying to open Noesis.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (!(e.Button == MouseButtons.Left) || e.Node.ImageKey != "model")
                return;
            
            if (!Directory.Exists("Noesis"))
            {
                string message = "PSASBR Tool doesn't have native support for PSASBR Models. However, you can " +
                "view and export models from the game using Noesis and the PSASBR Noesis Script.\n\nFor more" +
                " info, check the GitHub page for the script.\n\nPlace the Noesis files inside a" +
                " \"Noesis\" folder, which should be located in the same folder of this program in order to " +
                "use the shortcuts for opening/exporting the models.\n\nDo you want to visit the page now?";
                DialogResult result = MessageBox.Show(message, "About models...", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);
                if (result == DialogResult.Yes)
                    Process.Start("https://github.com/Cri4Key/PSAS-Vita-Noesis");
                return;
            }

            FileInfo model = (FileInfo)treeView1.SelectedNode.Tag;
            try
            {
                Process.Start("Noesis\\Noesis.exe", model.FullName);
            }
            catch (Win32Exception)
            {
                MessageBox.Show("Error while trying to open Noesis.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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

        private void checkUpdate()
        {
            XmlDocument versionInfo = new XmlDocument();
            try
            {
                versionInfo.LoadXml(GetWebPage("https://golbot.altervista.org/PSASBRT/version.xml"));
                Version latestVersion = new Version(versionInfo.SelectSingleNode("//latest").InnerText);
                Version currentVersion = new Version(Application.ProductVersion);
                if (latestVersion < currentVersion)
                {
                    MessageBox.Show("No update");
                }
                
            }
            catch (Exception ex)
            {
                if (ex is XmlException)
                {
                    MessageBox.Show("Couldn't retrieve Updates. Check your internet connection", "Connection error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (ex is FileNotFoundException)
                {
                    MessageBox.Show("Missing PSASBR Tool.exe\n\nBe sure the updater is in the same folder of the tool", "Missing Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        }
    }
}
