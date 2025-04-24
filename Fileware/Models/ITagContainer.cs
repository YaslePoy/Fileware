using System.Collections.Generic;
using Avalonia.Controls;
using Fileware.Controls;
using Fileware.ViewModels;

namespace Fileware.Models;

public interface ITagContainer
{
    public int PointId { get; set; }
    public WrapPanel? TagsPreviewPanel { get; set; }

    public bool HasTags => Tags.Count > 0;
    List<Tag> Tags { get; set; }

    public void UpdateTagPanel()
    {
        if (TagsPreviewPanel is not null)
        {
            TagsPreviewPanel.Children.Clear();
            foreach (var tag in Tags) TagsPreviewPanel.Children.Add(new TagPoint { DataContext = tag });
        }
    }
}