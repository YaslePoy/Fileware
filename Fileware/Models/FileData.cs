using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

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
    public bool UploadVisibility { get; set; }
    public bool DownloadVisibility { get; set; }
    public bool LoadProgressEnable { get; set; }
    public bool HasPreview { get; set; }
    public byte[]? PreviewData;
    public IEffect PreviewEffect { get; set; }

    public IImage Preview
    {
        get
        {
            if (PreviewData is not null)
            {
                using var fullPreview = new MemoryStream(PreviewData);
                _preview = new Bitmap(fullPreview);
                PreviewEffect = null;
                OnPropertyChanged(nameof(PreviewEffect));

                return _preview;
            }

            if (SuperPreview is null)
                return null;
            PreviewEffect = new BlurEffect { Radius = 10 };
            using var stream = new MemoryStream(SuperPreview);
            _preview = new Bitmap(stream);

            return _preview;
            
        }
        set => _preview = value;
    }

    public byte[] SuperPreview { get; set; }
    private IImage? _preview;


    [ForeignKey("User")] public int UserId { get; set; }

    public int User { get; set; }

    public void UpdateSyncState()
    {
        if (AppContext.LocalStoredFiles.TryGetValue(Id, out var path))
        {
            var fileInfo = new FileInfo(path.Path);
            if (fileInfo.LastWriteTime > path.LastChangeTime != UploadVisibility)
            {
                UploadVisibility = fileInfo.LastWriteTime > path.LastChangeTime;
                OnPropertyChanged(nameof(UploadVisibility));
            }

            if (fileInfo.LastWriteTime < LoadTime != DownloadVisibility)
            {
                DownloadVisibility = Version > path.Version;
                OnPropertyChanged(nameof(DownloadVisibility));
            }
        }
    }

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

    public static string SpeedFormatted(long bytesPerSecond)
    {
        if (bytesPerSecond <= 1024)
            return bytesPerSecond + " B/s";
        if (bytesPerSecond <= 1024 * 1024)
            return Math.Round(bytesPerSecond / 1024d, 1) + " KB/s";

        return Math.Round(bytesPerSecond / 1024d / 1024d, 1) + " MB/s";
    }
}