//#define no_optimize
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using Cra0Framework.Core;
using Cra0Framework.Core.Graphics;
using Cra0Framework.Core.FileFormats;

using MOD_COM;

//typedef in c#
using asciiz = System.String;

using int8 = System.SByte;
using int16 = System.Int16;
using int32 = System.Int32;
using int64 = System.Int64;

using uint8 = System.Byte;
using uint16 = System.UInt16;
using uint32 = System.UInt32;
using uint64 = System.UInt64;

namespace FoxLib
{
    public partial class FMDL
    {

        //Data members
        IO.ByteOrder endian;
        FileStream fs;
        BinaryReader br;
        BinaryWriter bw;

        //GLOBAL DATA
        List<fMesh> GLOBAL_MESHLIST = new List<fMesh>();
        List<fMesh> OPTIMIZED_MESHLIST = new List<fMesh>();


        //flags
        public string texdirFolderpath = String.Empty;
        public const string masterfile_name = "ftex_core.mtl";
        public MTL masterTextureList;
        public bool loaded;

        private bool merge_meshes = false;

        //FMDL Data
        fmdl_head head;

        List<foxEntry> FoxBlockAList = new List<foxEntry>();
        List<foxEntry2> FoxBlockBList = new List<foxEntry2>();

        //Const <base from where you offset>
        uint32 FOXA_OFFSET;
        uint32 FOXB_OFFSET;


        List<foxBone> FoxBoneList = new List<foxBone>();
        List<foxMeshGroup> FoxMeshGroupList = new List<foxMeshGroup>();
        List<foxMeshGroupInfo> FoxMeshGroupInfoList = new List<foxMeshGroupInfo>();
        List<foxMeshInfo> FoxMeshInfoList = new List<foxMeshInfo>();

        List<foxTexture> FoxTextureList = new List<foxTexture>();
        List<foxMaterial> FoxMaterialList = new List<foxMaterial>();

        List<foxDataFormatInfo> FoxDataFrmtList = new List<foxDataFormatInfo>();

        List<foxUnkFloat> FoxUnkFloatList = new List<foxUnkFloat>();

        List<foxBoneLookup> FoxBoneLookupList = new List<foxBoneLookup>();

        List<foxPreVertIndexTable> FoxPreVertDefList = new List<foxPreVertIndexTable>();
        List<foxTextureHashTable> FoxTexHashList = new List<foxTextureHashTable>();

        List<foxString> FoxStringList = new List<foxString>();


        //->Debug
        List<foxMaterialGroup> fmats = new List<foxMaterialGroup>();


        //Events
        public delegate void FMDLConsolePrint(string message, Color col);
        public event FMDLConsolePrint RaiseFMDLConsolePrint;

        public delegate void FMDLLoaded(bool result, foxModelDetails info);
        public event FMDLLoaded RaiseFMDLLoaded;

        public delegate void FMDLUnloaded();
        public event FMDLUnloaded RaiseFMDLUnloaded;

        public delegate void FMDLLoading();
        public event FMDLLoading RaiseFMDLLoading;

        public delegate void FMDLErrorLoading(ErrorEvents error, string message);
        public event FMDLErrorLoading RaiseFMDLErrorLoading;


        public FMDL()
        {

            //little endian
            endian = IO.ByteOrder.LittleEndian;


        }

        public FMDL(string filepath)
        {
            fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            br = new BinaryReader(fs);

            //little endian
            endian = IO.ByteOrder.LittleEndian;
        }


        public FMDL(FileStream ifs)
        {
            this.fs = ifs;

            //little endian
            endian = IO.ByteOrder.LittleEndian;
        }


        //public methods
        public bool CheckFile()
        {
            //check FMDL magic
            head.magic = IO.ReadStringABSLen(br, 4);

            if (head.magic != "FMDL")
            {
                this.Dispose();
                return false;
            }

            return true;
        }

        public void SetTextureDir(string folderpath)
        {
            this.texdirFolderpath = folderpath;
        }


        public void ParseFile(bool option_merge)
        {
            merge_meshes = option_merge;

            RaiseFMDLLoading();
            //if texture dir is set try and load the mtl
            if (texdirFolderpath != string.Empty)
            {
                string full_path = Path.Combine(texdirFolderpath, masterfile_name);
                if (File.Exists(full_path))
                {
                    ConsolePrint("Loading in Master Texture List", Color.Yellow);

                    masterTextureList = new MTL();
                    bool zresult = masterTextureList.Load(full_path);
                    if (zresult == true)
                    {
                        ConsolePrint("Loaded Master Texture List", Color.LimeGreen);
                    }
                    else
                    {
                        ConsolePrint("Failed loading Master Texture List", Color.Red);
                    }
                }
            }




            //Parse data into our common containers
            bool result = true;
            result = this.ParseHeader(result);
            result = this.ParseFoxBlockA(result);
            result = this.ParseFoxBlockB(result);
            result = this.ParseElements(result); //load entries into memory
            result = this.PostProcess(result); // post process

            //  result = this.DebugExport(result); //try and export mesh data
            // return result;
            loaded = true;
            RaiseFMDLLoaded(result, new foxModelDetails(this.FoxBoneList.Count,0,0,-1,this.FoxMaterialList.Count, this.FoxMeshInfoList.Count));
        }

        public bool ExportMesh()
        {
            return false;
        }


