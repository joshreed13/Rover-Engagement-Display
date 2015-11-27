﻿using Caliburn.Micro;
using RED.Models;
using RED.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RED.ViewModels.ControlCenter
{
    public class NetworkManagerViewModel : PropertyChangedBase, ISubscribe
    {
        private const ushort DestinationPort = 11000;

        private NetworkManagerModel _model;
        private ControlCenterViewModel _cc;

        private INetworkEncoding encoding;
        private INetworkTransportProtocol continuousDataSocket;
        private IIPAddressProvider ipAddressProvider;

        public NetworkManagerViewModel(ControlCenterViewModel cc)
        {
            _model = new NetworkManagerModel();
            _cc = cc;

            encoding = new RoverProtocol();
            continuousDataSocket = new UDPEndpoint(DestinationPort, DestinationPort);
            ipAddressProvider = cc.MetadataManager;

            foreach (var command in _cc.MetadataManager.Commands)
                _cc.DataRouter.Subscribe(this, command.Id);
            //_cc.DataRouter.Subscribe(this, 1);
            //_cc.DataRouter.Subscribe(this, 50);

            Listen();
        }

        private async void Listen()
        {
            while (true)
            {
                byte[] buffer = await continuousDataSocket.ReceiveMessage();
                byte dataId;
                byte[] data = encoding.DecodePacket(buffer, out dataId);
                _cc.DataRouter.Send(dataId, data);
            }
        }

        public void ReceiveFromRouter(byte dataId, byte[] data)
        {
            IPAddress destIP = ipAddressProvider.GetIPAddress(dataId);
            SendPacket(dataId, data, destIP);
        }

        public async void SendPacket(byte dataId, byte[] data, IPAddress destIP)
        {
            if (destIP == null)
            {
                _cc.Console.WriteToConsole("Attempted to send packet with unknown IP address. DataId=" + dataId.ToString());
                return;
            }
            byte[] packet = encoding.EncodePacket(dataId, data);
            await continuousDataSocket.SendMessage(destIP, packet);
        }
    }
}