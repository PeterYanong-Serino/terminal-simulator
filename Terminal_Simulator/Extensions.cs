using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Terminal_Simulator {
    public static class Extensions {
        public static TcpState GetState(this TcpClient tcpClient) {
            try {
                if (tcpClient.Connected) {

                    var foo = IPGlobalProperties.GetIPGlobalProperties()
                      .GetActiveTcpConnections()
                      .SingleOrDefault(x
                        => x.LocalEndPoint.Address.ToString() == IPEndPoint.Parse(tcpClient.Client.LocalEndPoint.ToString()).Address.ToString().Replace("::ffff:", "")
                        && x.LocalEndPoint.Port == IPEndPoint.Parse(tcpClient.Client.LocalEndPoint.ToString()).Port
                        && x.RemoteEndPoint.Address.ToString() == IPEndPoint.Parse(tcpClient.Client.RemoteEndPoint.ToString()).Address.ToString().Replace("::ffff:", "")
                        && x.RemoteEndPoint.Port == IPEndPoint.Parse(tcpClient.Client.RemoteEndPoint.ToString()).Port
                      );

                    return foo != null ? foo.State : TcpState.Unknown;
                }
                return TcpState.Unknown;
            }
            catch (System.Exception) {
                return TcpState.Unknown;
            }
        }
    }
}
