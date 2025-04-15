namespace Fileware.Models;

public interface IMultiLevelView
{
    void MakeTopLevel(string key, object sender);
    void RemoveTopLevel(string key);
}