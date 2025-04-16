using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Fileware.Controls;
using Fileware.ViewModels;

namespace Fileware.Models;

public class FileData : INotifyPropertyChanged, ITagContainer
{
    private string _name;
    private IImage? _preview;
    private byte[]? _previewData;
    public int Id { get; set; }
    private List<Tag> _tags = [];

    public int PointId { get; set; } = -1;
    public WrapPanel? TagsPreviewPanel { get; set; }

    public void UpdateTagPanel()
    {
        if (TagsPreviewPanel is null) return;
        TagsPreviewPanel.Children.Clear();
        foreach (var tag in _tags)
        {
            TagsPreviewPanel.Children.Add(new TagPoint { DataContext = tag });
        }
    }

    public List<Tag> Tags
    {
        get => _tags;
        set
        {
            if (Equals(value, _tags)) return;
            _tags = value;
            UpdateTagPanel();

            if (PointId != -1)
            {
            }
            
            OnPropertyChanged(nameof(Tags));
            OnPropertyChanged(nameof(HasTags));
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (value == _name) return;
            if (_name is not null && AppContext.LocalStoredFiles.TryGetValue(Id, out var meta))
            {
                File.Move(meta.Path, meta.Path.Replace(Name, value));
                meta.Path = meta.Path.Replace(Name, value);
            }

            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public bool HasTags => Tags.Count > 0;

    public int Version { get; set; }
    public DateTime LastChange { get; set; }
    public long Size { get; set; }
    public DateTime LoadTime { get; set; }
    public string FileType { get; set; }
    public bool UploadVisibility { get; set; }
    public bool DownloadVisibility { get; set; }
    public bool LoadProgressEnable { get; set; }
    public bool HasPreview { get; set; }

    public byte[]? PreviewData
    {
        get => _previewData;
        set
        {
            if (Equals(value, _previewData)) return;
            _previewData = value;
            OnPropertyChanged(nameof(PreviewData));
            OnPropertyChanged(nameof(Preview));
        }
    }

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
                // OnPropertyChanged(nameof(Preview));
                return _preview;
            }

            if (SuperPreview is null)
                return null;

            PreviewEffect = new BlurEffect { Radius = 10 };
            using var stream = new MemoryStream(SuperPreview);
            _preview = new Bitmap(stream);
            OnPropertyChanged(nameof(PreviewEffect));
            // OnPropertyChanged(nameof(Preview));
            return _preview;
        }
        set => _preview = value;
    }

    public byte[]? SuperPreview { get; set; }


    [ForeignKey("User")] public int UserId { get; set; }

    public int User { get; set; }

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

    public virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

public interface ITagContainer
{
    public int PointId { get; set; }
    public WrapPanel? TagsPreviewPanel { get; set; }

    public void UpdateTagPanel()
    {
        if (TagsPreviewPanel is not null)
        {
            TagsPreviewPanel.Children.Clear();
            foreach (var tag in Tags)
            {
                TagsPreviewPanel.Children.Add(new TagPoint { DataContext = tag });
            }
        }
    }

    public bool HasTags => Tags.Count > 0;
    List<Tag> Tags { get; set; }
}