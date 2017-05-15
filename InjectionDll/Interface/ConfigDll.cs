using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjectionDll.Interface
{
    [Serializable]
    public class ConfigDll
    {
        public Direct3DVersion Direct3DVersion { get; set; }
        public bool ShowOverlay { get; set; }
        public int TargetFramesPerSecond { get; set; }

        public ConfigDll()
        {
            Direct3DVersion = Direct3DVersion.AutoDetect;
        }
    }
}
