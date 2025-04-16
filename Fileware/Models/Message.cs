using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Avalonia.Controls;
using Fileware.Controls;
using Fileware.ViewModels;

namespace Fileware.Models;

public sealed class Message : INotifyPropertyChanged, ITagContainer
{
    public int Id { get; set; }

    public string Text { get; set; }
    public DateTime Time { get; set; }
    public string FormattedTime => Time.ToString("t");

    [ForeignKey("User")] public int UserId { get; set; }

    public int User { get; set; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool HasTags => Tags.Count > 0;

    private List<Tag> _tags = [];

    public int PointId { get; set; } = -1;
    public WrapPanel? TagsPreviewPanel { get; set; }

    public void UpdateTagPanel()
    {
        if (TagsPreviewPanel is not null)
        {
            TagsPreviewPanel.Children.Clear();
            foreach (var tag in _tags)
            {
                TagsPreviewPanel.Children.Add(new TagPoint { DataContext = tag });
            }
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
            
            
            
            OnPropertyChanged(nameof(Tags));
            OnPropertyChanged(nameof(HasTags));
        }
    }
}