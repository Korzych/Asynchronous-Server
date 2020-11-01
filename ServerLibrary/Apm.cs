using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
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
        public List<string> loggedUsers = new List<string>();
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
                       // Console.WriteLine("halo");
                        message_size = stream.Read(buffer, 0, 1024);
                    }
                    string rec = Encoding.ASCII.GetString(buffer).Trim();
                    
                    rec = Correct(rec.ToLower());
                    Console.WriteLine(rec);
                    if (rec.Contains("login"))
                    {
             
                        string msg = "Enter Login:\n \r";
                        buffer = new byte[Buffer_size];
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        message_size = stream.Read(buffer, 0, Buffer_size);
                        if (Encoding.ASCII.GetString(buffer, 0, message_size) == "\r\n")
                        {
                            message_size = stream.Read(buffer, 0, Buffer_size);
                        }
                        username = Encoding.ASCII.GetString(buffer).Trim();
                        username = Correct(username);
                        buffer = new byte[Buffer_size];
                        msg = "Enter Password:\n \r";
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                        message_size = stream.Read(buffer, 0, Buffer_size);
                        
                        if (Encoding.ASCII.GetString(buffer, 0, message_size) == "\r\n")
                        {
                            message_size = stream.Read(buffer, 0, Buffer_size);
                        }
                        passwd= Encoding.ASCII.GetString(buffer).Trim();
                        passwd = Correct(passwd);
                        if (Login(username, passwd))
                        {
                            msg = "Logged in as "+username +" \n \r";
                            loggedUsers.Add(username);
                            stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                            if(username == "admin")
                            {
                                admin(stream, loggedUsers,passwd);
                                break;
                            }
                            else
                            {
                                User(stream, loggedUsers,username);
                                break;
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
        protected void admin(NetworkStream stream, List<string> loggedUsers,string password)
        {
            string msg;
            msg = "Welcome Administrator\n \rusers - List current users\n \radduser- Add user\n \rdeleteuser- Delete User\n \rlogout- Logout\n \r";
            stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
            while (true)
            {
                byte[] buffer = new byte[Buffer_size];
                int message_size = stream.Read(buffer, 0, Buffer_size);
                if (Encoding.ASCII.GetString(buffer, 0, message_size) == "\r\n")
                {
                    message_size = stream.Read(buffer, 0, Buffer_size);
                }
                string rec = Encoding.ASCII.GetString(buffer).Trim();
                rec = Correct(rec.ToLower());
                if (rec == "users")
                {
                    ListUsers(stream, loggedUsers);
                }
                else if (rec == "adduser")
                {

                    AddUser(stream);

                }
                else if (rec == "deleteuser")
                {
                    DeleteUser(password);
                }
                else if (rec == "logout")
                {
                    msg = "Bye\n \r ";
                    stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                    Logout("admin",loggedUsers);
                    break;
                }
                else
                {
                    msg = "Invalid Command\n \r";
                    stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                }
            }

            //POLECENIA DOSTĘPU:

            //Liczba użytkowników 
            //DODAĆ użytkownika
            //USUŃ UŻytkownika
            //Logout
        }
        protected void AddUser(NetworkStream stream)
        {
            string username;
            string msg = "Enter Username:\n \r";
            string passwd;
            byte[] buffer = new byte[Buffer_size];
            stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
            int message_size = stream.Read(buffer, 0, Buffer_size);
            if (Encoding.ASCII.GetString(buffer, 0, message_size) == "\r\n")
            {
                message_size = stream.Read(buffer, 0, Buffer_size);
            }
            username = Encoding.ASCII.GetString(buffer).Trim();
            username = Correct(username);
            buffer = new byte[Buffer_size];
            msg = "Enter Password:\n \r";
            stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
            message_size = stream.Read(buffer, 0, Buffer_size);

            if (Encoding.ASCII.GetString(buffer, 0, message_size) == "\r\n")
            {
                message_size = stream.Read(buffer, 0, Buffer_size);
            }
            passwd = Encoding.ASCII.GetString(buffer).Trim();
            passwd = Correct(passwd);
            if(FindUser(username)==-20)
            {
               // Console.WriteLine("User not found");
                using (StreamWriter sw = File.AppendText(@"users.txt"))
                {
                    //sw.WriteLine("\n");
                    sw.WriteLine(username);
                    sw.Close();
                }
                
                using (StreamWriter sw = File.AppendText(@"passwords.txt"))
                {
                   // sw.WriteLine("\n");
                    sw.WriteLine(passwd);
                    sw.Close();
                }

                msg = "User created\n \r";
                stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);

            }
            else
            {
                msg = "User already exists\n \r";
                stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
            }


            //TWORZENIE KONTA
        }
        protected void DeleteUser( string password)
        {
            string username;
            string msg = "Enter Username:\n \r";
            string passwd;
            byte[] buffer = new byte[Buffer_size];
            stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
            int message_size = stream.Read(buffer, 0, Buffer_size);
            if (Encoding.ASCII.GetString(buffer, 0, message_size) == "\r\n")
            {
                message_size = stream.Read(buffer, 0, Buffer_size);
            }
            username = Encoding.ASCII.GetString(buffer).Trim();
            username = Correct(username);
            if(username!="admin")
            {
                buffer = new byte[Buffer_size];
                msg = "Enter Admin Password To Confirm:\n \r";
                stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                message_size = stream.Read(buffer, 0, Buffer_size);
                if (Encoding.ASCII.GetString(buffer, 0, message_size) == "\r\n")
                {
                    message_size = stream.Read(buffer, 0, Buffer_size);
                }
                passwd = Encoding.ASCII.GetString(buffer).Trim();
                passwd = Correct(passwd);
                int usr = FindUser(username);
                if (FindUser(username) != -20 && passwd == password)
                {
                    //USUWANIE UŻYTKOWNIKA 
                    string[] readText = File.ReadAllLines(@"users.txt");

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"users.txt"))
                    {
                        for (int i = 0; i < readText.Length; i++)
                        {
                            if (i != usr)
                            {
                                file.WriteLine(readText[i]);
                            }
                        }
                        file.Close();
                    }
                    //USUWANIE HASŁA
                    string[] readPass = File.ReadAllLines(@"passwords.txt");
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"passwords.txt"))
                    {
                        for (int i = 0; i < readPass.Length; i++)
                        {
                            if (i != usr)
                            {
                                file.WriteLine(readPass[i]);
                            }
                        }
                        file.Close();
                        msg = "User Deleted\n \r";
                        stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                    }
                }
            }
            else
            {
                msg = "Permission Denied\n \r";
                stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
            }
           


        }
        protected void ListUsers(NetworkStream stream, List<string> loggedUsers)
        {
            string msg = "There are: " + loggedUsers.Count() + " logged users\r \n";
            stream.Write(Encoding.ASCII.GetBytes(msg), 0, (msg).Length);
            loggedUsers.ForEach(users => stream.Write(Encoding.ASCII.GetBytes(users+"\n\r"), 0, (users+"\n\r").Length));
        }
        protected int FindUser(string user)
        {
            int counter = 0;
            System.IO.StreamReader file = new System.IO.StreamReader(@"users.txt");
            string line;
            while ((line = file.ReadLine()) != null)
            {
                if (line == user)
                {
                    file.Close();
                   return  counter;                 
                }
                counter++;
            }
            file.Close();
            return -20;

        }
        protected void Logout(string username, List<string> loggedUsers)
        {
            loggedUsers.Remove(username);
        }
        protected void User(NetworkStream stream, List<string> loggedUsers, string username)
        {
            string msg;
            msg = "Welcome User\n \rrusers - List current users\n \rlogout- Logout\n \r ";
            stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
            while(true)
            {
                byte[] buffer = new byte[Buffer_size];
                int message_size = stream.Read(buffer, 0, Buffer_size);
                if (Encoding.ASCII.GetString(buffer, 0, message_size) == "\r\n")
                {
                    message_size = stream.Read(buffer, 0, Buffer_size);
                }
                string rec = Encoding.ASCII.GetString(buffer).Trim();
                rec = Correct(rec.ToLower());
                if (rec == "users")
                {
                    ListUsers(stream, loggedUsers);
                }
                else if (rec == "logout")
                {
                    Logout(username, loggedUsers);
                    msg = "Bye\n \r ";
                    stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                    break; 
                }
                else
                {
                    msg = "Invalid Command\n \r ";
                    stream.Write(Encoding.ASCII.GetBytes(msg), 0, msg.Length);
                }

            }
            
        }
        public bool Login(string user, string passwd)
        {
        
            string line;
            int counter = 0;
            bool userOk = false;
            bool passOk = false;
            System.IO.StreamReader file = new System.IO.StreamReader(@"users.txt");
            Console.WriteLine("Finding user\n"+user);

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
                    helper++;
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

        private static string Correct(string input)
        {
            input =Regex.Replace(input, @"\0+", "");
            input = Regex.Replace(input, @"\r\n", "");
            return input;

        }


    }

}
