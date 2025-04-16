namespace Fileware.Models;

public interface IMultiLevelView
{
    void MakeTopLevel(string key, object sender);
}