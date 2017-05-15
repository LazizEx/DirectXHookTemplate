using InjectionDll.Interface;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading.Tasks;

namespace InjectionDll
{
    public class EntryPoint : EasyHook.IEntryPoint
    {
        private InterfaceDll _interface;
        ClientCaptureInterfaceEventProxy _clientEventProxy = new ClientCaptureInterfaceEventProxy();
        IpcServerChannel _clientServerChannel = null;
        private System.Threading.ManualResetEvent _runWait;

        public EntryPoint(EasyHook.RemoteHooking.IContext context, String channelName, ConfigDll config)
        {
            _interface = EasyHook.RemoteHooking.IpcConnectClient<InterfaceDll>(channelName);
            
            #region Allow client event handlers (bi-directional IPC)

            // Attempt to create a IpcServerChannel so that any event handlers on the client will function correctly
            System.Collections.IDictionary properties = new System.Collections.Hashtable();
            properties["name"] = channelName;
            properties["portName"] = channelName + Guid.NewGuid().ToString("N"); // random portName so no conflict with existing channels of channelName

            BinaryServerFormatterSinkProvider binaryProv = new BinaryServerFormatterSinkProvider();
            binaryProv.TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;

            _clientServerChannel = new IpcServerChannel(properties, binaryProv);
            ChannelServices.RegisterChannel(_clientServerChannel, false);

            #endregion
        }

        public void Run(EasyHook.RemoteHooking.IContext context, String channelName, ConfigDll config)
        {
            //Если не использовать GAC, могут возникать проблемы с правильной организацией удаленных сборок. 
            //Это обходной путь, который гарантирует, что текущая сборка правильно связана
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += (sender, args) =>
            {
                return this.GetType().Assembly.FullName == args.Name ? this.GetType().Assembly : null;
            };

            var curProcess = Process.GetCurrentProcess();
            NativeMethods.MessageBox(curProcess.MainWindowHandle.ToString());
            //NativeMethods.SendMessage(handle, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

            // NOTE: This is running in the target process
            _interface.Message("Injected into process Id: " + EasyHook.RemoteHooking.GetCurrentProcessId().ToString());

            _runWait = new System.Threading.ManualResetEvent(false);
            _runWait.Reset();

            try
            {
                // point to Initialise the Hook


                _interface.Disconnected += _clientEventProxy.DisconnectedProxyHandler;

                _clientEventProxy.Disconnected += () =>
                {
                    // We can now signal the exit of the Run method
                    _runWait.Set();
                };

                // We start a thread here to periodically check if the host is still running
                // If the host process stops then we will automatically uninstall the hooks
                StartCheckHostIsAliveThread();

                // Wait until signaled for exit either when a Disconnect message from the host 
                // or if the the check is alive has failed to Ping the host.
                _runWait.WaitOne();

                // we need to tell the check host thread to exit (if it hasn't already)
                StopCheckHostIsAliveThread();


                // point of disposin all threads
                //NativeMethods.MessageBox("Disposing");
            }
            catch (Exception e)
            {
                _interface.Message("Method \"Run\". An unexpected error occured: " + e.ToString());
            }
            finally
            {
                try
                {
                    _interface.Message("Disconnecting from process " + EasyHook.RemoteHooking.GetCurrentProcessId());
                }
                catch
                {
                }

                // Remove the client server channel (that allows client event handlers)
                ChannelServices.UnregisterChannel(_clientServerChannel);

                // Always sleep long enough for any remaining messages to complete sending
                System.Threading.Thread.Sleep(100);
            }
            
        }

        Task _checkAlive;
        long _stopCheckAlive = 0;
        /// <summary>
        /// Begin a background thread to check periodically that the host process is still accessible on its IPC channel
        /// </summary>
        private void StartCheckHostIsAliveThread()
        {
            _checkAlive = new Task(() =>
            {
                try
                {
                    while (System.Threading.Interlocked.Read(ref _stopCheckAlive) == 0)
                    {
                        System.Threading.Thread.Sleep(1000);

                        // .NET Remoting exceptions will throw RemotingException
                        _interface.Ping();
                    }
                }
                catch // We will assume that any exception means that the hooks need to be removed. 
                {
                    // Signal the Run method so that it can exit
                    _runWait.Set();
                }
            });

            _checkAlive.Start();
        }

        /// <summary>
        /// Tell the _checkAlive thread that it can exit if it hasn't already
        /// </summary>
        private void StopCheckHostIsAliveThread()
        {
            System.Threading.Interlocked.Increment(ref _stopCheckAlive);
        }
    }
}
