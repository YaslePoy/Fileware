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

public class FileData : INotifyPropertyChanged, ITagContainer, ISearchable
{
    private int _lastTagCount = -1;
    private string _name;
    private IImage? _preview;
    private byte[]? _previewData;
    private List<Tag> _tags = [];
    private int _id;
    private int _version;
    private DateTime _lastChange;
    private long _size;
    private DateTime _loadTime;
    private string _fileType;
    private bool _uploadVisibility;
    private bool _downloadVisibility;
    private bool _loadProgressEnable;
    private bool _hasPreview;
    private IEffect _previewEffect;
    private byte[]? _superPreview;
    private int _userId;
    private int _user;
    private int _pointId = -1;
    private WrapPanel? _tagsPreviewPanel;

    public int Id
    {
        get => _id;
        set
        {
            if (value == _id) return;
            _id = value;
            OnPropertyChanged(nameof(Id));
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
            OnPropertyChanged(nameof(Name));
        }
    }

    public int Version
    {
        get => _version;
        set
        {
            if (value == _version) return;
            _version = value;
            OnPropertyChanged(nameof(Version));
        }
    }

    public DateTime LastChange
    {
        get => _lastChange;
        set
        {
            if (value.Equals(_lastChange)) return;
            _lastChange = value;
            OnPropertyChanged(nameof(LastChange));
        }
    }

    public long Size
    {
        get => _size;
        set
        {
            if (value == _size) return;
            _size = value;
            OnPropertyChanged(nameof(Size));
            OnPropertyChanged(nameof(SizeFormatted));
        }
    }

    public DateTime LoadTime
    {
        get => _loadTime;
        set
        {
            if (value.Equals(_loadTime)) return;
            _loadTime = value;
            OnPropertyChanged(nameof(LoadTime));
        }
    }

    public string FileType
    {
        get => _fileType;
        set
        {
            if (value == _fileType) return;
            _fileType = value;
            OnPropertyChanged(nameof(FileType));
        }
    }

    public bool UploadVisibility
    {
        get => _uploadVisibility;
        set
        {
            if (value == _uploadVisibility) return;
            _uploadVisibility = value;
            OnPropertyChanged(nameof(UploadVisibility));
        }
    }

    public bool DownloadVisibility
    {
        get => _downloadVisibility;
        set
        {
            if (value == _downloadVisibility) return;
            _downloadVisibility = value;
            OnPropertyChanged(nameof(DownloadVisibility));
        }
    }

    public bool LoadProgressEnable
    {
        get => _loadProgressEnable;
        set
        {
            if (value == _loadProgressEnable) return;
            _loadProgressEnable = value;
            OnPropertyChanged(nameof(LoadProgressEnable));
        }
    }

    public bool HasPreview
    {
        get => _hasPreview;
        set
        {
            if (value == _hasPreview) return;
            _hasPreview = value;
            OnPropertyChanged(nameof(HasPreview));
        }
    }

    public byte[]? PreviewData
    {
        get => _previewData;
        set
        {
            if (Equals(value, _previewData)) return;
            _previewData = value;
            OnPropertyChanged(nameof(PreviewData));
            OnPropertyChanged(nameof(Preview));
            OnPropertyChanged(nameof(PreviewData));
            OnPropertyChanged(nameof(Preview));
        }
    }

    public IEffect PreviewEffect
    {
        get => _previewEffect;
        set
        {
            if (Equals(value, _previewEffect)) return;
            _previewEffect = value;
            OnPropertyChanged(nameof(PreviewEffect));
        }
    }

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
        set
        {
            if (Equals(value, _preview)) return;
            _preview = value;
            OnPropertyChanged(nameof(Preview));
        }
    }

    public byte[]? SuperPreview
    {
        get => _superPreview;
        set
        {
            if (Equals(value, _superPreview)) return;
            _superPreview = value;
            OnPropertyChanged(nameof(SuperPreview));
            OnPropertyChanged(nameof(Preview));
        }
    }


    [ForeignKey("User")]
    public int UserId
    {
        get => _userId;
        set
        {
            if (value == _userId) return;
            _userId = value;
            OnPropertyChanged(nameof(UserId));
        }
    }

    public int User
    {
        get => _user;
        set
        {
            if (value == _user) return;
            _user = value;
            OnPropertyChanged(nameof(User));
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

    public bool IsSuits(string template)
    {
        return _name.Contains(template, StringComparison.CurrentCultureIgnoreCase);
    }

    public int PointId
    {
        get => _pointId;
        set
        {
            if (value == _pointId) return;
            _pointId = value;
            OnPropertyChanged(nameof(PointId));
        }
    }

    public WrapPanel? TagsPreviewPanel
    {
        get => _tagsPreviewPanel;
        set
        {
            if (Equals(value, _tagsPreviewPanel)) return;
            _tagsPreviewPanel = value;
            OnPropertyChanged(nameof(TagsPreviewPanel));
        }
    }

    public void UpdateTagPanel()
    {
        if (_lastTagCount == _tags.Count)
            return;
        _lastTagCount = _tags.Count;

        if (TagsPreviewPanel is null) return;

        TagsPreviewPanel.Children.Clear();
        foreach (var tag in _tags) TagsPreviewPanel.Children.Add(new TagPoint { DataContext = tag });

        Api.UpdateTags(PointId, _tags);
    }

    public List<Tag> Tags
    {
        get => _tags;
        set
        {
            if (Equals(value, _tags)) return;
            _tags = value;
            OnPropertyChanged(nameof(Tags));
            OnPropertyChanged(nameof(HasTags));
            UpdateTagPanel();

            OnPropertyChanged(nameof(Tags));
            OnPropertyChanged(nameof(HasTags));
        }
    }

    public bool HasTags => Tags.Count > 0;

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