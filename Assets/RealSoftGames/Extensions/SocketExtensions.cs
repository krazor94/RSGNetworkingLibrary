//Author: Jake Aquilina
//Creation Date: 28/06/20 01:44 AM
//Company: RealSoft Games
//Website: https://www.realsoftgames.com/

using System.Data;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Ping = System.Net.NetworkInformation.Ping;

namespace RealSoftGames.Network
{
    public static class SocketExtensions
    {
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch
            (SocketException)
            {
                return false;
            }
        }
    }
}