using RocketPlus.Utils;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RocketPlus.Services
{
    public class UnityClient : Singleton<UnityClient>
    {
        private Socket? unityClientSocket;

        public async Task Connect()
        {
            var ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), int.Parse("8080"));
            unityClientSocket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            await unityClientSocket.ConnectAsync(ipEndPoint);
        }

        public async void Send(string posture)
        {
            var messageBytes = Encoding.UTF8.GetBytes(posture);
            while (unityClientSocket == null)
                await Connect();
            await unityClientSocket.SendAsync(messageBytes, SocketFlags.None);
        }
    }
}
