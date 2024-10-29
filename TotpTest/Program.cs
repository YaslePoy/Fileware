// See https://aka.ms/new-console-template for more information

using OtpNet;

var key = "LDOSWWALOFXGXWNXXYBN";
var totp = new Totp(Base32Encoding.ToBytes(key));
while (true)
{
    // var str = Console.ReadLine();
    Console.WriteLine(totp.VerifyTotp(Console.ReadLine(), out long x));
    Console.WriteLine(totp.ComputeTotp());
    Thread.Sleep(500);
}