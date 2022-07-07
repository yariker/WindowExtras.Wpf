using System;
using System.Windows;
using System.Windows.Input;

namespace WindowExtras.Wpf.Helpers;

internal static class CommandHelper
{
    internal static void Execute(ICommandSource source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var command = source.Command;
        if (command == null)
        {
            return;
        }

        var parameter = source.CommandParameter;
        var target = source.CommandTarget;

        if (command is RoutedCommand routed)
        {
            target ??= source as IInputElement;

            if (routed.CanExecute(parameter, target))
            {
                routed.Execute(parameter, target);
            }
        }
        else if (command.CanExecute(parameter))
        {
            command.Execute(parameter);
        }
    }

    internal static bool CanExecute(ICommandSource source)
    {        
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var command = source.Command;
        if (command == null)
        {
            return false;
        }

        var parameter = source.CommandParameter;
        var target = source.CommandTarget;

        if (command is RoutedCommand routed)
        {
            target ??= source as IInputElement;

            return routed.CanExecute(parameter, target);
        }

        return command.CanExecute(parameter);
    }
}
