// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Sockets;


if (Environment.GetCommandLineArgs()[1] == "share")
{
    var file = File.OpenRead(Environment.GetCommandLineArgs()[2]);
    var fi = new FileInfo(Environment.GetCommandLineArgs()[2]);
    var listener = new TcpListener(IPAddress.Any, 8000);

    listener.Start();
    var stream = listener.AcceptTcpClient().GetStream();
    stream.Write(BitConverter.GetBytes(fi.Length));
    file.CopyTo(stream);
    stream.ReadByte();
    listener.Stop();
    return;
}
else if (Environment.GetCommandLineArgs()[1] == "connect")
{
    var client = new TcpClient();
    client.Connect(IPEndPoint.Parse(Environment.GetCommandLineArgs()[2]));
    var len = new byte[4];
    var stream = client.GetStream();
    stream.ReadExactly(len);
    var file = File.Create(Environment.GetCommandLineArgs()[3]);
    var convertedLen = BitConverter.ToInt64(len);
    stream.CopyToAsync(file);

    while (stream.Position < convertedLen + 4)
    {
        Console.WriteLine($"Loading: {Math.Round((double)(stream.Position - 4) / (double)convertedLen * 100.0, 2),6}");
        Thread.Sleep(1000);
    }

    Console.WriteLine("Loaded!!!");
}