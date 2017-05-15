using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InjectionDll.Interface
{
    [Serializable]
    public delegate void MessageReceivedEvent(MessageReceivedEventArgs message);
    [Serializable]
    public delegate void DisconnectedEvent();

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

        public void Message(string message)
        {
            SafeInvokeMessageRecevied(new MessageReceivedEventArgs(message));
        }

        public void Ping()
        {
            Message("pinging");
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
    }

    public class ClientCaptureInterfaceEventProxy : MarshalByRefObject
    {
        public event DisconnectedEvent Disconnected;

        public void DisconnectedProxyHandler()
        {
            if (Disconnected != null)
                Disconnected();
        }
    }
}
