using System;
using System.Net.Sockets;
using System.Text;

namespace TheGameClient
{
    class Program
    {
        static TcpClient tcpClient = new TcpClient();
        static public void Main()
        {

            
            try
            {
                tcpClient.Connect("localhost", 10);
                NetworkStream networkStream = tcpClient.GetStream();

                if (networkStream.CanWrite && networkStream.CanRead)
                {

                    String DataToSend = "";

                    while (DataToSend != "quit")
                    {

                        Console.WriteLine("\nType a text to be sent:");
                        DataToSend = Console.ReadLine();
                        if (DataToSend.Length == 0) break;

                        
                        /*Byte[] sendBytes = Encoding.ASCII.GetBytes(System.IO.File.ReadAllText("C:\\Users\\Amir\\Desktop\\Clienttext.txt"));
                        for (int i=0; i< 100 ; i++)
                        {
                            networkStream.Write(sendBytes, 0, sendBytes.Length);
                            System.Threading.Thread.Sleep(100);
                        }*/

                        for (; ; )
                        {
                            Send(Encoding.ASCII.GetBytes("Hallo!"));
                            Send(Encoding.ASCII.GetBytes("Ich bin Amir, ich bin 19 jahre alt!"));
                            System.Threading.Thread.Sleep(5000);
                        }

                        // Reads the NetworkStream into a byte buffer.
                        byte[] bytes = new byte[tcpClient.ReceiveBufferSize];
                        int BytesRead = networkStream.Read(bytes, 0, (int)tcpClient.ReceiveBufferSize);

                        // Returns the data received from the host to the console.
                        string returndata = Encoding.ASCII.GetString(bytes, 0, BytesRead);
                        Console.WriteLine("This is what the host returned to you: \r\n{0}", returndata);
                    }
                    networkStream.Close();
                    tcpClient.Close();
                }
                else if (!networkStream.CanRead)
                {
                    Console.WriteLine("You can not write data to this stream");
                    tcpClient.Close();
                }
                else if (!networkStream.CanWrite)
                {
                    Console.WriteLine("You can not read data from this stream");
                    tcpClient.Close();
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Sever not available!");
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine("Sever not available!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }       // Main() 

        static void Send(byte[] buff)
        {
            tcpClient.Client.Send(buff);
        }
    }
}
