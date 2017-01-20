using System;
using System.Text;

using Cra0Framework.Core;
using Cra0Framework.Core.Graphics;


namespace MOD_COM
{


    /// <summary>
    /// Dump 3D Data here
    /// </summary>
    public class MOD_VertexStruc
    {

        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Tangent;
        public UInt32 VertexColor;
        public Vector2 TextureUV;
        public Vector2 TextureUV2;
        public Vector2 TextureUV3;


        public int vertStride;
        public long vertPosInBuffer;
        public int dx9_numofPAIRS;

        public float dx9_blendweight;
        public float dx9_blendweight2;
        public float dx9_blendweight3;
        public float dx9_blendweight4;


        public int dx_index;
        public int dx_index2;
        public int dx_index3;
        public int dx_index4;


        public struct Weights
        {
            public float HW_Weight;
            public int HW_BoneIndex;
        }


        public MOD_VertexStruc()
        {
            Position = new Vector3();
            Normal = new Vector3();
            Tangent = new Vector3();
            TextureUV = new Vector2();
            TextureUV2 = new Vector2();
            TextureUV3 = new Vector2();
        }



        /// <summary>
        /// Constructor for Input method2
        /// </summary>
        /// <remarks></remarks>
        public MOD_VertexStruc(float vX, float vY, float vZ, float nX, float nY, float nZ, float tU, float tV, UInt32 VertexColor, float dcl_blendweight, float dcl_blendweight2, float dcl_blendweight3, float dcl_blendweight4, int dcl_blendindices, int dcl_blendindices2, int dcl_blendindices3, int dcl_blendindices4, long IN_vertposBuffer, int IN_vertStride)
        {

            Position = new Vector3(vX, vY, vZ);
            Normal = new Vector3(nX, nY, nZ);
            TextureUV = new Vector2(tU, tV);

            this.VertexColor = VertexColor;


            //BoneStuff
            this.dx9_blendweight = dcl_blendweight;
            this.dx9_blendweight2 = dcl_blendweight2;
            this.dx9_blendweight3 = dcl_blendweight3;
            this.dx9_blendweight4 = dcl_blendweight4;

            this.dx_index = dcl_blendindices;
            this.dx_index2 = dcl_blendindices2;
            this.dx_index3 = dcl_blendindices3;
            this.dx_index4 = dcl_blendindices4;

            //VertPos
            this.vertPosInBuffer = IN_vertposBuffer;
            this.vertStride = IN_vertStride;

        }




        public MOD_VertexStruc(Vector3 pos, Vector3 nor, Vector2 uvs, UInt32 VertexColor, float dcl_blendweight, float dcl_blendweight2, float dcl_blendweight3, float dcl_blendweight4, byte dcl_blendindices, byte dcl_blendindices2, byte dcl_blendindices3, byte dcl_blendindices4, long IN_vertposBuffer, int IN_vertStride)
        {
            this.Position = pos;
            this.Normal = nor;
            this.TextureUV = uvs;

            this.VertexColor = VertexColor;


            //BoneStuff
            this.dx9_blendweight = dcl_blendweight;
            this.dx9_blendweight2 = dcl_blendweight2;
            this.dx9_blendweight3 = dcl_blendweight3;
            this.dx9_blendweight4 = dcl_blendweight4;

            this.dx_index = dcl_blendindices;
            this.dx_index2 = dcl_blendindices2;
            this.dx_index3 = dcl_blendindices3;
            this.dx_index4 = dcl_blendindices4;


            //VertPos
            this.vertPosInBuffer = IN_vertposBuffer;
            this.vertStride = IN_vertStride;



        }





        //Methods below
        public void scaleVerts(int value)
        {
            Position *= value;
        }

    }



}