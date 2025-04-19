using System;
using System.Security.Cryptography.X509Certificates;

namespace Fileware.Models;

public class FileSpace
{
    public string Id { get; set; }

    public string Display
    {
        get
        {
            var extracted = Id[(Id.IndexOf(':')+1)..];
            
            if (extracted == "master")
            {
                extracted = "Мастер пространство";
            }

            return extracted;
        }
    }

    public static FileSpace OfUser(UserData user) => new() { Id = $"user_{user.Id}:master" };
}