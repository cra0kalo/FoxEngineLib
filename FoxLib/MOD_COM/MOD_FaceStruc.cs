using System;
using System.Text;

using Cra0Framework.Core;
using Cra0Framework.Core.Graphics;

namespace MOD_COM
{

    /// <summary>
    /// Dump face info here
    /// </summary>
    public class MOD_FaceStruc
    {

        ////Unsigned we don't want to be having any negative indexes
        public Int32 face1
        {
            get;
            set;
        }
        public Int32 face2
        {
            get;
            set;
        }
        public Int32 face3
        {
            get;
            set;
        }




        public MOD_FaceStruc()
        {




        }



        /// <summary>
        /// Constructor for the Class
        /// </summary>
        public MOD_FaceStruc(Int32 f1, Int32 f2, Int32 f3)
        {
            face1 = f1;
            face2 = f2;
            face3 = f3;
        }


        public void Flip()
        {
            int[] verts = new int[2];
            verts[0] = this.face3;
            verts[1] = this.face1;
            this.face1 = verts[0];
            this.face3 = verts[1];
        }



    }

}