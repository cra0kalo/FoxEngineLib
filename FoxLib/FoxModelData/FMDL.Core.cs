using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    //store structures here
    public partial class FMDL
    {
        public enum ErrorEvents
        {
            Unknown = -1,
            None = 0,
            InvalidFile = 1,
            InvalidVersion = 2,
            ErrorExport,
            CommonError,
            SuccessfulExport,

        }

        public struct foxModelDetails
        {
            public int boneCount;
            public int vertexCount;
            public int faceCount;
            public int lodCount;
            public int materialCount;
            public int meshCount;


            public foxModelDetails(int in_bcount, int in_vtxcount, int in_faccount, int in_lodcount, int in_matcount, int in_mshcount)
            {
                boneCount = in_bcount;
                vertexCount = in_vtxcount;
                faceCount = in_faccount;
                lodCount = in_lodcount;
                materialCount = in_matcount;
                meshCount = in_mshcount;
            }
        }

        public enum ExportFormat
        {
            OBJ,
            PLY,
            SMD,
            BMD
        }


        public struct ExportOptions
        {
            public bool retainNormals;
            public bool flipYZ;
            public bool exportMeta;
        }

        public static EntryType getType(int i)
        {
            EntryType type;
            bool validType = Enum.IsDefined(typeof(EntryType), i);
            if (validType == true)
                type = (EntryType)i;
            else
                type = EntryType.UNKNOWNTYPE;
            return type;
        }


        public static EntryType2 getType2(int i)
        {
            EntryType2 type;
            bool validType = Enum.IsDefined(typeof(EntryType2), i);
            if (validType == true)
                type = (EntryType2)i;
            else
                type = EntryType2.UNKNOWNTYPE;
            return type;
        }

    }



}
