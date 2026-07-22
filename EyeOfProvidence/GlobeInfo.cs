using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EyeOfProvidence
{
    public enum GlobeRayMehtod
    {
        General = 0,
        Cube = 1
    }
    public class GlobeInfo
    {
        public string name = "Globe";
        public CamInfo[] camInfos;
        public GlobeRayMehtod rayMethod = GlobeRayMehtod.General;
    }
}
