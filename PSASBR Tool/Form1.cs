using System;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Media;

namespace PSASBR_Tool
{
    public partial class Form1 : Form
    {
        SoundPlayer player = null;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            player = new SoundPlayer(PSASBR_Tool.Properties.Resources.gui);
            player.PlayLooping();
            LinkLabel.Link link = new LinkLabel.Link();
            link.LinkData = "https://github.com/Cri4Key";
            linkLabel1.Links.Add(link);
            if(!File.Exists("Resources\\psp2psarc.exe"))
            {
                button1.Enabled = false;
                button1.Text = "Missing psp2psarc.exe";
                button4.Enabled = false;
                button4.Text = "Missing psp2psarc.exe";
                MessageBox.Show("Missing psp2psarc.exe\nRetrieve it online and put it inside the \"Resources\" folder to use the Package Tools", "Missing file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            if (!File.Exists("Resources\\psp2gxt.exe"))
            {
                button6.Text = "Repack CTXR (No DDS)";
                MessageBox.Show("Missing psp2gxt.exe\nRetrieve it online and put it inside the \"Resources\" folder to repack textures using the DDS format", "Missing file", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog PSARC = new OpenFileDialog())
            {
                PSARC.Title = "Select a PSARC file";
                PSARC.Filter = "PSARC Archive|*.psarc";
                PSARC.Multiselect = false;
                //PSARC.InitialDirectory = Directory.GetCurrentDirectory();

                if (PSARC.ShowDialog() == DialogResult.OK)
                {
                    bool isValid = false;
                    label1.Text = "Extracting the PSARC...";
                    label1.Refresh();

                    using (FileStream fs = new FileStream(@PSARC.FileName, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = new BinaryReader(fs, new ASCIIEncoding()))
                        {
                            uint magic = reader.ReadUInt32();
                            if (magic == 1380012880)
                                isValid = true;
                        }
                    }

                    if(isValid)
                    {
                        string DestinationDir = "EXTRACTED PSARC\\" + Path.GetFileNameWithoutExtension(PSARC.FileName);
                        DestinationDir = DestinationDir.Replace(".", null);

                        if (Directory.Exists(DestinationDir) == false)
                        {
                            Directory.CreateDirectory(DestinationDir); 
                            if (ExtractPSARC(PSARC.FileName, DestinationDir))
                            {
                                MessageBox.Show("Done!", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                Process.Start(Path.GetFullPath(DestinationDir));
                            }
                            else
                            {
                                //MessageBox.Show("PSARC Extraction failed", "Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }                               
                        }
                        else
                        {
                            DialogResult risultato = MessageBox.Show("There is already a folder containing an extracted \"" + Path.GetFileNameWithoutExtension(PSARC.FileName) + "\" PSARC archive. If you choose to continue, all conflicting files in that folder will be overwritten with the ones that will be now extracted.", "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                            if (risultato == DialogResult.Yes)
                            {
                                if (ExtractPSARC(PSARC.FileName, DestinationDir))
                                {
                                    MessageBox.Show("Done!", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    Process.Start(Path.GetFullPath(DestinationDir));
                                }                                  
                                else
                                {
                                    //MessageBox.Show("Something went wrong... Try again.", "Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                    
                            }
                            else
                            {
                                MessageBox.Show("Operation aborted!", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("The file provided is not a valid PSARC!", "Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                label1.Text = "Ready!";
            }           
        }

        private bool ExtractPSARC(string PSARC, string destinationDir)
        {           
            string CodeLine = "extract -y \"" + PSARC + "\" --to=\"" + Path.GetFullPath(destinationDir) + "\"";
            CodeLine = CodeLine.Replace("\\", "/");

            string path = "Resources\\psp2psarc.exe";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                FileName = path,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = CodeLine
            };

            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                    exeProcess.WaitForExit();               
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("Something went wrong while trying to run the process... Be sure that psp2psarc.exe is inside the Resources folder", "Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void CreatePSARC(string Directories, string name, string destinationDir, string strip)
        {
            string CodeLine = "-y " + Directories + " --strip=\"" + strip + "\" --output=\"" + Path.GetFullPath(destinationDir) + name + "\"";
            CodeLine = CodeLine.Replace("\\", "/");

            string path = "Resources\\psp2psarc.exe";
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                FileName = path,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = CodeLine
            };

            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                    exeProcess.WaitForExit();
                MessageBox.Show("Done!", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Process.Start(destinationDir);
            }
            catch (Exception)
            {
                MessageBox.Show("Something went wrong while trying to run the process... Be sure that psp2psarc.exe is inside the Resources folder", "Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            player.Stop();
            button2.Enabled = false;
            button3.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            player.PlayLooping();
            button3.Enabled = false;
            button2.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string DestinationDir = "REPACKED PSARC\\";

            DestinationDir = DestinationDir.Replace(".", null);

            if (Directory.Exists(DestinationDir) == false)
            {
                Directory.CreateDirectory(DestinationDir);
            }

            using (var PSARC = new FolderBrowserDialog())
            {
                PSARC.Description = "All the files contained inside the folder selected will be repacked inside a PSARC Archive.";
                PSARC.SelectedPath = Path.GetFullPath("EXTRACTED PSARC\\");
                PSARC.ShowNewFolderButton = false;
                DialogResult result = PSARC.ShowDialog();

                if(result == DialogResult.OK && !string.IsNullOrWhiteSpace(PSARC.SelectedPath))
                {
                    string[] files = Directory.GetFileSystemEntries(PSARC.SelectedPath);

                    label1.Text = "Creating the PSARC..."; // Change "Ready!" to "Wait..."
                    label1.Refresh();

                    string codeLine = "";
                    for(int i = 0; i < files.Length; i++)
                    {
                        codeLine = codeLine + "\"" + files[i] + "\" ";
                    }

                    string psarcName = Path.GetFileName(PSARC.SelectedPath) + ".psarc";

                    if (File.Exists(Path.Combine(DestinationDir, psarcName)))
                    {
                        DialogResult risultato = MessageBox.Show("There is already a PSARC Archive \"" + psarcName + "\" inside the REPACKED PSARC folder. If you choose to continue, the older file will be overwritten with the new one.", "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (risultato == DialogResult.Yes)
                            CreatePSARC(codeLine, psarcName, DestinationDir, PSARC.SelectedPath);                       
                        else
                            MessageBox.Show("Operation aborted!", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        CreatePSARC(codeLine, psarcName, DestinationDir, PSARC.SelectedPath);
                    }
                }
                label1.Text = "Ready!";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog CTXR = new OpenFileDialog())
            {
                CTXR.Title = "Select a PSASBR CTXR file";
                CTXR.Filter = "CTXR Texture File|*.ctxr";
                CTXR.Multiselect = true;

                if (CTXR.ShowDialog() == DialogResult.OK)
                {
                    label1.Text = "Analysing textures...";
                    label1.Refresh();

                    foreach(string SingleTexture in CTXR.FileNames)
                    {
                        try
                        {
                            using (FileStream fs = new FileStream(@SingleTexture, FileMode.Open, FileAccess.Read))
                            {
                                using (var reader = new BinaryReader(fs, new ASCIIEncoding()))
                                {
                                    bool isValid = false;
                                    long length = fs.Length;
                                    byte[] buffer = new byte[4];
                                    string analisi = "File: " + Path.GetFileName(SingleTexture) + "\n";
                                    long finalpos = 0;
                                    long count = 0;
                                    uint signature, txtrsign;
                                    ulong jfifsign;

                                    txtrsign = reader.ReadUInt32();        
                                    if(txtrsign == 1381259348) // Check if the file is a valid CTXR file
                                    {
                                        while ((isValid == false) && (reader.PeekChar() != -1)) // Search for GXT Magic and saves position for texture infos
                                        {
                                            signature = reader.ReadUInt32();
                                            if (signature == 5527623)
                                            {
                                                analisi = analisi + "Type: Texture PS Vita\n";
                                                isValid = true;
                                                finalpos = fs.Position;
                                                finalpos = (finalpos - 4) + 48;
                                                fs.Position = finalpos;

                                                // Identifies PS Vita Texture Type
                                                buffer = reader.ReadBytes(4);
                                                if (buffer[3] == 0x00)
                                                {
                                                    analisi = analisi + "Texture Type: Swizzled\n";
                                                }
                                                else if (buffer[3] == 0x40)
                                                {
                                                    analisi = analisi + "Texture Type: Cube\n";
                                                }
                                                else if (buffer[3] == 0x60)
                                                {
                                                    analisi = analisi + "Texture Type: Linear\n";
                                                }
                                                else if (buffer[3] == 0x80)
                                                {
                                                    analisi = analisi + "Texture Type: Tiled\n";
                                                }
                                                else if (buffer[3] == 0xA0)
                                                {
                                                    analisi = analisi + "Texture Type: Swizzled Arbitrary\n";
                                                }
                                                else if (buffer[3] == 0xC0)
                                                {
                                                    analisi = analisi + "Texture Type: Linear Strided\n";
                                                }
                                                else if (buffer[3] == 0xE0)
                                                {
                                                    analisi = analisi + "Texture Type: Cube Arbitrary\n";
                                                }
                                                else
                                                {
                                                    analisi = analisi + "Texture Type: Unknown\n";
                                                }

                                                // Identifies Compression
                                                buffer = reader.ReadBytes(4);
                                                if (buffer[3] == 0x85)
                                                {
                                                    analisi = analisi + "Texture Format: UBC1 (DXT1)\n";
                                                }
                                                else if (buffer[3] == 0x86)
                                                {
                                                    analisi = analisi + "Texture Format: UBC2 (DXT3)\n";
                                                }
                                                else if (buffer[3] == 0x87)
                                                {
                                                    analisi = analisi + "Texture Format: UBC3 (DXT5)\n";
                                                }
                                                else if (buffer[3] == 0x0C && buffer[1] == 0x10)
                                                {
                                                    analisi = analisi + "Texture Format: A8R8G8B8\n";
                                                }
                                                else if (buffer[3] == 0x0C && buffer[1] == 0x50)
                                                {
                                                    analisi = analisi + "Texture Format: X8U8U8U8_1RGB\n";
                                                }
                                                else if (buffer[3] == 0x8B)
                                                {
                                                    analisi = analisi + "Texture Format: SBC5 (Signed RGTc2)\n";
                                                }
                                                else if (buffer[3] == 0x8A)
                                                {
                                                    analisi = analisi + "Texture Format: UBC5 (Unsigned RGTc2)\n";
                                                }                                               
                                                else
                                                {
                                                    analisi = analisi + "Texture Format: Unknown\n";
                                                }

                                                // Retrieves resolution and mipmaps
                                                ushort width = reader.ReadUInt16();
                                                ushort height = reader.ReadUInt16();
                                                ushort mipmaps = reader.ReadUInt16();
                                                analisi = analisi + "Resolution: " + width + "x" + height + "\n";
                                                if (mipmaps != 1)
                                                {
                                                    analisi = analisi + "MIP maps: " + mipmaps;
                                                }
                                                else
                                                {
                                                    analisi = analisi + "MIP maps: No";
                                                }

                                                // Shows analysis results
                                                MessageBox.Show(analisi, "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            }
                                            else if (signature == 3774863615) // Eventually checks for JFIF
                                            {
                                                jfifsign = reader.ReadUInt64();
                                                if (jfifsign == 72134874563743744)
                                                {
                                                    // If JFIF found, stops while and outputs result
                                                    isValid = true;
                                                    analisi = analisi + "Type: Image\nFormat: JFIF";
                                                    MessageBox.Show(analisi, "Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                                }
                                            }
                                            count++;
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("The file " + Path.GetFileName(SingleTexture) + " is not valid!", "Bad CTXR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            if(ex is IOException)
                            {
                                string error = "Access to " + Path.GetFileName(SingleTexture) + " denied. Be sure the file isn't opened by some other program and that the tool has got enough privileges to open it.";
                                MessageBox.Show(error, "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }else if(ex is IndexOutOfRangeException)
                            {
                                string error = "Unable to load the" + Path.GetFileName(SingleTexture) + " file. No known structure detected.\nMight be an invalid CTXR file or might not be a PSASBR PS Vita file.";
                                MessageBox.Show(error, "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }         
                        }
                    }
                }
            }
            label1.Text = "Ready!";
        }

        // Repack CTXR Button 
        private void button6_Click(object sender, EventArgs e)
        {
            string DestinationDir = "REPACKED TEXTURES\\";

            DestinationDir = DestinationDir.Replace(".", null);

            if (Directory.Exists(DestinationDir) == false)
            {
                Directory.CreateDirectory(DestinationDir);
            }

            uint txtrHeader;
            byte[] txtrData = null;
            long finalpos;
            bool jfif = false;
            bool loaded = false;
            bool isValid = false;
            string filename = null;

            using (OpenFileDialog CTXR = new OpenFileDialog())
            {
                CTXR.Title = "Select a working CTXR file containing the texture you want to repack";
                CTXR.Filter = "PSASBR Texture File|*.ctxr";
                CTXR.Multiselect = false;
                //CTXR.InitialDirectory = Directory.GetCurrentDirectory();

                if (CTXR.ShowDialog() == DialogResult.OK)
                {
                    loaded = true;
                    label1.Text = "Repacking texture...";
                    label1.Refresh();                   

                    try
                    {
                        using (FileStream fs = new FileStream(@CTXR.FileName, FileMode.Open, FileAccess.Read))
                        {
                            using (var reader = new BinaryReader(fs, new ASCIIEncoding()))
                            {                                                       
                                long length = fs.Length;                     
                                uint signature;
                                ulong jfifsign;

                                txtrHeader = reader.ReadUInt32();
                                if (txtrHeader == 1381259348) // Check if the file is a valid CTXR file
                                {
                                    while ((isValid == false) && (reader.PeekChar() != -1)) // Search for GXT Magic and saves TXTR data
                                    {
                                        signature = reader.ReadUInt32();
                                        if (signature == 5527623)
                                        {
                                            isValid = true;
                                            finalpos = fs.Position;
                                            finalpos = finalpos - 4;
                                            fs.Position = 0;

                                            txtrData = reader.ReadBytes((int)finalpos);                                        
                                        }
                                        else if (signature == 3774863615) // Eventually checks for JFIF
                                        {
                                            jfifsign = reader.ReadUInt64();
                                            if (jfifsign == 72134874563743744)
                                            {
                                                // If JFIF found, saves its TXTR data
                                                isValid = true;
                                                jfif = true;
                                                finalpos = fs.Position;
                                                finalpos = finalpos - 12;
                                                fs.Position = 0;

                                                txtrData = reader.ReadBytes((int)finalpos);
                                            }
                                        }                                       
                                    }
                                    filename = CTXR.FileName;
                                }
                                else
                                {
                                    MessageBox.Show("The file " + Path.GetFileName(CTXR.FileName) + " is not valid!", "Bad CTXR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is IOException)
                        {
                            string error = "Access to " + Path.GetFileName(CTXR.FileName) + " denied. Be sure the file isn't opened by some other program and that the tool has got enough privileges to open it.";
                            MessageBox.Show(error, "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else if (ex is IndexOutOfRangeException)
                        {
                            string error = "Unable to load the" + Path.GetFileName(CTXR.FileName) + " file. No known structure detected.\nMight be an invalid CTXR file or might not be a PSASBR PS Vita file.";
                            MessageBox.Show(error, "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }

                // Another dialog to choose the texture to repack
                CTXR.Title = "Select the source image you want to repack into CTXR";
                if (jfif != true)
                {
                    CTXR.Filter = "Texture Files (*.dds;*.gxt)|*.dds;*.gxt";
                }
                else
                {
                    CTXR.Filter = "JFIF Image (*.jfif)|*.jfif";
                }   
                CTXR.Multiselect = false;

                if(loaded == true && isValid == true)
                {
                    if (CTXR.ShowDialog() == DialogResult.OK)
                    {
                        byte[] txtrSource = null;

                        if (jfif != true && Path.GetExtension(CTXR.FileName) != ".gxt")
                        {
                            txtrSource = CreateTXTR(CTXR.FileName);
                        }
                        else
                        {
                            txtrSource = File.ReadAllBytes(CTXR.FileName);
                        }

                        if (txtrSource != null)
                        {
                            try
                            {
                                byte[] finalTxtr = new byte[txtrData.Length + txtrSource.Length];
                                Buffer.BlockCopy(txtrData, 0, finalTxtr, 0, txtrData.Length);
                                Buffer.BlockCopy(txtrSource, 0, finalTxtr, txtrData.Length, txtrSource.Length);
                                File.WriteAllBytes(Path.Combine(Path.GetFullPath(DestinationDir), Path.GetFileNameWithoutExtension(filename) + ".ctxr"), finalTxtr);
                                MessageBox.Show("Done!", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                Process.Start(DestinationDir);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.ToString(), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Operation failed...", "Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            label1.Text = "Ready!";
        }

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

            string txtrPath = Path.Combine(Path.GetTempPath(), txtrName);
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
            catch (Exception)
            {
                MessageBox.Show("Something went wrong while trying to run the process... Be sure that psp2gxt.exe is inside the Resources folder. Without it, the tool can't handle DDS files for repack, only GXT files.", "Status", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string DestinationDir = "GXT\\";

            DestinationDir = DestinationDir.Replace(".", null);

            if (Directory.Exists(DestinationDir) == false)
            {
                Directory.CreateDirectory(DestinationDir);
            }

            uint txtrHeader;
            byte[] txtrData = null;
            long finalpos;
            bool isValid = false;

            using (OpenFileDialog CTXR = new OpenFileDialog())
            {
                CTXR.Title = "Choose the source CTXR to convert into GXT";
                CTXR.Filter = "CTXR Texture File|*.ctxr";
                CTXR.Multiselect = true;
                //CTXR.InitialDirectory = Directory.GetCurrentDirectory();

                if (CTXR.ShowDialog() == DialogResult.OK)
                {
                    label1.Text = "Converting to GXT...";
                    label1.Refresh();

                    foreach (string SingleTexture in CTXR.FileNames)
                    {
                        try
                        {
                            using (FileStream fs = new FileStream(@SingleTexture, FileMode.Open, FileAccess.Read))
                            {
                                using (var reader = new BinaryReader(fs, new ASCIIEncoding()))
                                {
                                    long length = fs.Length;
                                    byte[] buffer = new byte[4];
                                    uint signature;
                                    ulong jfifsign;

                                    txtrHeader = reader.ReadUInt32();
                                    if (txtrHeader == 1381259348) // Check if the file is a valid CTXR file
                                    {
                                        while ((isValid == false) && (reader.PeekChar() != -1)) // Search for GXT Magic and removes TXTR data
                                        {
                                            signature = reader.ReadUInt32();
                                            Console.WriteLine(signature);
                                            if (signature == 5527623)
                                            {                                         
                                                isValid = true;
                                                finalpos = fs.Position;
                                                finalpos = length - (finalpos - 4);
                                                fs.Position = fs.Position - 4;

                                                txtrData = reader.ReadBytes((int)finalpos);
                                                try
                                                {
                                                    File.WriteAllBytes(Path.Combine(Path.GetFullPath(DestinationDir), Path.GetFileNameWithoutExtension(SingleTexture) + ".gxt"), txtrData);               
                                                }
                                                catch (Exception ex)
                                                {
                                                    MessageBox.Show(ex.ToString(), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                            }
                                            else if (signature == 3774863615) // Eventually checks for JFIF
                                            {
                                                jfifsign = reader.ReadUInt64();
                                                if (jfifsign == 72134874563743744)
                                                {
                                                    // If JFIF found, notifies that it's not possible to create a GXT out of it
                                                    isValid = true;
                                                    MessageBox.Show("The CTXR file " + SingleTexture + " cannot be converted to a PS Vita Texture file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                            }
                                        }
                                        isValid = false;
                                    }
                                    else
                                    {
                                        MessageBox.Show("The file " + Path.GetFileName(SingleTexture) + " is not valid!", "Bad CTXR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex is IOException)
                            {
                                string error = "Access to " + Path.GetFileName(SingleTexture) + " denied. Be sure the file isn't opened by some other program and that the tool has got enough privileges to open it.";
                                MessageBox.Show(error, "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else if (ex is IndexOutOfRangeException)
                            {
                                string error = "Unable to load the" + Path.GetFileName(SingleTexture) + " file. No known structure detected.\nMight be an invalid CTXR file or might not be a PSASBR PS Vita file.";
                                MessageBox.Show(error, "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    MessageBox.Show("Done!", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Process.Start(DestinationDir);
                }
            }          
            label1.Text = "Ready!";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(e.Link.LinkData as string);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string DestinationDir = "GXT\\";
            string FinalDir = "EXTRACTED TEXTURES\\";

            DestinationDir = DestinationDir.Replace(".", null);
            FinalDir = FinalDir.Replace(".", null);

            if (Directory.Exists(DestinationDir) == false)
            {
                Directory.CreateDirectory(DestinationDir);
            }

            if (Directory.Exists(FinalDir) == false)
            {
                Directory.CreateDirectory(FinalDir);
            }

            uint txtrHeader;
            byte[] txtrData = null;
            long finalpos;
            bool isValid = false;

            using (OpenFileDialog CTXR = new OpenFileDialog())
            {
                CTXR.Title = "Choose the source CTXR to convert into PNG";
                CTXR.Filter = "CTXR Texture File|*.ctxr";
                CTXR.Multiselect = true;
                //CTXR.InitialDirectory = Directory.GetCurrentDirectory();

                if (CTXR.ShowDialog() == DialogResult.OK)
                {
                    label1.Text = "Extracting textures...";
                    label1.Refresh();

                    foreach (string SingleTexture in CTXR.FileNames)
                    {
                        try
                        {
                            using (FileStream fs = new FileStream(@SingleTexture, FileMode.Open, FileAccess.Read))
                            {
                                using (var reader = new BinaryReader(fs, new ASCIIEncoding()))
                                {
                                    long length = fs.Length;
                                    uint signature;
                                    ulong jfifsign;

                                    txtrHeader = reader.ReadUInt32();
                                    if (txtrHeader == 1381259348) // Check if the file is a valid CTXR file
                                    {
                                        while ((isValid == false) && (reader.PeekChar() != -1)) // Search for GXT Magic and removes TXTR data
                                        {
                                            signature = reader.ReadUInt32();
                                            Console.WriteLine(signature);
                                            if (signature == 5527623)
                                            {
                                                isValid = true;
                                                finalpos = fs.Position;
                                                finalpos = length - (finalpos - 4);
                                                fs.Position = fs.Position - 4;

                                                txtrData = reader.ReadBytes((int)finalpos);
                                                try
                                                {
                                                    File.WriteAllBytes(Path.Combine(Path.GetFullPath(DestinationDir), Path.GetFileNameWithoutExtension(SingleTexture) + ".gxt"), txtrData);
                                                }
                                                catch (Exception ex)
                                                {
                                                    MessageBox.Show(ex.ToString(), "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                            }
                                            else if (signature == 3774863615) // Eventually checks for JFIF
                                            {
                                                jfifsign = reader.ReadUInt64();
                                                if (jfifsign == 72134874563743744)
                                                {
                                                    // If JFIF found, notifies that it's not possible to create a GXT out of it
                                                    isValid = true;
                                                    MessageBox.Show("The CTXR file " + SingleTexture + " cannot be converted to a PS Vita Texture file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                }
                                            }
                                        }
                                        isValid = false;
                                    }
                                    else
                                    {
                                        MessageBox.Show("The file " + Path.GetFileName(SingleTexture) + " is not valid!", "Bad CTXR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex is IOException)
                            {
                                string error = "Access to " + Path.GetFileName(SingleTexture) + " denied. Be sure the file isn't opened by some other program and that the tool has got enough privileges to open it.";
                                MessageBox.Show(error, "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                            else if (ex is IndexOutOfRangeException)
                            {
                                string error = "Unable to load the" + Path.GetFileName(SingleTexture) + " file. No known structure detected.\nMight be an invalid CTXR file or might not be a PSASBR PS Vita file.";
                                MessageBox.Show(error, "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }

                    label1.Text = "Converting to PNG...";
                    label1.Refresh();
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        CreateNoWindow = true,
                        FileName = "Resources\\ExtractTextures.bat",
                        WindowStyle = ProcessWindowStyle.Hidden,
                    };

                    try
                    {
                        using (Process exeProcess = Process.Start(startInfo))
                            exeProcess.WaitForExit();
                        MessageBox.Show("Done!", "Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Process.Start(FinalDir);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("An error has occurred. Be sure that the Scarlet components are in the \"Resources\" folder: they are included with the PSASBR Tool release.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    label1.Text = "Ready!"; 
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            MessageBox.Show("PSASBR Tool v1.0\n© 2019 Cri4Key\n\nThis software is OPEN SOURCE and licensed under the MIT license.");
        }
    }
}
