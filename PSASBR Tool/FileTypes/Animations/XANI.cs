using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PSASBR_Tool.FileTypes
{
    class XANI
    {
        private XANI_Header header;
        private CANI_Info[] info;
        private List<byte[]> caniFiles; // Use until we figure out how CANI works

        public XANI(string path)
        {
            using (FileStream fs = new FileStream(@path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = new BinaryReader(fs, new ASCIIEncoding()))
                {
                    uint numAnim = reader.ReadUInt32();
                    if (!(numAnim > 0))
                        throw new ArgumentException("Provided XANI is an empty animation archive.");
                    uint xaniSize = reader.ReadUInt32();
                    uint bundleOffset = reader.ReadUInt32();
                    uint bundleSize = reader.ReadUInt32();
                    header = new XANI_Header(numAnim, xaniSize, bundleOffset, bundleSize);

                    info = new CANI_Info[numAnim];
                    for (int i = 0; i < header.numCANI; i++)
                    {
                        uint infoSize = reader.ReadUInt32();
                        uint offset = reader.ReadUInt32();
                        uint size = reader.ReadUInt32();
                        
                        char[] nameBuffer = new char[infoSize - 0xC];
                        int j;
                        for (j = 0; j < nameBuffer.Length && reader.PeekChar() != 0; j++)
                        {
                            nameBuffer[j] = reader.ReadChar();
                        }
                        string fullName = new string(nameBuffer, 0, j);
                        int paddingLength = nameBuffer.Length - j;
                        fs.Seek(paddingLength, SeekOrigin.Current);

                        info[i] = new CANI_Info(infoSize, offset, size, fullName, paddingLength);
                    }

                    caniFiles = new List<byte[]>((int)header.numCANI);

                    for (int i = 0; i < header.numCANI; i++)
                    {
                        fs.Seek(header.bundleOffset, SeekOrigin.Begin);
                        fs.Seek(info[i].caniOffset, SeekOrigin.Current);
                        caniFiles.Add(reader.ReadBytes((int)info[i].caniSize));
                    }
                }
            }
        }

        private class XANI_Header
        {
            public uint numCANI;
            public uint xaniSize;
            public uint bundleOffset;
            public uint bundleSize;

            public XANI_Header(uint numCANI, uint xaniSize, uint bundleOffset, uint bundleSize)
            {
                this.numCANI = numCANI;
                this.xaniSize = xaniSize;
                this.bundleOffset = bundleOffset;
                this.bundleSize = bundleSize;
            }
        }

        private class CANI_Info
        {
            public uint caniInfoSize;
            public uint caniOffset;
            public uint caniSize;
            public string caniFullName;
            public int padding_len;

            public CANI_Info(uint caniInfoSize, uint caniOffset, uint caniSize, string caniFullName, int paddingLength)
            {
                this.caniInfoSize = caniInfoSize;
                this.caniOffset = caniOffset;
                this.caniSize = caniSize;
                this.caniFullName = caniFullName;
                this.padding_len = paddingLength;
            }
        }

        public void ExtractXANI(string destinationDir)
        {
            
            for (int i = 0; i < header.numCANI; i++)
            {
                string animName = info[i].caniFullName.TrimStart(new char[] { '$', '/' });
                animName = animName.Replace('/', '\\');
                string FinalDir = Path.Combine(Path.GetFullPath(destinationDir), animName);
                Directory.CreateDirectory(Path.GetDirectoryName(FinalDir));
                //Console.WriteLine(FinalDir);
                File.WriteAllBytes(FinalDir, caniFiles[i]);
            }
        }
    }
}
