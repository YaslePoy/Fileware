using System;

namespace Fileware.Models;

public class FileData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Version { get; set; }
    public DateTime LastChange { get; set; }
}