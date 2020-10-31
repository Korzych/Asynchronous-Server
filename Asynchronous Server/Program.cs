using ServerLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Asynchronous_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerEchoAPM serverEchoApm = new ServerEchoAPM( IPAddress.Parse("127.0.0.1"),8000);
            serverEchoApm.Start();
        }
    }
}
