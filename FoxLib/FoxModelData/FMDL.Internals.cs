using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


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

    //store internal structures here
    public partial class FMDL
    {

        //All the different entries
        public enum EntryType : int
        {
            UNKNOWNTYPE = -1,
            BONELIST = 0,           //BoneList
            MESHGROUPS = 1,         //Mesh Groups
            MESHGROUPSINFO = 2,     //Mesh GroupInfo (this groups the drawcalls int the groups above ^)
            DRAWCALLS = 3,          //Mesh Info
            UNKSTRUC4 = 4,          //??
            BONELOOKUPLIST = 5,     //boneLookupTable
            TEXLIST = 6,            //texList
            MATLIST = 7,            //matList
            UNKSTRUC8 = 8,          //??
            UNKSTRUC9 = 9,          //??
            DATAFORMATINFO = 10,    //DataFormatInfo
            UNKSTRUC11 = 11,        //??
            STRINFOLIST = 12,       //StringAttrbTable
            UNKFLOATLIST = 13,      //UnkFloatList
            VBOIBOINFO = 14,        //PreVertIndexTable
            UNKSTRUC15 = 15,        //??
            UNKSTRUC16 = 16,        //??
            UNKSTRUC17 = 17,        //??
            UNKSTRUC18 = 18,        //??
            TEXHASHTABLE = 21       //TextureHashTable
        }


        public enum EntryType2 : int
        {
            UNKNOWNTYPE = -1,
            PREVERTDATA  = 0,
            VERTEXINDEXBUFFER = 2,
            STRINGTABLE = 3,
        }


        public struct fmdl_head
        {
            public  string   magic;
            public uint16 uvar_aS;
            public uint16 uvar_bS;
            public uint32 uvar_c;
            public uint32 uvar_zero;
            public uint16 uvar_d_a;
            public uint16 uvar_d_b;
            public uint32 uvar_e;
            public uint32 uvar_f;
            public uint32 uvar_g;
            public uint32 foxEntryACount; //table1
            public uint32 foxEntryBCount; //table2
            public uint32 startofMDLData;
            public uint32 lengtofMDLData;
            public uint32 startofVertexFaceData;
            public uint32 lengtofVertexFaceData;
            public uint32 uvar_j;
            public uint32 uvar_k;
        };

        public class foxEntry
        {
            public EntryType entryId;
            public uint16 entryBlockCount;
            public uint32 entryOffset;
        };


        public class foxEntry2
        {
            public EntryType2 ent_id;
            public uint32 ent_offset; //offset of entry: starting from 1st part of 'start of Vertex, face, filename data' (yellow 2a) (0x5AB0) for this file
            public uint32 ent_length;
        };

        [DebuggerDisplay("idx = {idx} name = {name}")]
        public class foxBone
        {

            public string name;
            public int16  idx; //name of string also
            public int16 parent;

            public uint16 _unkA;
            public uint16 _unkB;

            public uint32 unknownA;
            public uint32 unknownB;

            public float bone_World_X;
            public float bone_World_Y;
            public float bone_World_Z;
            public float bone_World_W;

            public float bone_Rot_X;
            public float bone_Rot_Y;
            public float bone_Rot_Z;
            public float bone_Rot_W;


            //Make matrix
            public Cra0Framework.Core.Graphics.Matrix4x3 boneMatrix;


        };

        [DebuggerDisplay("idx = {strTable_idx} name = {name}")]
        public struct foxMeshGroup
        {
            public string name;
            public uint16 strTable_idx;
            public uint16 unkVal;
            public uint16 gSegA;
            public uint16 gSegB;
        };

        [DebuggerDisplay(" idx = {meshGroupIDX} name = {_mshGroup.name}")]
        public class foxMeshGroupInfo
        {

            public foxMeshGroup _mshGroup;
            public List<foxMeshInfo> mshPieces = new List<foxMeshInfo>();

            public   uint32  unknown;
            public   uint16  meshGroupIDX; //this lines with the 01 entry and give the name of the group (eg MESH.HEAD)
            public   uint16  meshCount;   //for snake it says 0x17 so it means that the first 23 objects are in the MESH.HEAD group
            public   uint16  startCount;  //the number to start reading the count from, starts at 0, for the next it says to start at 23
            public   uint16  groupIDX; // same as above
            public   uint32  unknown2;   //??
            public   uint16  materialIDX;
            public   byte[]  nulls; //14
        };

        public struct foxStrAttr
        {
            public uint16 fthree;
            public uint16 strLength;
            public uint32 strOffset; //offset from start of all the string info for this particual string
        };

        [DebuggerDisplay("str = {str}")]
        public class foxString
        {
            public foxStrAttr strAttributes;
            public string str;
        }

        [DebuggerDisplay(" msh_idx = {mesh_idx}")]
        public class foxMeshInfo
        {
            public string texName; // debug
            public uint32 unk_varA;
            public uint16 unk_varB_a;
            public uint16 boneGroupIndex;   //indexes bone lookup
            public uint16 mesh_idx;
            public uint16 vertCount;
            public uint16 unk_varD_a;
            public uint16 unk_varD_b;
            public uint32 faceStart; // * 2
            public uint32 faceLength; // * 2
            public uint16 unk_varF_a;
            public uint16 unk_varF_b;
            public byte[] nulls; //20bytes
        };

        [DebuggerDisplay(" texName = {texName} texPath = {texPath}")]
        public struct foxTexture
        {
            public string texName;
            public string texPath;
            public uint16 texNameIdx;
            public uint16 texPathIdx;

            public string fullPath
            {
                get
                {
                    return Path.Combine(texPath, texName);
                }
            }


        };

        [DebuggerDisplay(" matName = {matName} ")]
        public struct foxMaterial
        {
            public string matName;
            public foxTexture tex;
            public uint16 matNameIdx;
            public uint16 texIdx;
        };

        [DebuggerDisplay(" BlockSize = {BlockSize} ")]
        public struct foxDataFormatInfo
        {
            public byte seq_A;
            public byte seq_B;
            public byte BlockSize; //drawcall blocksize ///how many bytes to read for each vertex in the normal buffer
            public byte seq_C;
            public uint32 UnkOffset; //offset from start of normal buffers for where this object starts
        };


        public struct foxPreVertIndexTable
        {
            public uint32 unkIdx; //ID 0 is the vert POSITIONS 1 is Normal table 2 is indicies
            public uint32 entryLength; //length of entry
            public uint32 vertOffset; //offset from beg of vert buffer
            public uint32 unkVal;
        };

        [DebuggerDisplay("tex_hash = {tex_hash}")]
        public struct foxTextureHashTable
        {
            public uint64 tex_hash; //64bit hash of the texture
        };

        public class foxMaterialGroup
        {
            public int idx;
            public  List<foxMaterial> foxMat = new List<foxMaterial>();


            public void append(foxMaterial inMat)
            {
                foxMat.Add(inMat);
            }
        }


        public struct foxUnkFloat //13
        {
            public float unkA_x;
            public float unkA_y;
            public float unkA_z;
            public float unkA_w;

            public float unkB_x;
            public float unkB_y;
            public float unkB_z;
            public float unkB_w;
        };


        public class foxBoneLookup //5
        {
            public uint16 tag;
            public uint16 bgroupCount;
            public List<int16> boneLookups = new List<int16>();

            public int16 get(int16 dxIndex)
            {
                return boneLookups[dxIndex];
            }

        };

        public static class FoxBoneMatcher
        {

            public enum BONE_NAME2 : int
            {
                SKL_Unknown = -1,
                SKL_000_WAIST = 1,
                SKL_001_SPINE = 2,
                SKL_002_CHEST = 3,
                SKL_003_NECK = 4,
                SKL_004_HEAD = 5,
                SKL_010_LSHLD = 6,
                SKL_011_LUARM = 7,
                SKL_012_LFARM = 8,
                SKL_013_LHAND = 9,
                SKL_020_RSHLD = 10,
                SKL_021_RUARM = 11,
                SKL_022_RFARM = 12,
                SKL_023_RHAND = 13,
                SKL_030_LTHIGH = 14,
                SKL_031_LLEG = 15,
                SKL_032_LFOOT = 16,
                SKL_033_LTOE = 17,
                SKL_040_RTHIGH = 18,
                SKL_041_RLEG = 19,
                SKL_042_RFOOT = 20,
                SKL_043_RTOE = 21,
            }

            public enum BONE_NAME : int
            {
                SKL_Unknown = -1,
                SKL_000_WAIST = 1,
                SKL_001_SPINE = 2,
                SKL_002_CHEST = 3,
                SKL_003_NECK = 4,
                SKL_004_HEAD = 5,
                SKL_010_LSHLD = 6,
                SKL_011_LUARM = 7,
                SKL_012_LFARM = 8,
                SKL_013_LHAND = 9,
                SKL_020_RSHLD = 10,
                SKL_021_RUARM = 11,
                SKL_022_RFARM = 12,
                SKL_023_RHAND = 13,
                SKL_030_LTHIGH = 14,
                SKL_031_LLEG = 15,
                SKL_032_LFOOT = 16,
                SKL_033_LTOE = 17,
                SKL_040_RTHIGH = 18,
                SKL_041_RLEG = 19,
                SKL_042_RFOOT = 20,
                SKL_043_RTOE = 21,
                SKL_101_LF10 = 22,
                SKL_102_LF11 = 23,
                SKL_103_LF12 = 24,
                SKL_104_LF21 = 25,
                SKL_105_LF22 = 26,
                SKL_106_LF23 = 27,
                SKL_107_LF31 = 28,
                SKL_108_LF32 = 29,
                SKL_109_LF33 = 30,
                SKL_110_LF40 = 31,
                SKL_111_LF41 = 32,
                SKL_112_LF42 = 33,
                SKL_113_LF43 = 34,
                SKL_114_LF51 = 35,
                SKL_115_LF52 = 36,
                SKL_116_LF53 = 37,
                SKL_201_RF10 = 38,
                SKL_202_RF11 = 39,
                SKL_203_RF12 = 40,
                SKL_204_RF21 = 41,
                SKL_205_RF22 = 42,
                SKL_206_RF23 = 43,
                SKL_207_RF31 = 44,
                SKL_208_RF32 = 45,
                SKL_209_RF33 = 46,
                SKL_210_RF40 = 47,
                SKL_211_RF41 = 48,
                SKL_212_RF42 = 49,
                SKL_213_RF43 = 50,
                SKL_214_RF51 = 51,
                SKL_215_RF52 = 52,
                SKL_216_RF53 = 53,
                SKL_400_HEADROOT = 54,
                SKL_401_MOUTHROOT = 55,
                SKL_402_EYELROOT = 56,
                SKL_403_EYERROOT = 57,
                SKL_404_JAWS = 58,
                SKL_405_LPUL = 59,
                SKL_406_LPUR = 60,
                SKL_407_LPLL = 61,
                SKL_408_LPLR = 62,
                SKL_409_LPML = 63,
                SKL_410_LPMR = 64,
                SKL_411_LPNL = 65,
                SKL_412_LPCL = 66,
                SKL_413_LPNR = 67,
                SKL_414_LPCR = 68,
                SKL_415_LPUI = 69,
                SKL_416_LPLI = 70,
                SKL_417_MDBL = 71,
                SKL_418_MDBR = 72,
                SKL_419_TONE = 73,
                SKL_420_THRT = 74,
                SKL_421_CHKL = 75,
                SKL_422_CHKR = 76,
                SKL_423_JAWL = 77,
                SKL_424_JAWR = 78,
                SKL_425_NOSE = 79,
                SKL_426_EBOL = 80,
                SKL_427_EBOR = 81,
                SKL_428_EBEL = 82,
                SKL_429_EBER = 83,
                SKL_430_EYEL = 84,
                SKL_431_EYER = 85,
                SKL_432_EIUL = 86,
                SKL_433_EIUR = 87,
                SKL_434_EYLL = 88,
                SKL_435_EYLR = 89,
                SKL_436_MCKL = 90,
                SKL_437_MCKR = 91,
                SKL_500_LLRPZ_HLP = 92,
                SKL_502_LHMRS_HLP = 93,
                SKL_503_LUARL_HLP = 94,
                SKL_504_LELBW_HLP = 95,
                SKL_505_LFARL_HLP = 96,
                SKL_506_LTNB_HLP = 97,
                SKL_507_LTOW_HLP = 98,
                SKL_509_RLRPZ_HLP = 99,
                SKL_511_RHMRS_HLP = 100,
                SKL_512_RUARL_HLP = 101,
                SKL_513_RELBW_HLP = 102,
                SKL_514_RFARL_HLP = 103,
                SKL_515_RTNB_HLP = 104,
                SKL_516_RTOW_HLP = 105,
                SKL_518_LBUD_HLP = 106,
                SKL_519_LMRF_HLP = 107,
                SKL_520_LPTL_HLP = 108,
                SKL_524_RBUD_HLP = 109,
                SKL_525_RMRF_HLP = 110,
                SKL_526_RPTL_HLP = 111,
                SKL_530_CLVR_HLP = 112,
                SKL_600_PTUL_HLP = 113,
                SKL_601_PTUL_HLP = 114,
                SKL_602_SLVR_HLP = 115,
                SKL_603_SLVL_HLP = 116,
                SKL_604_CLNR_HLP = 117,
                SKL_605_CLNL_HLP = 118,
                SKL_630_HLRR_SIM = 119,
                SKL_631_HMRR_SIM = 120,
                SKL_632_HMMR_SIM = 121,
                SKL_633_HMRR_SIM = 122,
                SKL_634_HMRL_SIM = 123,
                SKL_640_HLRU_SIM = 124,
                SKL_641_HFLU_SIM = 125,
                SKL_642_HMRU_SIM = 126,
                SKL_643_HSMU_SIM = 127,
                SKL_644_HSRU_SIM = 128,
                SKL_645_HSLU_SIM = 129,
                SKL_660_PTUL_SIM = 130,
                SKL_661_PTUR_SIM = 131,
                SKL_662_PTDL_SIM = 132,
                SKL_663_PTDR_SIM = 133,
                SKL_665_SLLR_SIM = 134,
                SKL_667_SLRR_SIM = 135,
                SKL_669_SRLR_SIM = 136,
                SKL_671_SRRR_SIM = 137,
                SKL_701_CLTH_SIM = 138,
                SKL_702_CLTH_SIM = 139,
                SKL_703_CLTH_SIM = 140,
                SKL_711_CLTH_SIM = 141,
                SKL_712_CLTH_SIM = 142,
                SKL_713_CLTH_SIM = 143,
                SKL_721_CLTH_SIM = 144,
                SKL_722_CLTH_SIM = 145,
                SKL_723_CLTH_SIM = 146,
            }

            public static string FindMatch(int idx)
            {
                string bone_str = string.Empty;
                bool validType = Enum.IsDefined(typeof(BONE_NAME), idx);
                if (validType == true)
                    bone_str = ((BONE_NAME)idx).ToString();
                else
                    bone_str = "";
                return bone_str;
            }



        }


    }
}
