using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Avalonia.Controls;
using Fileware.Controls;
using Fileware.ViewModels;

namespace Fileware.Models;

public sealed class Message : INotifyPropertyChanged, ITagContainer, ISearchable
{
    private int _lastTagCount = -1;

    private List<Tag> _tags = [];
    public int Id { get; set; }

    public string Text { get; set; }
    public DateTime Time { get; set; }
    public string FormattedTime => Time.ToString("t");

    [ForeignKey("User")] public int UserId { get; set; }

    public int User { get; set; }
    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsSuits(string template)
    {
        return Text.Contains(template, StringComparison.CurrentCultureIgnoreCase);
    }

    public bool HasTags => Tags.Count > 0;

    public int PointId { get; set; } = -1;
    public WrapPanel? TagsPreviewPanel { get; set; }

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
            UpdateTagPanel();

            OnPropertyChanged(nameof(Tags));
            OnPropertyChanged(nameof(HasTags));
        }
    }

    public void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}