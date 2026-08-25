using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FZ4P.Extensions
{
    public static class FIndResultExtension
    {
        public static FindResult SaveImage(this FindResult myResult,Global m__G,int ImageIndex,string filePath = "")
        {
            if(filePath.Length != 0)
                m__G.oCam[0].SaveSourceImage(ImageIndex, filePath);
            return myResult;
        }
    }
}
