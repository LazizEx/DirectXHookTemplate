using EasyHook;
using InjectionDll.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading.Tasks;

namespace InjectionDll
{
    public class MainClass : IDisposable
    {
        string _channelName = null;
        private IpcServerChannel _screenshotServer;
        private InterfaceDll _serverInterface;

        public MainClass(Process process, ConfigDll config, InterfaceDll Interface)
        {
            
            _serverInterface = Interface;

            _screenshotServer = RemoteHooking.IpcCreateServer<InterfaceDll>(ref _channelName, 
                System.Runtime.Remoting.WellKnownObjectMode.Singleton, _serverInterface);

            RemoteHooking.Inject(process.Id,
                InjectionOptions.Default,
                typeof(InterfaceDll).Assembly.Location,
                typeof(InterfaceDll).Assembly.Location,
                _channelName,
                config);
        }

        public InterfaceDll CaptureInterface
        {
            get { return _serverInterface; }
        }
        ~MainClass()
        {
            Dispose();
        }

        private bool _isDisposed = false;
        public void Dispose()
        {
            if (!_isDisposed)
            {

            }
        }
    }
}
