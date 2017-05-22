using EasyHook;
using InjectionDll.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace InjectionDll.DxHook
{
    internal abstract class BaseDXHook : IDXHook
    {
        protected readonly ClientCaptureInterfaceEventProxy InterfaceEventProxy = new ClientCaptureInterfaceEventProxy();

        public BaseDXHook(InterfaceDll ssInterface)
        {
            this.Interface = ssInterface;
            this.Timer = new Stopwatch();
            this.Timer.Start();
            this.FPS = new FramesPerSecond();
            ProcessLoad = new Common.ProcessLoad(new System.Drawing.Font("Arial", 12)) { Location = new System.Drawing.Point(5, 20), Color = System.Drawing.Color.Red, AntiAliased = true };

            Interface.DisplayText += InterfaceEventProxy.DisplayTextProxyHandler;
            InterfaceEventProxy.DisplayText += new DisplayTextEvent(InterfaceEventProxy_DisplayText);
        }

        protected Stopwatch Timer { get; set; }
        protected FramesPerSecond FPS { get; set; }
        protected TextDisplay TextDisplay { get; set; }
        protected Common.ProcessLoad ProcessLoad { get; set; }


        ~BaseDXHook()
        {
            Dispose(false);
        }

        void InterfaceEventProxy_DisplayText(DisplayTextEventArgs args)
        {
            TextDisplay = new TextDisplay()
            {
                Text = args.Text,
                Duration = args.Duration
            };
        }

        int _processId = 0;
        protected int ProcessId
        {
            get
            {
                if (_processId == 0)
                {
                    _processId = RemoteHooking.GetCurrentProcessId();
                }
                return _processId;
            }
        }

        protected virtual string HookName
        {
            get
            {
                return "BaseDXHook";
            }
        }

        protected void Frame()
        {
            FPS.Frame();
            if (TextDisplay != null && TextDisplay.Display)
                TextDisplay.Frame();
        }

        protected IntPtr[] GetVTblAddresses(IntPtr pointer, int numberOfMethods)
        {
            return GetVTblAddresses(pointer, 0, numberOfMethods);
        }

        protected IntPtr[] GetVTblAddresses(IntPtr pointer, int startIndex, int numberOfMethods)
        {
            List<IntPtr> vtblAddresses = new List<IntPtr>();

            IntPtr vTable = Marshal.ReadIntPtr(pointer);
            for (int i = startIndex; i < startIndex + numberOfMethods; i++)
                vtblAddresses.Add(Marshal.ReadIntPtr(vTable, i * IntPtr.Size)); // using IntPtr.Size allows us to support both 32 and 64-bit processes

            return vtblAddresses.ToArray();
        }

        protected void DebugMessage(string message)
        {
#if DEBUG
            try
            {
                Interface.Message("DEBUG: " + HookName + ": " + message);
            }
            catch (RemotingException)
            {
                // Ignore remoting exceptions
            }
#endif
        }

        #region IDXHook Members

        public InterfaceDll Interface
        {
            get;
            set;
        }

        private ConfigDll _config;
        public ConfigDll Config
        {
            get { return _config; }
            set
            {
                _config = value;
                //CaptureDelay = new TimeSpan(0, 0, 0, 0, (int)((1.0 / (double)_config.TargetFramesPerSecond) * 1000.0));
            }
        }

        protected List<LocalHook> Hooks = new List<LocalHook>();
        public abstract void Hook();

        public abstract void Cleanup();

        #endregion

        #region IDispose Implementation

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Only clean up managed objects if disposing (i.e. not called from destructor)
            if (disposing)
            {
                try
                {
                    // Uninstall Hooks
                    if (Hooks.Count > 0)
                    {
                        // First disable the hook (by excluding all threads) and wait long enough to ensure that all hooks are not active
                        foreach (var hook in Hooks)
                        {
                            // Lets ensure that no threads will be intercepted again
                            hook.ThreadACL.SetInclusiveACL(new int[] { 0 });
                        }

                        System.Threading.Thread.Sleep(100);

                        // Now we can dispose of the hooks (which triggers the removal of the hook)
                        foreach (var hook in Hooks)
                        {
                            hook.Dispose();
                        }

                        Hooks.Clear();
                    }

                    try
                    {
                        // Remove the event handlers
                        Interface.DisplayText -= InterfaceEventProxy.DisplayTextProxyHandler;
                    }
                    catch (RemotingException) { } // Ignore remoting exceptions (host process may have been closed)
                }
                catch
                {
                }
            }
        }

        #endregion
    }
}
