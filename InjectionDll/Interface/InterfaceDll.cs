using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjectionDll.Interface
{
    public enum Direct3DVersion
    {
        Unknown,
        AutoDetect,
        Direct3D9,
        Direct3D10,
        Direct3D10_1,
        Direct3D11,
        Direct3D11_1,
    }

    [Serializable]
    public delegate void MessageReceivedEvent(MessageReceivedEventArgs message);
    [Serializable]
    public delegate void DisconnectedEvent();
    [Serializable]
    public delegate void DisplayTextEvent(DisplayTextEventArgs args);

    [Serializable]
    public class MessageReceivedEventArgs : MarshalByRefObject
    {
        public string Message { get; set; }

        public MessageReceivedEventArgs(string message)
        {
            Message = message;
        }

        public override string ToString()
        {
            return Message;
        }
    }

    [Serializable]
    public class InterfaceDll : MarshalByRefObject
    {
        public event MessageReceivedEvent RemoteMessage;
        public event DisconnectedEvent Disconnected;
        public event DisplayTextEvent DisplayText;

        public void Message(string message)
        {
            SafeInvokeMessageRecevied(new MessageReceivedEventArgs(message));
        }

        public void Ping()
        {
            //Message("pinging");
        }

        public void Disconnect()
        {
            SafeInvokeDisconnected();
        }

        private void SafeInvokeMessageRecevied(MessageReceivedEventArgs eventArgs)
        {
            if (RemoteMessage == null)
                return;         //No Listeners
            MessageReceivedEvent listener = null;
            Delegate[] dels = RemoteMessage.GetInvocationList();

            foreach (Delegate del in dels)
            {
                try
                {
                    listener = (MessageReceivedEvent)del;
                    listener.Invoke(eventArgs);
                }
                catch (Exception)
                {
                    //Could not reach the destination, so remove it
                    //from the list
                    RemoteMessage -= listener;
                }
            }
        }

        private void SafeInvokeDisconnected()
        {
            if (Disconnected == null)
                return;         //No Listeners

            DisconnectedEvent listener = null;
            Delegate[] dels = Disconnected.GetInvocationList();

            foreach (Delegate del in dels)
            {
                try
                {
                    listener = (DisconnectedEvent)del;
                    listener.Invoke();
                }
                catch (Exception)
                {
                    //Could not reach the destination, so remove it
                    //from the list
                    Disconnected -= listener;
                }
            }
        }

        private void SafeInvokeDisplayText(DisplayTextEventArgs displayTextEventArgs)
        {
            if (DisplayText == null)
                return;         //No Listeners

            DisplayTextEvent listener = null;
            Delegate[] dels = DisplayText.GetInvocationList();

            foreach (Delegate del in dels)
            {
                try
                {
                    listener = (DisplayTextEvent)del;
                    listener.Invoke(displayTextEventArgs);
                }
                catch (Exception)
                {
                    //Could not reach the destination, so remove it
                    //from the list
                    DisplayText -= listener;
                }
            }
        }
    }

    public class ClientCaptureInterfaceEventProxy : MarshalByRefObject
    {
        public event DisconnectedEvent Disconnected;
        public event DisplayTextEvent DisplayText;

        public void DisconnectedProxyHandler()
        {
            if (Disconnected != null)
                Disconnected();
        }
        public void DisplayTextProxyHandler(DisplayTextEventArgs args)
        {
            DisplayText?.Invoke(args);
        }
    }
}
