using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOD_COM
{
    public class MTL
    {
        [DebuggerDisplay("file_hash = {file_hash} file_path = {file_path} ")]
        public class TEX_ITEM
        {
            public UInt64 file_hash;
            public string file_path;

            public TEX_ITEM(UInt64 in_hash, string in_path)
            {
                file_hash = in_hash;
                file_path = in_path;
            }

        }

        //Data
        private string in_folderpath;
        private string out_filepath;
        private List<TEX_ITEM> masterList = new List<TEX_ITEM>();

        //Events
        public delegate void FMDLConsolePrint(string message, Color col);
        public event FMDLConsolePrint RaiseFMDLConsolePrint;

        public List<TEX_ITEM> Elements
        {
            get
            {
                return masterList;
            }
        }

        public int ElementCount
        {
            get
            {
                return masterList.Count;
            }
        }


        public MTL()
        {

        }


        public bool GenerateSave(string main_folder)
        {
            in_folderpath = main_folder;
            out_filepath = Path.Combine(in_folderpath, "ftex_core.mtl");

            ConsolePrint("Building filelist, this may take a while just wait.", Color.Pink);

            //Find all the ini files in the path (including subpath)
            string[] files = System.IO.Directory.GetFiles(in_folderpath, "*.inf");
            for (int f = 0; f < files.Length; f++)
            {
                string curFile = files[f];
                ConsolePrint("Parsing: " + curFile, Color.Cyan);


                string[] lines = File.ReadAllLines(curFile);
                for (int x = 3; x < lines.Length; x++)
                {
                    string line = lines[x];
                    char[] delimiters = new char[] { '|', ' ' };
                    string[] segA = line.Split(delimiters, StringSplitOptions.None);
                    for (int i = 0; i < segA.Length; i++)
                    {
                        //if file extension isn't .ftex ignore it!
                        if (Path.GetExtension(segA[1]) != ".ftex")
                            continue;


                        //mk object son and add to list
                        TEX_ITEM t = new TEX_ITEM(Convert.ToUInt64(segA[0], 16), segA[1]);
                        masterList.Add(t);
                    }
                } //for each line
            } //for files in dir

            ConsolePrint("Saving masterfile: " + Path.GetFileName(out_filepath), Color.Pink);

            using (FileStream fs = new FileStream(out_filepath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(Encoding.ASCII.GetBytes("FMTL"));
                bw.Write(masterList.Count);
                for (int j = 0; j < masterList.Count; j++)
                {
                    bw.Write(masterList[j].file_hash);

                    byte[] strData = Encoding.ASCII.GetBytes(masterList[j].file_path);
                    bw.Write(strData.Length);
                    bw.Write(strData);
                }
            }
            return true;
        }

        public bool Load(string path2file)
        {
            using(FileStream fs = new FileStream(path2file, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                string magic = Encoding.ASCII.GetString(br.ReadBytes(4));
                if (magic != "FMTL")
                    return false;

                int entry_count = br.ReadInt32();
                if (entry_count == 0)
                    return false;

                for (int i = 0; i < entry_count; i++)
                {
                    UInt64 hash = br.ReadUInt64();
                    Int32 path_length = br.ReadInt32();
                    string path = Encoding.ASCII.GetString(br.ReadBytes(path_length));
                    masterList.Add(new TEX_ITEM(hash, path));
                }
            }

            return true;
        }

        private void ConsolePrint(string msg)
        {
            RaiseFMDLConsolePrint(msg, Color.White);
        }

        private void ConsolePrint(string msg, Color col)
        {
            RaiseFMDLConsolePrint(msg, col);
        }



    }
}
