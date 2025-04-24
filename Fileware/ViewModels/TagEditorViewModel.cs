using System.Collections.Generic;
using Fileware.Models;
using ReactiveUI;

namespace Fileware.ViewModels;

public class TagEditorViewModel : ReactiveObject
{
    public string CurrentTagName { get; set; }
    public ITagContainer CurrentTagsOwner { get; set; }
    public List<string> AllTags { get; set; }
    
}