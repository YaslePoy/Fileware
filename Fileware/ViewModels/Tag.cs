using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Media;
using ReactiveUI;

namespace Fileware.ViewModels;

public class Tag : ReactiveObject
{
    private static readonly Dictionary<string, Tag> ReleasedTags = new();

    public string Name { get; set; }

    public IBrush Color
    {
        get => new SolidColorBrush(TagColorService.GetColorByString(Name));
        set
        {
            if (TagColorService.ColorPreferences.ContainsKey(Name))
                TagColorService.ColorPreferences[Name] = (value as SolidColorBrush).Color;
            else
                TagColorService.ColorPreferences.Add(Name, (value as SolidColorBrush).Color);
            this.RaisePropertyChanged();
        }
    }

    public ICommand ChangeColor => ReactiveCommand.Create(() =>
    {
        AppContext.CurrentMultiLevelView.MakeTopLevel("RecolorTag", this);
    });

    public static Tag FromName(string name)
    {
        if (ReleasedTags.TryGetValue(name, out var tag))
            return tag;

        var creating = new Tag { Name = name };
        ReleasedTags.Add(name, creating);
        return creating;
    }
}

public class SkipTag : Tag
{
}