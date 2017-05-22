using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjectionDll.DxHook.Common
{
    class ProcessLoad : TextElement, IDisposable
    {
        string val;
        System.Threading.Thread thread;
        System.Diagnostics.PerformanceCounter cpucounter;

        public ProcessLoad(Font font) : base(font)
        {
            cpucounter = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total");
            thread = new System.Threading.Thread(() =>
            {
                while (thread.ThreadState != System.Threading.ThreadState.Aborted)
                {
                    val = "CPU " + Math.Round(cpucounter.NextValue(), 0).ToString() + " %";
                    System.Threading.Thread.Sleep(4000);
                }
            })
            {
                IsBackground = true
            };
            thread.Start();
            
        }

        public override string Text { get => val; }

        public override void Frame()
        {
            //base.Frame();
        }

        ~ProcessLoad()
        {
            Dispose(false);
        }


        protected override void Dispose(bool disposing)
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
            if (cpucounter != null)
                cpucounter.Dispose();
            base.Dispose(disposing);
        }
    }
}
