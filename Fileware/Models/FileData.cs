using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Fileware.Models;

public class FileData : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Version { get; set; }
    public DateTime LastChange { get; set; }
    public long Size { get; set; }
    public DateTime LoadTime { get; set; }
    public string FileType { get; set; }
    public string Hash { get; set; }

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

    public event PropertyChangedEventHandler? PropertyChanged;

    public virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}