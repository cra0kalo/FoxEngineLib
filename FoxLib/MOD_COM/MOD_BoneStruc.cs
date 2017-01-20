using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Cra0Framework.Core;
using Cra0Framework.Core.Graphics;

namespace MOD_COM
{

    [DebuggerDisplay("BONE_ID = {BONE_ID} parentBoneIndex = {parentBoneIndex} BoneName = {BoneName}")]
    public class MOD_BoneStruc
    {


        //Structure

        public int BONE_ID
        {
            get;
            set;
        }
        public string BoneName
        {
            get;
            set;
        }
        public int parentBoneIndex
        {
            get;
            set;
        }


        //3D
        public Vector3 Pos
        {
            get;
            set;
        }
        public Quaternion Rot
        {
            get;
            set;
        }
        public Vector3 RotEuler
        {
            get;
            set;
        }
        public Vector3 Scale
        {
            get;
            set;
        }


        public float BoneScale
        {
            get;
            set;
        }


        //Debug
        public long BufferPos
        {
            get;
            set;
        }




        /// <summary>
        /// Constructor for container
        /// </summary>


        public MOD_BoneStruc(int GMD_Bone_ID, string GMD_BoneName, int GMD_parentBoneIndex, Vector3 _pos3, Quaternion _quat4, Vector3 _scale3,long bufferPos)
        {
            //Append
            this.BONE_ID = GMD_Bone_ID;
            this.BoneName = (GMD_BoneName);
            this.parentBoneIndex = GMD_parentBoneIndex;

            this.Pos = _pos3;
            this.Rot = ((_quat4));
            this.Scale = _scale3;

            //format specs
            this.BufferPos = bufferPos;



        }





        ////Methods below




    }


}