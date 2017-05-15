using InjectionDll.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjectionDll.DxHook
{
    internal interface IDXHook : IDisposable
    {
        InterfaceDll Interface
        {
            get;
            set;
        }
        ConfigDll Config
        {
            get;
            set;
        }

        void Hook();

        void Cleanup();
    }
}
