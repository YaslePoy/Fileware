using System;
using System.Drawing;

namespace Fileware.Models;

public class FileData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Version { get; set; }
    public DateTime LastChange { get; set; }
    public long Size { get; set; }
    public DateTime LoadTime { get; set; }
    public string FileType { get; set; }

    public string SizeFormatted
    {
        get
        {
            if (Size <= 1024)
                return Size + " B";
            if (Size <= 1024 * 1024)
                return Math.Round(Size / 1024d, 1) + " KB";
            if (Size <= 1024 * 1024 * 1024)
                return Math.Round(Size / 1024d / 1024d, 1) + " MB";

            return Math.Round(Size / 1024d / 1024d / 1024d, 1) + " GB";
        }
    }
}