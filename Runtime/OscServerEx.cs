using UnityEngine;

namespace OscJackExpansion
{
    public class OscServerEx : MonoBehaviour
    {
        [SerializeField] public int port = 3333;
        [SerializeField] public bool autoStart = true;

        [System.NonSerialized] internal uOSC.DataReceiveEvent onDataReceived = new uOSC.DataReceiveEvent();
        [System.NonSerialized] internal uOSC.ServerStartEvent onServerStarted = new uOSC.ServerStartEvent();
        [System.NonSerialized] internal uOSC.ServerStopEvent onServerStopped = new uOSC.ServerStopEvent();

#if NETFX_CORE
        private uOSC.Udp udp_ = new uOSC.Uwp.Udp();
        private uOSC.Thread thread_ = new uOSC.Uwp.Thread();
#else
        private uOSC.Udp udp_ = new uOSC.DotNet.Udp();
        private uOSC.Thread thread_ = new uOSC.DotNet.Thread();
#endif
        private uOSC.Parser parser_ = new uOSC.Parser();

        private int prevPort_;
        private bool isStarted_;

        [SerializeField] private string status = "Stop";
        [SerializeField, TextArea(5, 15)] private string messages = "";

        public bool isRunning => udp_.isRunning;

        private void Awake()
        {
            prevPort_ = port;
        }

        private void OnEnable()
        {
            if (autoStart)
                StartServer();
        }

        private void OnDisable()
        {
            StopServer();
        }

        private void Update()
        {
            UpdateReceive();
            UpdateChangePort();
        }

        public void StartServer()
        {
            if (isStarted_) return;

            udp_.StartServer(port);
            thread_.Start(UpdateMessage);

            isStarted_ = true;
            status = "Running";

            onServerStarted.Invoke(port);
        }

        public void StopServer()
        {
            if (!isStarted_) return;

            thread_.Stop();
            udp_.Stop();

            isStarted_ = false;
            status = "Stop";

            onServerStopped.Invoke(port);
        }

        private void UpdateReceive()
        {
            while (parser_.messageCount > 0)
            {
                var message = parser_.Dequeue();
                onDataReceived.Invoke(message);

#if UNITY_EDITOR
                messages = message.ToString() + "\n" + messages;
                if (messages.Length > 2000)
                    messages = messages.Substring(0, 2000);
#endif
            }
        }

        private void UpdateChangePort()
        {
            if (prevPort_ == port) return;

            if (isStarted_)
            {
                StopServer();
                StartServer();
            }

            prevPort_ = port;
        }

        private void UpdateMessage()
        {
            while (udp_.messageCount > 0)
            {
                var buf = udp_.Receive();
                int pos = 0;
                parser_.Parse(buf, ref pos, buf.Length);
            }
        }
    }
}
