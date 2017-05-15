using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjectionDll.DxHook.Common
{
    public class ImageElement
    {
        public System.IO.Stream ImageStream { get; set; }
        public float Alpha { get; set; }
        public System.Drawing.PointF Location { get; set; }
    }
}
