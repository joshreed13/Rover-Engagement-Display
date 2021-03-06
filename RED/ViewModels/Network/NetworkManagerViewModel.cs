﻿using Caliburn.Micro;
using RED.Contexts;
using RED.Interfaces;
using RED.Interfaces.Network;
using RED.Models.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RED.ViewModels.Network
{
    public class NetworkManagerViewModel : PropertyChangedBase, RED.Interfaces.ISubscribe
    {
        private const ushort DestinationPort = 11000;
        private const bool defaultReliable = false;

        private readonly TimeSpan ReliableRetransmissionTimeout = TimeSpan.FromMilliseconds(1000);
        private const int ReliableMaxRetries = 5;

        private readonly NetworkManagerModel _model;
        private readonly IDataRouter _router;
        private readonly ILogger _log;
        private readonly IIPAddressProvider _ipProvider;

        private INetworkEncoding encoding;
        private INetworkTransportProtocol continuousDataSocket;
        private ISequenceNumberProvider sequenceNumberProvider;

        private HashSet<UnACKedPacket> outgoingUnACKed = new HashSet<UnACKedPacket>();
        private HashSet<PendingPing> pendingPings = new HashSet<PendingPing>();

        public bool EnableReliablePackets
        {
            get
            {
                return _model.EnableReliablePackets;
            }
            set
            {
                _model.EnableReliablePackets = value;
                NotifyOfPropertyChange(() => EnableReliablePackets);
            }
        }

        public event Action<IPAddress> TelemetryRecieved;

        public NetworkManagerViewModel(IDataRouter router, MetadataRecordContext[] commands, ILogger log, IIPAddressProvider ipProvider)
        {
            _model = new NetworkManagerModel();
            _router = router;
            _log = log;
            _ipProvider = ipProvider;

            sequenceNumberProvider = new SequenceNumberManager();
            encoding = new RoverProtocol();
            continuousDataSocket = new UDPEndpoint(DestinationPort, DestinationPort);

            foreach (var command in commands)
                _router.Subscribe(this, command.Id);

            Listen();
        }

        private async void Listen()
        {
            while (true)
            {
                Tuple<IPAddress, byte[]> result = await continuousDataSocket.ReceiveMessage();
                ReceivePacket(result.Item1, result.Item2);
            }
        }

        public void ReceiveFromRouter(ushort dataId, byte[] data, bool reliable)
        {
            IPAddress destIP = _ipProvider.GetIPAddress(dataId);
            SendPacket(dataId, data, destIP, reliable);
        }

        public void SendPacket(ushort dataId, byte[] data, IPAddress destIP, bool isReliable)
        {
            ushort seqNum = sequenceNumberProvider.IncrementValue(dataId);
            SendPacket(dataId, data, destIP, isReliable, seqNum);
        }

        private void SendPacket(ushort dataId, byte[] data, IPAddress destIP, bool isReliable, ushort seqNum)
        {
            if (destIP == null)
            {
                _log.Log($"Attempted to send packet with unknown IP address. DataId={dataId}");
                return;
            }
            if (destIP.Equals(IPAddress.None))
            {
                _log.Log($"Attempted to send packet with invalid IP address. DataId={dataId} IP={destIP}");
                return;
            }

            byte[] packetData = encoding.EncodePacket(dataId, data, seqNum, isReliable);

            if (isReliable && EnableReliablePackets)
                SendPacketReliable(destIP, packetData, dataId, seqNum);
            else
                SendPacketUnreliable(destIP, packetData);
        }

        public async Task<TimeSpan> SendPing(IPAddress ip, TimeSpan timeout)
        {
            PendingPing ping = new PendingPing()
            {
                Timestamp = DateTime.Now,
                SeqNum = sequenceNumberProvider.IncrementValue((ushort)SystemDataId.Ping),
                Semaphore = new System.Threading.SemaphoreSlim(0)
            };
            pendingPings.Add(ping);

            SendPacket((ushort)SystemDataId.Ping, new byte[0], ip, false, ping.SeqNum);
            await ping.Semaphore.WaitAsync(timeout);
            return ping.RoundtripTime;
        }

        private void ProcessPing(byte[] data)
        {
            DateTime now = DateTime.Now;
            ushort responseSeqNum = BitConverter.ToUInt16(data, 0);
            PendingPing ping = pendingPings.FirstOrDefault(x => x.SeqNum == responseSeqNum);
            if (ping == null) return;

            ping.RoundtripTime = now - ping.Timestamp;
            ping.Semaphore.Release();
        }

        private async void SendPacketUnreliable(IPAddress destIP, byte[] packetData)
        {
            await continuousDataSocket.SendMessage(destIP, packetData);
        }

        private async void SendPacketReliable(IPAddress destIP, byte[] packetData, ushort dataId, ushort seqNum)
        {
            var packetInfo = new UnACKedPacket() { DataId = dataId, SeqNum = seqNum };
            outgoingUnACKed.Add(packetInfo);

            for (int i = 0; i < ReliableMaxRetries; i++)
            {
                await continuousDataSocket.SendMessage(destIP, packetData);
                await Task.Delay(ReliableRetransmissionTimeout);
                if (!outgoingUnACKed.Contains(packetInfo)) return;
            }

            outgoingUnACKed.Remove(packetInfo);
            if (dataId != (ushort)SystemDataId.Subscribe)
                _log.Log($"No ACK recieved for DataId={packetInfo.DataId} and SeqNum={packetInfo.SeqNum} after {ReliableMaxRetries} retries");
        }

        private void ReceivePacket(IPAddress srcIP, byte[] buffer)
        {
            TelemetryRecieved?.Invoke(srcIP);
            byte[] data = encoding.DecodePacket(buffer, out ushort dataId, out ushort seqNum, out bool needsACK);
            if (!sequenceNumberProvider.UpdateNewer(dataId, seqNum))
            {
                _log.Log($"Packet recieved with invalid sequence number={seqNum} DataId={dataId}");
                return;
            }
            if (needsACK)
                SendAck(srcIP, dataId, seqNum);
            InterpretDataId(srcIP, data, dataId, seqNum);
        }

        private void InterpretDataId(IPAddress srcIP, byte[] data, ushort dataId, ushort seqNum)
        {
            switch ((SystemDataId)dataId)
            {
                case SystemDataId.Null:
                    _log.Log("Packet recieved with null dataId");
                    break;
                case SystemDataId.Ping:
                    SendPacket((ushort)SystemDataId.PingReply, BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)seqNum)), srcIP, false);
                    break;
                case SystemDataId.PingReply:
                    ProcessPing(data);
                    break;
                case SystemDataId.Subscribe:
                    _log.Log($"Packet recieved requesting subscription to dataId={dataId}");
                    break;
                case SystemDataId.Unsubscribe:
                    _log.Log($"Packet recieved requesting unsubscription from dataId={dataId}");
                    break;
                case SystemDataId.ACK:
                    ushort ackedId = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(data, 0));
                    ushort ackedSeqNum = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(data, 2));
                    if (!outgoingUnACKed.Remove(new UnACKedPacket { DataId = ackedId, SeqNum = ackedSeqNum }))
                        _log.Log($"Unexected ACK recieved from ip={srcIP} with dataId={ackedId} and seqNum={ackedSeqNum}");
                    break;
                default: //Regular DataId
                    _router.Send(dataId, data);
                    break;
            }
        }

        private void SendAck(IPAddress srcIP, ushort dataId, ushort seqNum)
        {
            byte[] ackedId = BitConverter.GetBytes(IPAddress.NetworkToHostOrder((short)dataId));
            byte[] ackedSeqNum = BitConverter.GetBytes(IPAddress.NetworkToHostOrder((short)seqNum));
            byte[] data = new byte[4]; data.CopyTo(ackedId, 0); data.CopyTo(ackedSeqNum, ackedId.Length);
            SendPacket((ushort)SystemDataId.ACK, data, srcIP, false);
        }

        private enum SystemDataId : ushort
        {
            Null = 0,
            Ping = 1,
            PingReply = 2,
            Subscribe = 3,
            Unsubscribe = 4,
            ForceUnsubscribe = 5,
            ACK = 6
        }

        private struct UnACKedPacket { public ushort DataId; public ushort SeqNum; }
        private class PendingPing
        {
            public ushort SeqNum;
            public DateTime Timestamp;
            public System.Threading.SemaphoreSlim Semaphore;
            public TimeSpan RoundtripTime;
        }
    }
}