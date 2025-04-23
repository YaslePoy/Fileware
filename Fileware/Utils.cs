using System;
using System.Linq;
using System.Windows.Input;
using ReactiveUI;

namespace Fileware;

public static class BasincExtentions
{
    public static T Also<T>(this T value, Action<T> operation)
    {
        operation(value);
        return value;
    }

    public static ICommand ToCommand(this Action action)
    {
        return ReactiveCommand.Create(action);
    }
}

public static class Utils
{
    public static E TransferData<E>(object basic)
    {
        var e = Activator.CreateInstance<E>();
        return TransferData(e, basic);
    }

    public static E TransferData<E>(E to, object from)
    {
        var entityType = typeof(E);
        var entityProps = entityType.GetProperties();
        foreach (var property in from.GetType().GetProperties())
        {
            var value = property.GetValue(from);
            var curr = entityProps.FirstOrDefault(i => i.Name == property.Name);
            if (!property.CanRead || !property.CanWrite) continue;
            
            if (curr == null)
                continue;

            curr.SetValue(to, value);

        }

        return to;
    }
}