        public bool ExportMesh(string outputPath,ExportFormat fmt, ExportOptions exportOptions)
        {

            //Giant switch statement for each format
            //pass the format classes the export options needed along with the path n name


#if no_optimize
            for (int i = 0; i < GLOBAL_MESHLIST.Count; i++)
            {
                //get
                string groupFolder = GLOBAL_MESHLIST[i].groupFolder;
                Utilz.CreatePath(groupFolder);
                foxMeshGroupInfo group = GLOBAL_MESHLIST[i].group;
                List<cMesh> meshList = GLOBAL_MESHLIST[i].mshList;
#else
            for (int i = 0; i < OPTIMIZED_MESHLIST.Count; i++)
            {
                //get
                string groupFolder = OPTIMIZED_MESHLIST[i].groupFolder;
                string finalPath = Path.Combine(outputPath, groupFolder);


                Utilz.CreatePath(finalPath);
                foxMeshGroupInfo group = OPTIMIZED_MESHLIST[i].group;
                List<cMesh> meshList = OPTIMIZED_MESHLIST[i].mshList;
#endif

                for (int j = 0; j < meshList.Count; j++)
                {

                    cMesh curMesh = meshList[j];

                    switch (fmt)
                    {
                    case ExportFormat.OBJ:

                        OBJ curOBJ = new OBJ(curMesh, false);
                        curOBJ.saveOBJ(Path.Combine(finalPath, "mesh_" + curMesh.idx + ".obj"), true);

                        break;
                    case ExportFormat.PLY:

                        PLY curPLY = new PLY(curMesh);
                        curPLY.Save(Path.Combine(finalPath, "mesh_" + curMesh.idx + ".ply"));

                        break;
                    case ExportFormat.SMD:

                        SMD curSMD = new SMD(curMesh, true, true);
                        curSMD.saveSMD(Path.Combine(finalPath, "mesh_" + curMesh.idx + ".smd"));

                        break;
                    case ExportFormat.BMD:

                        BMD curBMD = new BMD(curMesh, true);
                        curBMD.SaveBMD(Path.Combine(finalPath, "mesh_" + curMesh.idx + ".bmd"));

                        break;
                    }

                }

                //Textures
                if(masterTextureList != null)
                    this.stub_CopyUsedTextures(outputPath);


            }

            ConsolePrint("Exported!", Color.BlueViolet);



            return true;
        }

        public void Dispose()
        {
            if (br != null)
            {
                br.Close();
            }

            if (bw != null)
            {
                bw.Flush();
                bw.Close();
            }

            //Binary reader closes fs no need to close

        }

        public void Unload()
        {
            loaded = false;

            //dispose first
            Dispose();
            //Raise Unload
            RaiseFMDLUnloaded();

        }


        //private
        private bool ParseHeader(bool prev)
        {
            if (prev != true)
            {
                ConsolePrint("Fault detected ParseHeader() Skipped!",Color.Yellow);
                return false;
            }


            //read rest of header into structures
            head.uvar_aS = IO.ReadUInt16(br, endian);
            head.uvar_bS = IO.ReadUInt16(br, endian);
            head.uvar_c = IO.ReadUInt32(br, endian);
            head.uvar_zero = IO.ReadUInt32(br, endian);
            head.uvar_d_a = IO.ReadUInt16(br, endian);
            head.uvar_d_b = IO.ReadUInt16(br, endian);
            head.uvar_e = IO.ReadUInt32(br, endian);
            head.uvar_f = IO.ReadUInt32(br, endian);
            head.uvar_g = IO.ReadUInt32(br, endian);
            head.foxEntryACount = IO.ReadUInt32(br, endian); //tb 1
            head.foxEntryBCount = IO.ReadUInt32(br, endian); //tb 2
            head.startofMDLData = IO.ReadUInt32(br, endian);
            head.lengtofMDLData = IO.ReadUInt32(br, endian);
            head.startofVertexFaceData = IO.ReadUInt32(br, endian);
            head.lengtofVertexFaceData = IO.ReadUInt32(br, endian);
            head.uvar_j = IO.ReadUInt32(br, endian);
            head.uvar_k = IO.ReadUInt32(br, endian);


            return true;
        }

        private bool ParseFoxBlockA(bool prev)
        {
            if (prev != true)
            {
                ConsolePrint("Fault detected ParseFoxBlockA() Skipped!", Color.Yellow);
                return false;
            }

            ConsolePrint("Loading FoxEntriesA", Color.Pink);

            //Seek ?
            for (int i = 0; i < head.foxEntryACount; i++)
            {
                foxEntry curEntry = new foxEntry();
                curEntry.entryId = getType(IO.ReadUInt16(br, endian));
                curEntry.entryBlockCount = IO.ReadUInt16(br, endian);
                curEntry.entryOffset = IO.ReadUInt32(br, endian);

                //append
                FoxBlockAList.Add(curEntry);
            }


            return true;
        }

        private bool ParseFoxBlockB(bool prev)
        {
            if (prev != true)
            {
                ConsolePrint("Fault detected ParseFoxBlockB() Skipped!", Color.Yellow);
                return false;
            }

            ConsolePrint("Loading FoxEntriesB", Color.Pink);


            //Seek ?
            for (int i = 0; i < head.foxEntryBCount; i++)
            {
                foxEntry2 curEntry = new foxEntry2();
                curEntry.ent_id = getType2(IO.ReadInt32(br, endian));
                curEntry.ent_offset = IO.ReadUInt32(br, endian);
                curEntry.ent_length = IO.ReadUInt32(br, endian);

                //append
                FoxBlockBList.Add(curEntry);
            }


            return true;
        }


        private bool ParseElements(bool prev)
        {
            if (prev != true)
            {
                ConsolePrint("Fault detected ParseElements() Skipped!", Color.Yellow);
                return false;
            }


            //Set const offset positions
            FOXA_OFFSET = head.startofMDLData;
            FOXB_OFFSET = head.startofVertexFaceData;

            //MGS 5 doesn't have strings no more!
            //get strings first (other structs reference this)
            //stub_StrAttrTable();


            //get or find required entries
            stub_BoneList();              //0
            stub_MeshGroups();           //1
            stub_MeshGroupsInfo();      //2
            stub_MeshInfo();           //3

            stub_BoneLookup();

            stub_TextureHashList(); //21 (do this first before texture list)
            stub_TexList();           //4
            stub_MatList();          //5

            stub_DataFormatInfo();  //10

            stub_UnkFloatList(); // 13
            stub_PreVertIndexTable(); //14



            return true;
        }

        private bool PostProcess(bool prev)
        {
            if (prev != true)
            {
                ConsolePrint("Fault detected PostProcess() Skipped!", Color.Yellow);
                return false;
            }

            this.stub_proMeshGroups();
            this.stub_SetMat();


            this.stub_PrepMeshData(); //make fMesh

#if !no_optimize
            this.stub_CombineCOMmeshes();
#endif

            //Do we merge even more?
            if (merge_meshes)
            {
                stub_CombineMeshV2();
            }

            return true;
        }



        private bool DebugExport(bool prev)
        {
            if (prev != true)
            {
                ConsolePrint("Fault detected DebugExport() Skipped!", Color.Yellow);
                return false;
            }

            //Get our tmp offsets
            uint32 VertexBufferOffset = this.FOXB_OFFSET + this.FoxBlockBList[1].ent_offset;
            uint32 NormalBufferOffset = VertexBufferOffset + this.FoxPreVertDefList[1].vertOffset;
            uint32 FaceBufferOffset = VertexBufferOffset + this.FoxPreVertDefList[2].vertOffset;

            int format = 0;
            int format_ptr = 0;


            //Loop drawcalls
            for (int i = 0; i < this.FoxMeshInfoList.Count; i++)
            {

                foxMeshInfo curFOXMesh = FoxMeshInfoList[i];


                fs.Seek(VertexBufferOffset,SeekOrigin.Begin);

                int32 vert_count = (int)curFOXMesh.vertCount;
                int32 face_count = (int)(curFOXMesh.faceLength / 3);

                //mod mesh
                cMesh curMesh = new cMesh();
                List<cVertex> VBO = new List<cVertex>();
                List<cFace> FBO = new List<cFace>();
                List<cBone> BoneList = new List<cBone>();

                //Read verticies into vbo
                for (int vtx = 0; vtx < vert_count; vtx++)
                {

                    cVertex vert = new cVertex();
                    //vert.vertPosInBuffer = fs.Position;
                    vert.Position = IO.ReadVec3(br, endian) * 34;
                    VBO.Add(vert);
                }

                //update vert pointer
                IO.AlignStream(fs, 16);
                VertexBufferOffset = (uint)fs.Position;

                //jump to normals block
                fs.Seek(NormalBufferOffset,SeekOrigin.Begin);


                while (FoxDataFrmtList[format_ptr].BlockSize != 0x0C)
                {
                    format_ptr = format_ptr + 1;
                }

                format_ptr = format_ptr + 1; // first after
                format = FoxDataFrmtList[format_ptr].BlockSize;


                for (int nmx = 0; nmx < vert_count; nmx++)
                {
                    //Get vertex
                    cVertex vert = VBO[nmx];


                    switch (format)
                    {
                    case 0x20:
                        //blockSize 32

                        // normals - as half floats
                        vert.Normal = new Vector3(IO.ReadHalfFloat(br, endian), IO.ReadHalfFloat(br, endian), IO.ReadHalfFloat(br, endian));
                        float vertNorW = IO.ReadHalfFloat(br, endian);

                        //??
                        float deltaX2 = IO.ReadFloat(br, endian);
                        float deltaY2 = IO.ReadFloat(br, endian);

                        //Color i think
                        uint32 p1 = IO.ReadUInt32(br, endian); //col
                        //weight
                        vert.dx_weight = C3D.DecompressBoneWeights(IO.ReadByte(br));
                        vert.dx_weight2 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                        vert.dx_weight3 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                        vert.dx_weight4 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                        //links to bone
                        vert.dx_index = IO.ReadByte(br);
                        vert.dx_index2 = IO.ReadByte(br);
                        vert.dx_index3 = IO.ReadByte(br);
                        vert.dx_index4 = IO.ReadByte(br);
                        //uv
                        vert.TextureUV =  new Vector2(IO.ReadHalfFloat(br, endian),1 - (IO.ReadHalfFloat(br, endian)));



                        break;

                    case 0x1C:
                        //blockSize 28

                        // normals - as half floats
                        vert.Normal = new Vector3(IO.ReadHalfFloat(br, endian), IO.ReadHalfFloat(br, endian), IO.ReadHalfFloat(br, endian));
                        float vertNor2W = IO.ReadHalfFloat(br, endian);

                        //??
                        float delta2X2 = IO.ReadFloat(br, endian);
                        float delta2Y2 = IO.ReadFloat(br, endian);

                        //weight
                        vert.dx_weight = C3D.DecompressBoneWeights(IO.ReadByte(br));
                        vert.dx_weight2 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                        vert.dx_weight3 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                        vert.dx_weight4 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                        //links to bone
                        vert.dx_index = IO.ReadByte(br);
                        vert.dx_index2 = IO.ReadByte(br);
                        vert.dx_index3 = IO.ReadByte(br);
                        vert.dx_index4 = IO.ReadByte(br);
                        //uv
                        vert.TextureUV = new Vector2(IO.ReadHalfFloat(br, endian), 1 - (IO.ReadHalfFloat(br, endian)));

                        break;
                    case 0x14:
                        //blockSize 14

                        // normals - as half floats
                        vert.Normal = new Vector3(IO.ReadHalfFloat(br, endian), IO.ReadHalfFloat(br, endian), IO.ReadHalfFloat(br, endian));
                        float vertNor3W = IO.ReadHalfFloat(br, endian);

                        //??
                        float delta3X2 = IO.ReadFloat(br, endian);
                        float delta3Y2 = IO.ReadFloat(br, endian);


                        //uv
                        vert.TextureUV = new Vector2(IO.ReadHalfFloat(br, endian), 1 - (IO.ReadHalfFloat(br, endian)));


                        break;
                    default:
                        //blockSize Unknow
                        throw new Exception("Error BlockSize not found Engine requests size: " + format);
                    }


                }

                //update normal pointer
                IO.AlignStream(fs, 16);
                NormalBufferOffset = (uint)fs.Position;

                //seek to faces
                fs.Seek(FaceBufferOffset + (curFOXMesh.faceStart * 2), SeekOrigin.Begin);

                for (int fx = 0; fx < face_count; fx++)
                {
                    cFace curFace = new cFace();
                    curFace.vtx1 = IO.ReadInt16(br, endian);
                    curFace.vtx2 = IO.ReadInt16(br, endian);
                    curFace.vtx3 = IO.ReadInt16(br, endian);

                    curFace.Flip();     //FIX FRONT FACE CULLING

                    FBO.Add(curFace);
                }


                //bones?
                for (int b = 0; b < FoxBoneList.Count; b++)
                {
                    foxBone cBone = FoxBoneList[b];
                    BoneList.Add(new cBone((cBone.idx - 1), cBone.parent, cBone.name, new Vector3(cBone.bone_World_X * 34, cBone.bone_World_Y * 34, cBone.bone_World_Z * 34),
                                           (new Quaternion(cBone.bone_Rot_Y,cBone.bone_Rot_X, cBone.bone_Rot_Z, cBone.bone_Rot_W)), Vector3.One));
                }

                //  BoneList.Add(new MOD_BoneStruc(0, "root", -1, new Vector3(-0.107F, 0.113F, 0.0F), new Quaternion(0, 0, 0, 1), Vector3.One, 0));
                //  BoneList.Add(new MOD_BoneStruc(1, "a", -1, new Vector3(1.715F, 0F, 53.034F), new Quaternion(0, 0, 0, 1), Vector3.One, 0));
                //  BoneList.Add(new MOD_BoneStruc(1, "a", -1, new Vector3(1.715F, 0F, 77.034F), new Quaternion(0, 0, 0, 1), Vector3.One, 0));



                //collect bones + verts + faces
                curMesh.idx = curFOXMesh.mesh_idx;
                curMesh.name = "mesh_" + curMesh.idx;
                curMesh.material = FoxMeshInfoList[i].texName;
                curMesh.VertexBuffer = VBO;
                curMesh.FaceBuffer = FBO;
                curMesh.BoneList = BoneList; // LOLOL SHADOW


                string fakepathDir = Path.Combine(Path.GetDirectoryName(this.fs.Name),"_exDUMP");
                Utilz.CreatePath(fakepathDir);

                //obj anyone?
                OBJ curOBJ = new OBJ(curMesh,false);
                curOBJ.saveOBJ(Path.Combine(fakepathDir, "mesh_" + curMesh.idx + ".obj"), false);

                //smd
                SMD curSMD = new SMD(curMesh, true, false);
                curSMD.saveSMD(Path.Combine(fakepathDir, "mesh_" + curMesh.idx + ".smd"));

                int getvl = curMesh.getLastFaceVertIndex();
                int gg = getvl;


                //bmd
                // BMD_MOD curBMD = new BMD_MOD(BoneList, curMesh, false);
                //  curBMD.SaveBMD(Path.Combine(fakepathDir, "mesh_" + objectIDX + ".bmd"));

                ConsolePrint("Dumped-> " + curMesh.name);

            }


#if debu
            string fakepathDirD = Path.Combine(Path.GetDirectoryName(this.fs.Name),"_exDUMP");
            //points?
            List<Vector3> ptrs = new List<Vector3>();
            foreach (foxUnkFloat ptc in FoxUnkFloatList)
            {
                ptrs.Add(new Vector3(ptc.unkA_x, ptc.unkA_y, ptc.unkA_z));
            }

            PLY curPLY = new PLY(ptrs);
            curPLY.Save(Path.Combine(fakepathDirD, "mesh_" + 0 + ".ply"));
#endif

            return true;
        }

        #region "LoadIn"


        private bool stub_StrAttrTable()
        {
            foxEntry foxStrAttrTableEntry = this.FoxBlockAList.Find((o) => o.entryId == EntryType.STRINFOLIST);
            foxEntry2 foxStringTableEntry = this.FoxBlockBList.Find((o) => o.ent_id == EntryType2.STRINGTABLE);
            if (foxStrAttrTableEntry == null || foxStringTableEntry == null)
            {
                ConsolePrint("Skipping StringTable()", Color.Yellow);
                return true;
            }

            //Seek
            fs.Seek((FOXA_OFFSET + foxStrAttrTableEntry.entryOffset), SeekOrigin.Begin);

            //Read
            for (int i = 0; i < foxStrAttrTableEntry.entryBlockCount; i++)
            {

                foxString curString = new foxString();
                foxStrAttr strInfo;
                strInfo.fthree = IO.ReadUInt16(br, endian);
                strInfo.strLength = IO.ReadUInt16(br, endian);
                strInfo.strOffset = IO.ReadUInt32(br, endian);

                //Save pos
                int32 saveMarker = (int32)fs.Position;


                //BIND
                curString.strAttributes = strInfo;

                //Seek to stringtable
                uint32 RAW_STRTABLE_OFFSET = FOXB_OFFSET + foxStringTableEntry.ent_offset;
                fs.Seek(RAW_STRTABLE_OFFSET + strInfo.strOffset, SeekOrigin.Begin);

                //Read string
                curString.str = IO.ReadStringABSLen(br,strInfo.strLength);


                //APPEND
                FoxStringList.Add(curString);

                //Seek back
                fs.Seek(saveMarker, SeekOrigin.Begin);

            }


            ConsolePrint("->Parsed Strings", Color.LimeGreen);
            return true;
        }

        private bool stub_BoneList()
        {
            foxEntry foxBoneListEntry = this.FoxBlockAList.Find((o) => o.entryId == EntryType.BONELIST);
            if (foxBoneListEntry == null)
            {
                return true;
            }

            //Seek
            fs.Seek((FOXA_OFFSET + foxBoneListEntry.entryOffset), SeekOrigin.Begin);

            //Read
            for (int i = 0; i < foxBoneListEntry.entryBlockCount; i++)
            {
                foxBone curBone = new foxBone();
                curBone.idx = IO.ReadInt16(br, endian);
                //curBone.name = FoxStringList[curBone.idx].str;

                string recovered_name = FoxBoneMatcher.FindMatch(curBone.idx);
                if (recovered_name == string.Empty)
                    curBone.name = "bone_" + curBone.idx;
                else
                    curBone.name = recovered_name;

                curBone.parent = IO.ReadInt16(br, endian);
                curBone._unkA = IO.ReadUInt16(br, endian);
                curBone._unkB = IO.ReadUInt16(br, endian);

                curBone.unknownA = IO.ReadUInt32(br, endian);
                curBone.unknownB = IO.ReadUInt32(br, endian);

                curBone.bone_World_X = IO.ReadFloat(br, endian);
                curBone.bone_World_Y = IO.ReadFloat(br, endian);
                curBone.bone_World_Z = IO.ReadFloat(br, endian);
                curBone.bone_World_W = IO.ReadFloat(br, endian);

                curBone.bone_Rot_X = IO.ReadFloat(br, endian);
                curBone.bone_Rot_Y = IO.ReadFloat(br, endian);
                curBone.bone_Rot_Z = IO.ReadFloat(br, endian);
                curBone.bone_Rot_W = IO.ReadFloat(br, endian);

                //Matrix!
                //Make matrix
                //  Matrix4x3 boneMatrix = Matrix4x3.CreateFromQuaternion((new Quaternion(curBone.bone_Rot_X,curBone.bone_Rot_Y, curBone.bone_Rot_Z,curBone.bone_Rot_W)));
                Matrix4x3 boneMatrix = Matrix4x3.CreateFromQuaternion(Quaternion.Zero);
                boneMatrix.Row3 = new Vector3(curBone.bone_World_X * 34, curBone.bone_World_Y * 34, curBone.bone_World_Z * 34);

                curBone.boneMatrix = boneMatrix;

                FoxBoneList.Add(curBone);
            }

            ConsolePrint("->Parsed Bones", Color.LimeGreen);
            return true;
        }

        private bool stub_UnkFloatList()
        {
            foxEntry foxENT = this.FoxBlockAList.Find((o) => o.entryId == EntryType.UNKFLOATLIST);
            if (foxENT == null)
            {
                return true;
            }

            //Seek
            fs.Seek((FOXA_OFFSET + foxENT.entryOffset), SeekOrigin.Begin);

            //Read
            for (int i = 0; i < foxENT.entryBlockCount; i++)
            {
                foxUnkFloat curEntry = new foxUnkFloat();


                curEntry.unkA_x = IO.ReadFloat(br, endian);
                curEntry.unkA_y = IO.ReadFloat(br, endian);
                curEntry.unkA_z = IO.ReadFloat(br, endian);
                curEntry.unkA_w = IO.ReadFloat(br, endian);

                curEntry.unkB_x = IO.ReadFloat(br, endian);
                curEntry.unkB_y = IO.ReadFloat(br, endian);
                curEntry.unkB_z = IO.ReadFloat(br, endian);
                curEntry.unkB_w = IO.ReadFloat(br, endian);


                FoxUnkFloatList.Add(curEntry);
            }

            ConsolePrint("->Parsed UnkFloats", Color.LimeGreen);
            return true;
        }

        private bool stub_MeshGroups()
        {
            foxEntry foxMeshGroupsEntry = this.FoxBlockAList.Find((o) => o.entryId == EntryType.MESHGROUPS);
            if (foxMeshGroupsEntry == null)
            {
                return true;
            }

            //Seek
            fs.Seek((FOXA_OFFSET + foxMeshGroupsEntry.entryOffset), SeekOrigin.Begin);

            //Read
            for (int i = 0; i < foxMeshGroupsEntry.entryBlockCount; i++)
            {
                foxMeshGroup curGroup = new foxMeshGroup();

                curGroup.strTable_idx = IO.ReadUInt16(br, endian);
                curGroup.unkVal = IO.ReadUInt16(br, endian);
                //curGroup.name = FoxStringList[(int)curGroup.strTable_idx].str;
                curGroup.name = "meshgroup_" + curGroup.strTable_idx;
                curGroup.gSegA = IO.ReadUInt16(br, endian);
                curGroup.gSegB = IO.ReadUInt16(br, endian);

                FoxMeshGroupList.Add(curGroup);
            }


            ConsolePrint("->Parsed MeshGroups", Color.LimeGreen);
            return true;

        }

        private bool stub_MeshGroupsInfo()
        {
            foxEntry foxENT = this.FoxBlockAList.Find((o) => o.entryId == EntryType.MESHGROUPSINFO);
            if (foxENT == null)
            {
                return true;
            }

            //Seek
            fs.Seek((FOXA_OFFSET + foxENT.entryOffset), SeekOrigin.Begin);

            //Read
            for (int i = 0; i < foxENT.entryBlockCount; i++)
            {
                foxMeshGroupInfo curGroupInfo = new foxMeshGroupInfo();

                curGroupInfo.unknown = IO.ReadUInt32(br, endian);
                curGroupInfo.meshGroupIDX = IO.ReadUInt16(br, endian);
                curGroupInfo.meshCount = IO.ReadUInt16(br, endian);
                curGroupInfo.startCount = IO.ReadUInt16(br, endian);
                curGroupInfo.groupIDX = IO.ReadUInt16(br, endian);
                curGroupInfo.unknown2 = IO.ReadUInt32(br, endian);
                curGroupInfo.materialIDX = IO.ReadUInt16(br, endian);
                curGroupInfo.nulls = IO.ReadBytes(br, 14, endian);

                curGroupInfo._mshGroup = this.FoxMeshGroupList[curGroupInfo.meshGroupIDX];

                FoxMeshGroupInfoList.Add(curGroupInfo);
            }


            ConsolePrint("->Parsed MeshGroupsInfo", Color.LimeGreen);
            return true;

        }

        private bool stub_MeshInfo()
        {
            foxEntry foxMeshInfoEntry = this.FoxBlockAList.Find((o) => o.entryId == EntryType.DRAWCALLS);
            if (foxMeshInfoEntry == null)
            {
                return true;
            }

            //Seek
            fs.Seek((FOXA_OFFSET + foxMeshInfoEntry.entryOffset), SeekOrigin.Begin);

            //Read
            for (int i = 0; i < foxMeshInfoEntry.entryBlockCount; i++)
            {
                foxMeshInfo curMeshInfo = new foxMeshInfo();

                curMeshInfo.unk_varA = IO.ReadUInt32(br, endian);
                curMeshInfo.unk_varB_a = IO.ReadUInt16(br, endian);
                curMeshInfo.boneGroupIndex = IO.ReadUInt16(br, endian);
                curMeshInfo.mesh_idx = IO.ReadUInt16(br, endian);
                curMeshInfo.vertCount = IO.ReadUInt16(br, endian);
                curMeshInfo.unk_varD_a = IO.ReadUInt16(br, endian);
                curMeshInfo.unk_varD_b = IO.ReadUInt16(br, endian);
                curMeshInfo.faceStart = IO.ReadUInt32(br, endian);
                curMeshInfo.faceLength = IO.ReadUInt32(br, endian);
                curMeshInfo.unk_varF_a = IO.ReadUInt16(br, endian);
                curMeshInfo.unk_varF_b = IO.ReadUInt16(br, endian);
                curMeshInfo.nulls = IO.ReadBytes(br, 20, endian);
                FoxMeshInfoList.Add(curMeshInfo);
            }


            ConsolePrint("->Parsed MeshInfo", Color.LimeGreen);
            return true;

        }

        private bool stub_BoneLookup()
        {
            foxEntry foxENT = this.FoxBlockAList.Find((o) => o.entryId == EntryType.BONELOOKUPLIST);
            if (foxENT == null)
            {
                return true;
            }

            //Seek
            fs.Seek((FOXA_OFFSET + foxENT.entryOffset), SeekOrigin.Begin);

            //Read
            for (int i = 0; i < foxENT.entryBlockCount; i++)
            {
                foxBoneLookup curEntry = new foxBoneLookup();

                curEntry.tag = IO.ReadUInt16(br, endian);
                curEntry.bgroupCount = IO.ReadUInt16(br, endian);
                for (int x = 0; x < 32; x++)
                {
                    curEntry.boneLookups.Add(IO.ReadInt16(br, endian));
                }

                FoxBoneLookupList.Add(curEntry);
            }

            ConsolePrint("->Parsed BoneLUT", Color.LimeGreen);
            return true;
        }


        private bool stub_TexList()
        {
            foxEntry foxTexListEntry = this.FoxBlockAList.Find((o) => o.entryId == EntryType.TEXLIST);
            if (foxTexListEntry == null)
            {
                return true;
            }

            //Seek
            fs.Seek((FOXA_OFFSET + foxTexListEntry.entryOffset), SeekOrigin.Begin);

            //Read
            for (int i = 0; i < foxTexListEntry.entryBlockCount; i++)
            {
                foxTexture curTex = new foxTexture();

                curTex.texNameIdx = IO.ReadUInt16(br, endian);
                curTex.texPathIdx = IO.ReadUInt16(br, endian);

                //find
                //curTex.texName = FoxStringList[curTex.texNameIdx].str; //MGS GZ
                //curTex.texPath = FoxStringList[curTex.texPathIdx].str; //MGS GZ

                //get hash from hashtable
                uint64 texture_hash = FoxTexHashList[i].tex_hash;
                bool mtl_lookupFailed = false;

                //fixup this if mtl is valid
                if (masterTextureList != null)
                {
                    //find texture
                    MTL.TEX_ITEM item = masterTextureList.Elements.Find((o) => o.file_hash == texture_hash);
                    if (item != null)
                    {
                        string texFolderpath = texdirFolderpath;
                        string dds_texturePath = Path.ChangeExtension(item.file_path, ".dds");
                        curTex.texName = Path.GetFileName(dds_texturePath);
                        curTex.texPath = Path.GetDirectoryName(Path.Combine(texFolderpath, dds_texturePath));
                    }
                    else
                    {
                        mtl_lookupFailed = true;
                    }
                }
                else
                {
                    mtl_lookupFailed = true;
                }

                if (mtl_lookupFailed)
                {
                    curTex.texName = "tex_" + curTex.texNameIdx;
                    curTex.texPath = "texpath_" + curTex.texPathIdx;
                }

                //commit
                FoxTextureList.Add(curTex);

            }


            ConsolePrint("->Parsed Texture List", Color.LimeGreen);
            return true;
        }

        private bool stub_MatList()
        {
            foxEntry foxMatListEntry = this.FoxBlockAList.Find((o) => o.entryId == EntryType.MATLIST);
            if (foxMatListEntry == null)
            {
                return true;
            }

            //Seek
            fs.Seek((FOXA_OFFSET + foxMatListEntry.entryOffset), SeekOrigin.Begin);

            //Read
            for (int i = 0; i < foxMatListEntry.entryBlockCount; i++)
            {
                foxMaterial curMat = new foxMaterial();

                curMat.matNameIdx = IO.ReadUInt16(br, endian);
                curMat.texIdx = IO.ReadUInt16(br, endian);

                //find
                //curMat.matName = FoxStringList[curMat.matNameIdx].str; //MGS GZ
                curMat.matName = "unkmat_" + curMat.matNameIdx;

                if (curMat.texIdx > (FoxTextureList.Count() - 1))
                {
                    curMat.tex = new foxTexture();
                    curMat.tex.texName = "unknown";
                    ConsolePrint("Unable to match up texture index: " + curMat.texIdx + " Max texListCOunt is: " + FoxTextureList.Count(), Color.Yellow);
                }
                else
                    curMat.tex = FoxTextureList[curMat.texIdx];

                //commit
                FoxMaterialList.Add(curMat);

            }


            ConsolePrint("->Parsed Material List", Color.LimeGreen);
            return true;
        }

        private bool stub_DataFormatInfo()
        {
            foxEntry foxENT = this.FoxBlockAList.Find((o) => o.entryId == EntryType.DATAFORMATINFO);
            if (foxENT == null)
            {
                return true;
            }

            //Seek
            fs.Seek((FOXA_OFFSET + foxENT.entryOffset), SeekOrigin.Begin);

            //Read
            for (int i = 0; i < foxENT.entryBlockCount; i++)
            {

                foxDataFormatInfo curEntry;
                curEntry.seq_A = IO.ReadByte(br);
                curEntry.seq_B = IO.ReadByte(br);
                curEntry.BlockSize = IO.ReadByte(br);
                curEntry.seq_C = IO.ReadByte(br);
                curEntry.UnkOffset = IO.ReadUInt32(br, endian);


                //commit
                FoxDataFrmtList.Add(curEntry);
            }


            ConsolePrint("->Parsed DataFormatInfo", Color.LimeGreen);
            return true;
        }

        private bool stub_PreVertIndexTable()
        {
            foxEntry foxPreVertDefEntry = this.FoxBlockAList.Find((o) => o.entryId == EntryType.VBOIBOINFO);
            if (foxPreVertDefEntry == null)
            {
                return true;
            }

            //Seek
            fs.Seek((FOXA_OFFSET + foxPreVertDefEntry.entryOffset), SeekOrigin.Begin);

            //Read
            for (int i = 0; i < foxPreVertDefEntry.entryBlockCount; i++)
            {
                foxPreVertIndexTable curEntry = new foxPreVertIndexTable();

                curEntry.unkIdx = IO.ReadUInt32(br, endian);
                curEntry.entryLength = IO.ReadUInt32(br, endian);
                curEntry.vertOffset = IO.ReadUInt32(br, endian);
                curEntry.unkVal = IO.ReadUInt32(br, endian);


                //commit
                FoxPreVertDefList.Add(curEntry);

            }


            ConsolePrint("->Parsed PreVertNorIndex Table", Color.LimeGreen);
            return true;
        }

        private bool stub_TextureHashList()
        {
            foxEntry foxTexHashTable = this.FoxBlockAList.Find((o) => o.entryId == EntryType.TEXHASHTABLE);
            if (foxTexHashTable == null)
            {
                return true;
            }

            //Seek
            fs.Seek((FOXA_OFFSET + foxTexHashTable.entryOffset), SeekOrigin.Begin);

            //Read
            for (int i = 0; i < foxTexHashTable.entryBlockCount; i++)
            {
                foxTextureHashTable curEntry = new foxTextureHashTable();
                curEntry.tex_hash = IO.ReadUInt64(br, endian);

                //commit
                FoxTexHashList.Add(curEntry);
            }


            ConsolePrint("->Parsed TextureHash Table", Color.LimeGreen);
            return true;
        }

        #endregion

        #region "Parse"

        //groups meshes into their meshgroups
        private void stub_proMeshGroups()
        {

            for (int i = 0; i < FoxMeshGroupInfoList.Count; i++)
            {
                //get
                foxMeshGroupInfo curGroup = FoxMeshGroupInfoList[i];
                for (int g = curGroup.startCount; g < (curGroup.startCount + curGroup.meshCount); g++)
                {
                    //Get
                    foxMeshInfo curInfo = FoxMeshInfoList[g];
                    //Set
                    curGroup.mshPieces.Add(curInfo);
                }
            }

        }

        private void stub_SetMat()
        {



            foxMaterialGroup curGroup;
            for(int i = 0; i < FoxMaterialList.Count; i++)
            {
                //get
                foxMaterial fmat = FoxMaterialList[i];
                if (fmat.matName.StartsWith("Base"))
                {
                    //make new entry
                    //new entry
                    int breakPointPos = -1;
                    curGroup = new foxMaterialGroup();
                    curGroup.idx = i;
                    curGroup.append(fmat);


                    for(int j = i + 1; j < FoxMaterialList.Count; j++)
                    {
                        if (FoxMaterialList[j].matName.StartsWith("Base"))
                        {
                            breakPointPos = j;
                            break;
                        }
                        else
                        {
                            curGroup.append(FoxMaterialList[j]);
                            breakPointPos = j;
                        }
                    }


                    this.fmats.Add(curGroup);
                    i = (breakPointPos - 1);

                }

            }





            for(int x = 0; x < FoxMeshInfoList.Count; x++)
            {
                foxMeshInfo drawcall = FoxMeshInfoList[x];
                //string mat = fmats[drawcall.unk_varB_a].foxMat[0].tex.texName;


                //resolve material
                string resolved_material = FoxMaterialList[drawcall.unk_varB_a].tex.texName;
                string mat = "foxmat_" + x;
                // ConsolePrint("drawcall " + drawcall.mesh_idx + " " + mat);
                // drawcall.texName = Path.ChangeExtension(mat, "dds");
                drawcall.texName = resolved_material;
            }



        }

        private void stub_CopyUsedTextures(string outpath)
        {
            ConsolePrint("Copying used textures");
            string texture_folderPath = Path.Combine(outpath, "Textures");
            Utilz.CreatePath(texture_folderPath);

            //Lets make it easier on the user and copy all the required textures from this model
            for (int i = 0; i < FoxTextureList.Count; i++)
            {
                foxTexture tex = FoxTextureList[i];
                string texpath = tex.fullPath;
                if (File.Exists(texpath))
                    File.Copy(texpath, Path.Combine(texture_folderPath, tex.texName), true);

            }
        }

        private void stub_PrepMeshData()
        {

            //Get our tmp offsets
            uint32 VertexBufferOffset = this.FOXB_OFFSET + this.FoxBlockBList[1].ent_offset;
            uint32 NormalBufferOffset = VertexBufferOffset + this.FoxPreVertDefList[1].vertOffset;
            uint32 FaceBufferOffset = VertexBufferOffset + this.FoxPreVertDefList[2].vertOffset;


            int format = 0;
            int format_ptr = 0;

            string fakepathDir = Path.Combine(Path.GetDirectoryName(this.fs.Name), "_exDUMP");
            for (int i = 0; i < this.FoxMeshGroupInfoList.Count; i++)
            {


                //Get
                foxMeshGroupInfo curGroup = FoxMeshGroupInfoList[i];

                List<cMesh> comMeshPieces = new List<cMesh>();

                //Folders
                string groupFolder = Path.Combine(curGroup._mshGroup.name);


                //loop pieces
                for (int j = 0; j < curGroup.mshPieces.Count; j++)
                {
                    //GET
                    foxMeshInfo curFOXMesh = curGroup.mshPieces[j];

                    fs.Seek(VertexBufferOffset, SeekOrigin.Begin);

                    int32 vert_count = (int)curFOXMesh.vertCount;
                    int32 face_count = (int)(curFOXMesh.faceLength / 3);

                    //mod mesh
                    cMesh curMesh = new cMesh();
                    List<cVertex> VBO = new List<cVertex>();
                    List<cFace> FBO = new List<cFace>();
                    List<cBone> BoneList = new List<cBone>();

                    //Read verticies into vbo
                    for (int vtx = 0; vtx < vert_count; vtx++)
                    {
                        cVertex vert = new cVertex();
                        //vert.vertPosInBuffer = fs.Position;
                        vert.Position = IO.ReadVec3(br, endian) * 34;
                        VBO.Add(vert);
                    }

                    //update vert pointer
                    IO.AlignStream(fs, 16);
                    VertexBufferOffset = (uint)fs.Position;

                    //jump to normals block
                    fs.Seek(NormalBufferOffset, SeekOrigin.Begin);


                    while (FoxDataFrmtList[format_ptr].BlockSize != 0x0C)
                    {
                        format_ptr = format_ptr + 1;
                    }

                    format_ptr = format_ptr + 1; // first after
                    format = FoxDataFrmtList[format_ptr].BlockSize;


                    for (int nmx = 0; nmx < vert_count; nmx++)
                    {
                        //Get vertex
                        cVertex vert = VBO[nmx];


                        switch (format)
                        {
                        case 0x28:
                            //blockSize 40

                            // normals - as half floats
                            vert.Normal = new Vector3(IO.ReadHalfFloat(br, endian), IO.ReadHalfFloat(br, endian), IO.ReadHalfFloat(br, endian));
                            float vertNorW40 = IO.ReadHalfFloat(br, endian);

                            //??
                            float delta0X240 = IO.ReadFloat(br, endian);
                            float delta0Y240 = IO.ReadFloat(br, endian);

                            //Color i think
                            uint32 p040 = IO.ReadUInt32(br, endian); //col
                            //weight
                            vert.dx_weight = C3D.DecompressBoneWeights(IO.ReadByte(br));
                            vert.dx_weight2 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                            vert.dx_weight3 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                            vert.dx_weight4 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                            //links to bone
                            vert.dx_index = this.FoxBoneLookupList[curFOXMesh.boneGroupIndex].get(IO.ReadByte(br));
                            vert.dx_index2 = this.FoxBoneLookupList[curFOXMesh.boneGroupIndex].get(IO.ReadByte(br));
                            vert.dx_index3 = this.FoxBoneLookupList[curFOXMesh.boneGroupIndex].get(IO.ReadByte(br));
                            vert.dx_index4 = this.FoxBoneLookupList[curFOXMesh.boneGroupIndex].get(IO.ReadByte(br));
                            //uv
                            vert.TextureUV = new Vector2(IO.ReadHalfFloat(br, endian), 1 - (IO.ReadHalfFloat(br, endian)));

                            //no fucken clue
                            uint32 unkDW3240 = IO.ReadUInt32(br, endian);
                            uint32 unkDW3240_2 = IO.ReadUInt32(br, endian);

                            break;
                        case 0x24:
                            //blockSize 36

                            // normals - as half floats
                            vert.Normal = new Vector3(IO.ReadHalfFloat(br, endian), IO.ReadHalfFloat(br, endian), IO.ReadHalfFloat(br, endian));
                            float vertNorW0 = IO.ReadHalfFloat(br, endian);

                            //??
                            float delta0X2 = IO.ReadFloat(br, endian);
                            float delta0Y2 = IO.ReadFloat(br, endian);

                            //Color i think
                            uint32 p0 = IO.ReadUInt32(br, endian); //col
                            //weight
                            vert.dx_weight = C3D.DecompressBoneWeights(IO.ReadByte(br));
                            vert.dx_weight2 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                            vert.dx_weight3 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                            vert.dx_weight4 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                            //links to bone
                            vert.dx_index = this.FoxBoneLookupList[curFOXMesh.boneGroupIndex].get(IO.ReadByte(br));
                            vert.dx_index2 = this.FoxBoneLookupList[curFOXMesh.boneGroupIndex].get(IO.ReadByte(br));
                            vert.dx_index3 = this.FoxBoneLookupList[curFOXMesh.boneGroupIndex].get(IO.ReadByte(br));
                            vert.dx_index4 = this.FoxBoneLookupList[curFOXMesh.boneGroupIndex].get(IO.ReadByte(br));
                            //uv
                            vert.TextureUV = new Vector2(IO.ReadHalfFloat(br, endian), 1 - (IO.ReadHalfFloat(br, endian)));

                            //no fucken clue
                            uint32 unkDW32 = IO.ReadUInt32(br, endian);
                            break;
                        case 0x20:
                            //blockSize 32

                            // normals - as half floats
                            vert.Normal = new Vector3(IO.ReadHalfFloat(br, endian), IO.ReadHalfFloat(br, endian), IO.ReadHalfFloat(br, endian));
                            float vertNorW = IO.ReadHalfFloat(br, endian);

                            //??
                            float deltaX2 = IO.ReadFloat(br, endian);
                            float deltaY2 = IO.ReadFloat(br, endian);

                            //Color i think
                            uint32 p1 = IO.ReadUInt32(br, endian); //col
                            //weight
                            vert.dx_weight = C3D.DecompressBoneWeights(IO.ReadByte(br));
                            vert.dx_weight2 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                            vert.dx_weight3 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                            vert.dx_weight4 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                            //links to bone
                            vert.dx_index = this.FoxBoneLookupList[curFOXMesh.boneGroupIndex].get(IO.ReadByte(br));
                            vert.dx_index2 = this.FoxBoneLookupList[curFOXMesh.boneGroupIndex].get(IO.ReadByte(br));
                            vert.dx_index3 = this.FoxBoneLookupList[curFOXMesh.boneGroupIndex].get(IO.ReadByte(br));
                            vert.dx_index4 = this.FoxBoneLookupList[curFOXMesh.boneGroupIndex].get(IO.ReadByte(br));
                            //uv
                            vert.TextureUV = new Vector2(IO.ReadHalfFloat(br, endian), 1 - (IO.ReadHalfFloat(br, endian)));

                            break;
                        case 0x1C:
                            //blockSize 28

                            // normals - as half floats
                            vert.Normal = new Vector3(IO.ReadHalfFloat(br, endian), IO.ReadHalfFloat(br, endian), IO.ReadHalfFloat(br, endian));
                            float vertNor2W = IO.ReadHalfFloat(br, endian);

                            //??
                            float delta2X2 = IO.ReadFloat(br, endian);
                            float delta2Y2 = IO.ReadFloat(br, endian);

                            //weight
                            vert.dx_weight = C3D.DecompressBoneWeights(IO.ReadByte(br));
                            vert.dx_weight2 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                            vert.dx_weight3 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                            vert.dx_weight4 = C3D.DecompressBoneWeights(IO.ReadByte(br));
                            //links to bone
                            vert.dx_index = this.FoxBoneLookupList[curFOXMesh.boneGroupIndex].get(IO.ReadByte(br));
                            vert.dx_index2 = this.FoxBoneLookupList[curFOXMesh.boneGroupIndex].get(IO.ReadByte(br));
                            vert.dx_index3 = this.FoxBoneLookupList[curFOXMesh.boneGroupIndex].get(IO.ReadByte(br));
                            vert.dx_index4 = this.FoxBoneLookupList[curFOXMesh.boneGroupIndex].get(IO.ReadByte(br));

                            if (vert.dx_index >= FoxBoneList.Count || vert.dx_index2 >= FoxBoneList.Count || vert.dx_index3 >= FoxBoneList.Count || vert.dx_index4 >= FoxBoneList.Count)
                                throw new Exception("um no no");


                            //uv
                            vert.TextureUV = new Vector2(IO.ReadHalfFloat(br, endian), 1 - (IO.ReadHalfFloat(br, endian)));

                            break;
                        case 0x14:
                            //blockSize 14

                            // normals - as half floats
                            vert.Normal = new Vector3(IO.ReadHalfFloat(br, endian), IO.ReadHalfFloat(br, endian), IO.ReadHalfFloat(br, endian));
                            float vertNor3W = IO.ReadHalfFloat(br, endian);

                            //??
                            float delta3X2 = IO.ReadFloat(br, endian);
                            float delta3Y2 = IO.ReadFloat(br, endian);


                            //uv
                            vert.TextureUV = new Vector2(IO.ReadHalfFloat(br, endian), 1 - (IO.ReadHalfFloat(br, endian)));

                            break;
                        default:
                            //blockSize Unknow
                            throw new Exception("Error BlockSize not found Engine requests size: " + format);
                        }


                    }

                    //update normal pointer
                    IO.AlignStream(fs, 16);
                    NormalBufferOffset = (uint)fs.Position;

                    //seek to faces
                    fs.Seek(FaceBufferOffset + (curFOXMesh.faceStart * 2), SeekOrigin.Begin);

                    for (int fx = 0; fx < face_count; fx++)
                    {
                        cFace curFace = new cFace();
                        curFace.vtx1 = IO.ReadInt16(br, endian);
                        curFace.vtx2 = IO.ReadInt16(br, endian);
                        curFace.vtx3 = IO.ReadInt16(br, endian);

                        curFace.Flip();     //FIX FRONT FACE CULLING

                        FBO.Add(curFace);
                    }


                    //bones?
                    for (int b = 0; b < FoxBoneList.Count; b++)
                    {
                        foxBone cBone = FoxBoneList[b];

                        //Make matrix
                        Matrix4x3 tfm = cBone.boneMatrix;


                        //   if (cBone.parent != -1)
                        //    {
                        //        //get parent matrix
                        //        Matrix4x3 parent_tfm = FoxBoneList[cBone.parent].boneMatrix;
                        //        tfm = tfm * Matrix4x3.Invert(parent_tfm);

                        //     }


                        // and it just works -_-
                        Vector3 WORLD_XYZ = tfm.ExtractTranslation().as_XZY_NegZ;
                        Vector3 EULER_XYZ = Vector3.Zero;
                        Vector3 SCALE_XYZ = Vector3.One;

                        BoneList.Add(new cBone((cBone.idx - 1), cBone.parent, cBone.name, WORLD_XYZ,EULER_XYZ, SCALE_XYZ));
                    }


                    //collect bones + verts + faces
                    curMesh.idx = curFOXMesh.mesh_idx;
                    curMesh.name = "mesh_" + curMesh.idx;
                    curMesh.material = curFOXMesh.texName;
                    curMesh.VertexBuffer = VBO;
                    curMesh.FaceBuffer = FBO;
                    curMesh.BoneList = BoneList; // SHADOW ?


                    ConsolePrint("Parsed-> " + Path.GetFileName(groupFolder) + "/" + curMesh.name );

                    comMeshPieces.Add(curMesh);


                } //end pieces loop


                //Append to global list
                GLOBAL_MESHLIST.Add(new fMesh(curGroup,groupFolder, comMeshPieces));


            } //loop group end


        }

        private void stub_CombineCOMmeshes()
        {
            List<cMesh> dbgScope = new List<cMesh>();

            for (int i = 0; i < GLOBAL_MESHLIST.Count; i++)
            {
                //get
                fMesh fmsh = new fMesh(GLOBAL_MESHLIST[i]);


                var groupedPiecesList = fmsh.mshList
                                        .GroupBy(u => u.material)
                                        .Select(grp => grp.ToList())
                                        .ToList();



                for (int x = 0; x < groupedPiecesList.Count; x++)
                {
                    //get
                    var foxmeshCOL = groupedPiecesList[x];
                    cMesh xPrime = new cMesh(foxmeshCOL[0]);
                    //xPrime.idx = x;
                    //xPrime.name = "mesh_" + xPrime.idx;
                    //add the rest to this mesh
                    for (int r = 1; r < foxmeshCOL.Count; r++)
                    {
                        xPrime.AppendMeshChunk(foxmeshCOL[r]);
                    }

                    //xPrime now has all the meshes inside it
                    fmsh.mshList.Clear();
                    dbgScope.Add(xPrime);
                } //falls out of scope here


                fmsh.mshList = new List<cMesh>(dbgScope); //make sure this doesnt fall
                dbgScope.Clear(); //CLEAR LIST
                OPTIMIZED_MESHLIST.Add(fmsh); //add to here (doesnt go out of scope)

            } //each group end


        } //function end

        private void stub_CombineMeshV2()
        {
            ConsolePrint("Further merging meshdata");
            for (int i = 0; i < OPTIMIZED_MESHLIST.Count; i++)
            {
                //fMesh mesh = OPTIMIZED_MESHLIST[i];

                //TODO MERGE BABY MERGE
            }
        }

        #endregion



        #region misc
        private void ConsolePrint(string msg)
        {
            RaiseFMDLConsolePrint(msg, Color.White);
        }

        private void ConsolePrint(string msg, Color col)
        {
            RaiseFMDLConsolePrint(msg, col);
        }
        #endregion


    }
}
