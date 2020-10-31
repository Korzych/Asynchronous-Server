using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



namespace ServerLibrary
{

    public class ServerEchoAPM : ServerEcho
    {
        public delegate void TransmissionDataDelegate(NetworkStream stream);
        public ServerEchoAPM(IPAddress IP, int port) : base(IP, port)  {}
        protected override void AcceptClient()
        {
            while (true)
            {
                TcpClient tcpClient = TcpListener.AcceptTcpClient();
                Stream = tcpClient.GetStream();
                TransmissionDataDelegate transmissionDelegate = new TransmissionDataDelegate(BeginDataTransmission);
                //callback style
                transmissionDelegate.BeginInvoke(Stream, TransmissionCallback, tcpClient);
                // async result style
                //IAsyncResult result = transmissionDelegate.BeginInvoke(Stream, null, null);
                ////operacje......
                //while (!result.IsCompleted) ;
                ////sprzątanie
            }
        }
        private void TransmissionCallback(IAsyncResult ar)
        {
            // sprzątanie
        }
        protected override void BeginDataTransmission(NetworkStream stream)
        {
            
            while (true)
            {
                try
                {
                    
                    string username, passwd;

                    byte[] buffer = new byte[Buffer_size];
                    int message_size = stream.Read(buffer, 0, Buffer_size);
                    if (Encoding.ASCII.GetString(buffer, 0, message_size) == "\r\n")
                    {
                        Console.WriteLine("halo");
                        message_size = stream.Read(buffer, 0, 1024);
                    }
                    string rec = Encoding.ASCII.GetString(buffer).Trim();
                    rec = rec.ToLower();
                    Console.WriteLine(rec);
                    if (rec.Contains("login"))
                    {
             
                        string msg = "Enter Login:\n \r";
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        message_size = stream.Read(buffer, 0, Buffer_size);
                        if (Encoding.ASCII.GetString(buffer, 0, message_size) == "\r\n")
                        {
                            message_size = stream.Read(buffer, 0, 1024);
                        }
                        username = Encoding.ASCII.GetString(buffer).Trim();
                        username = correct(username);
                        msg = "Enter Password:\n \r";
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        message_size = stream.Read(buffer, 0, Buffer_size);
                        
                        if (Encoding.ASCII.GetString(buffer, 0, message_size) == "\r\n")
                        {
                            message_size = stream.Read(buffer, 0, 1024);
                        }
                        passwd= Encoding.ASCII.GetString(buffer).Trim();
                        passwd = correct(passwd);
                        if (login(username, passwd))
                        {
                            msg = "Logged in\n \r";
                            stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                            if(username == "admin")
                            {
                                admin(stream);
                            }
                            else
                            {
                                user(stream);
                            }

                        }
                        else
                        {
                            msg = "Invalid login or password\n \r";
                            stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        }

                       
                    }
                    
                }
                catch (IOException e)
                {
                    Console.WriteLine("An exception occured. Abort called.");
                    break;
                }
            }
        }
        public override void Start()
        {
            StartListening();
            AcceptClient();

        }
        protected void admin(NetworkStream stream)
        {
            //LOGIN
            //PASSWORD
            //SPRAWDZANIE CZY USER JEST W PLIKU
            //SPRAWDZANIE CZY PASSWORD JEST W PLIKU
            //ODPOWIEDŹ
            //POLECENIA DOSTĘPU:
            //Liczba użytkowników 
            //DODAĆ użytkownika
            //USUŃ UŻytkownika
            //Logout
        }
        protected void user(NetworkStream stream)
        { 
            //POLECENIA DOSTĘPU:
            //Liczba użytkowników 
            //Logout
        }
        public bool login(string user, string passwd)
        {
            string msg;
            string line;
            int counter = 0;
            bool userOk = false;
            bool passOk = false;
            System.IO.StreamReader file = new System.IO.StreamReader(@"users.txt");
            Console.WriteLine("Finding user\n");

            while ((line = file.ReadLine()) != null)
            {
                if (line == user)
                {
                    userOk = true;
                    Console.WriteLine("found user");
                    break;
                }
                counter++;

            }
           
            file.Close();
            Console.WriteLine("Finding password\n");
           
            if (userOk)
            {
                int helper = 0;
                file = new System.IO.StreamReader(@"passwords.txt");
                while ((line = file.ReadLine()) != null)
                {
                    if (helper == counter && line == passwd)
                    {
                        Console.WriteLine("found password");
                        passOk = true;
                    }
                }
                file.Close();
            }
            
            if (userOk && passOk)
            {
                return true;
            }
            else
            {   
                return false;
            }
        }

        private static string correct(string input)
        {
            input =Regex.Replace(input, @"\0+", "");
            input = Regex.Replace(input, @"\r\n", "");
            return input;

        }


    }

}
