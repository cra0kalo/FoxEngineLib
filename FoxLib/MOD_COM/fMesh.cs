using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Cra0Framework.Core.FileFormats;
using Cra0Framework.Core.Graphics;

namespace MOD_COM
{
    public class fMesh
    {
        public FoxLib.FMDL.foxMeshGroupInfo group;
        public string groupFolder;
        public List<cMesh> mshList = new List<cMesh>();

        public fMesh()
        {

        }

        public fMesh(fMesh inMesh)
        {
            group = inMesh.group;
            groupFolder = inMesh.groupFolder;
            mshList = inMesh.mshList;
        }


        public fMesh(FoxLib.FMDL.foxMeshGroupInfo in_group, string gfolder, List<cMesh> mshl)
        {
            this.group = in_group;
            this.groupFolder = gfolder;
            this.mshList = mshl;
        }

    }
}
