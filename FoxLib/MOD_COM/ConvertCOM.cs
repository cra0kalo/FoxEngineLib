using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cra0Framework.Core.Graphics;
using Cra0Framework.Core.FileFormats;


namespace MOD_COM
{
    public static class COM
    {

        public static cMesh toMesh(MOD_MESH curMesh, List<MOD_BoneStruc> boneList)
        {

            cMesh ReturnMesh = new cMesh();
            ReturnMesh.idx = curMesh.OBJECT_ID;
            ReturnMesh.name = curMesh.MeshName;
            ReturnMesh.material = curMesh.MeshMaterial;

            for (int i = 0; i < curMesh.VBO.Count; i++)
            {
                if (boneList.Count != 0)
                {
                    ReturnMesh.VertexBuffer.Add(new cVertex(curMesh.VBO[i].Position, curMesh.VBO[i].Normal, curMesh.VBO[i].TextureUV, curMesh.VBO[i].VertexColor,
                                                            curMesh.VBO[i].dx_index, curMesh.VBO[i].dx_index2, curMesh.VBO[i].dx_index3, curMesh.VBO[i].dx_index4,
                                                            curMesh.VBO[i].dx9_blendweight, curMesh.VBO[i].dx9_blendweight2, curMesh.VBO[i].dx9_blendweight3, curMesh.VBO[i].dx9_blendweight4));
                }
                else
                {
                    ReturnMesh.VertexBuffer.Add(new cVertex(curMesh.VBO[i].Position, curMesh.VBO[i].Normal, curMesh.VBO[i].TextureUV, curMesh.VBO[i].VertexColor));
                }


            }

            for (int j = 0; j < curMesh.FBO.Count; j++)
            {
                ReturnMesh.FaceBuffer.Add(new cFace(curMesh.FBO[j].face1, curMesh.FBO[j].face2, curMesh.FBO[j].face3));
            }


            //bones?
            for (int k = 0; k < boneList.Count; k++)
            {
                MOD_BoneStruc curBone = boneList[k];
                ReturnMesh.BoneList.Add(new cBone(curBone.BONE_ID, curBone.parentBoneIndex, curBone.BoneName, curBone.Pos, curBone.Rot, curBone.Scale));
            }



            return ReturnMesh;
        }










    }
}
