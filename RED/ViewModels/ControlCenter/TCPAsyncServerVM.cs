﻿using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Caliburn.Micro;
using RED.Models.ControlCenter;

namespace RED.ViewModels.ControlCenter
{
    public class TCPAsyncServerVM : PropertyChangedBase
    {
        private TCPAsyncServerModel Model;
        private ControlCenterViewModel ControlCenterVM;

        public string LocalMachineName
        {
            get
            {
                return Model.LocalMachineName;
            }
            private set
            {
                Model.LocalMachineName = value;
                NotifyOfPropertyChange(() => LocalMachineName);
            }
        }
        public string LocalSoftwareName
        {
            get
            {
                return Model.LocalSoftwareName;
            }
            private set
            {
                Model.LocalSoftwareName = value;
                NotifyOfPropertyChange(() => LocalSoftwareName);
            }
        }

        public bool IsListening
        {
            get
            {
                return Model.IsListening;
            }
            private set
            {
                Model.IsListening = value;
                NotifyOfPropertyChange(() => IsListening);
            }
        }
        public bool IsConnected
        {
            get
            {
                return Connections.Count > 0;
            }
        }
        public short ListeningPort
        {
            get
            {
                return Model.ListeningPort;
            }
            private set
            {
                Model.ListeningPort = value;
                NotifyOfPropertyChange(() => ListeningPort);
            }
        }

        private TcpListener server;
        private List<TCPConnectionVM> Connections = new List<TCPConnectionVM>();

        public TCPAsyncServerVM(short portNum, ControlCenterViewModel CCVM)
        {
            Model = new TCPAsyncServerModel();
            ControlCenterVM = CCVM;

            LocalMachineName = "Red Master";
            LocalSoftwareName = "RED";

            ListeningPort = portNum;
        }

        public void Start()
        {
            server = new TcpListener(IPAddress.Any, ListeningPort);
            IsListening = true;
            Listen();
        }

        public void Stop()
        {
            server.Stop();
            IsListening = false;
            foreach (TCPConnectionVM client in Connections)
                client.Close();
        }

        private async void Listen()
        {
            server.Start();
            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                Connections.Add(new TCPConnectionVM(client, ControlCenterVM));
            }
        }
    }
}