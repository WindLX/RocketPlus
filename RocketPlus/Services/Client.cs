﻿using RocketPlus.Utils;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RocketPlus.Services
{
    public class Client : Singleton<Client>
    {
        public event Action<string>? OnRecvMessage;
        public event Action<string>? OnRawMessage;
        public event Action<RocketConnectState>? OnConnectStateChange;

        private Socket? clientSocket;
        private Thread? ping;
        private Thread? recv;
        private CancellationTokenSource cts = new();

        private static async Task<IPEndPoint> GetIPAddress()
        {
            IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync("bemfa.com");
            IPAddress? iPAddress = ipHostInfo.AddressList.FirstOrDefault();
            if (iPAddress == null)
            {
                throw new Exception("DNS resolution failed!");
            }
            else
            {
                return new IPEndPoint(iPAddress, 8344);
            }
        }

        public async Task Connect()
        {
            var ipEndPoint = await GetIPAddress();
            clientSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            OnConnectStateChange?.Invoke(RocketConnectState.TryConnect);
            await clientSocket.ConnectAsync(ipEndPoint);
            OnConnectStateChange?.Invoke(RocketConnectState.Connecting);

            await Subscribe();
            
            OnRecvMessage?.Invoke("Socket has been opened!\r\n");
            
            if (cts.Token.IsCancellationRequested)
            {
                cts.TryReset();
            }

            ping = new(Ping) { IsBackground = true };
            ping.Start();
            
            recv = new(Recv) { IsBackground = true };
            recv.Start();
        }

        private async Task Subscribe()
        {
            if (clientSocket != null && clientSocket.Connected)
            {
                var message = "cmd=1&uid=97c541e615ab43918f0a1e1a64d400ed&topic=Rocket\r\n";
                var messageBytes = Encoding.UTF8.GetBytes(message);
                var n = await clientSocket.SendAsync(messageBytes, SocketFlags.None);
                if (n == 0)
                    OnRecvMessage?.Invoke($"Socket subscribe failed!\r\n");
            }
        }

        public async Task Emergency()
        {
            if (clientSocket != null && clientSocket.Connected)
            {
                try
                {
                    var message = "cmd=2&uid=97c541e615ab43918f0a1e1a64d400ed&topic=Rocket&msg=OPEN\n";
                    var messageBytes = Encoding.UTF8.GetBytes(message);
                    await clientSocket.SendAsync(messageBytes, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    OnRecvMessage?.Invoke($"Emergency failed! {ex.Message}\r\n");
                }
            }
        }

        //public async void SendTest()
        //{
        //    if (clientSocket != null)
        //    {
        //        var message = "cmd=2&uid=97c541e615ab43918f0a1e1a64d400ed&topic=Rocket&msg=UTC=101801.000,Temperature=19.400,Pressure=94987,Altitude=541.309,RollAngle=64.6,PitchAngle=17.9,YawAngle=148.2,AccX=-0.170,AccY=1.001,AccZ=1.708,AnguSpeX=-4.7,AnguSpeY=-0.3,AnguSpeZ=-0.9,Lat=,Lon=\r\n";
        //        var messageBytes = Encoding.UTF8.GetBytes(message);
        //        await Task.Run(() =>
        //        {
        //            Thread.Sleep(3000);
        //        });
        //        await clientSocket.SendAsync(messageBytes, SocketFlags.None);
        //    }
        //}

        private void Ping()
        {
            if (clientSocket != null && clientSocket.Connected)
            {
                var message = "ping\r\n";
                var messageBytes = Encoding.UTF8.GetBytes(message);
                while (true)
                {
                    try
                    {
                        if (cts.Token.IsCancellationRequested)
                            break;
                        Thread.Sleep(30000);
                        clientSocket.Send(messageBytes, SocketFlags.None);
                    }
                    catch (SocketException ex)
                    {
                        OnRecvMessage?.Invoke($"Ping failed! {ex.Message}\r\n");
                    }
                    catch (ThreadInterruptedException)
                    {
                        break;
                    }
                }
            }
        }

        private void Recv()
        {
            if (clientSocket != null && clientSocket.Connected)
            {
                while (true)
                {
                    try
                    {
                        byte[] buffer = new byte[1024];
                        var received = clientSocket.Receive(buffer, SocketFlags.None);
                        if (received != 0)
                        {
                            var response = Encoding.UTF8.GetString(buffer, 0, received);
                            OnRecvMessage?.Invoke(response);
                            OnRawMessage?.Invoke(response);
                        }
                    }
                    catch (SocketException)
                    {
                        break;
                    }
                }
            }
        }

        public async Task Close()
        {
            if (clientSocket != null && clientSocket.Connected)
            {
                await Task.Run(() =>
                {
                    OnConnectStateChange?.Invoke(RocketConnectState.TryDisconnect);
                    cts.Cancel();
                    if (ping?.ThreadState == (System.Threading.ThreadState.WaitSleepJoin | System.Threading.ThreadState.Background))
                    {
                        ping.Interrupt();
                    }
                    ping?.Join();
                    OnConnectStateChange?.Invoke(RocketConnectState.Disconnect);
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    recv?.Join();
                    OnRecvMessage?.Invoke("Socket has been closed!\r\n");
                });
            }
        }
    }
}
