using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace MOD_COM
{


    [DebuggerDisplay("OBJECT_ID = {OBJECT_ID} MeshName = {MeshName} MeshMaterial = {MeshMaterial}")]
    public class MOD_MESH
    {

        public int VtxCount;

        public int OBJECT_ID;
        public string MeshName;
        public string MeshMaterial;

        public List<MOD_VertexStruc> VBO = new List<MOD_VertexStruc>();
        public List<MOD_FaceStruc> FBO = new List<MOD_FaceStruc>();

        public MOD_MESH()
        {



        }


        public MOD_MESH(int in_objectid, string in_meshname, string in_meshmat, List<MOD_VertexStruc> in_vbo, List<MOD_FaceStruc> in_fbo)
        {

            //append
            this.OBJECT_ID = in_objectid;
            this.MeshName = in_meshname;
            this.MeshMaterial = in_meshmat;
            this.VBO = in_vbo;
            this.FBO = in_fbo;

        }









    }


}