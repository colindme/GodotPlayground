using Godot;

public static class SignalExtensions
{
    /// <summary>
    /// Attempts to connect a call to a signal if a connection does not already exist
    /// </summary>
    /// <returns>Whether or not a signal connection was made</returns>
    public static bool TryConnect(GodotObject objectToConnectTo, StringName signalName, Callable call)
    {
        if (objectToConnectTo == null)
        {
            GD.PrintErr("Failed to connect because passed in object was null");
            return false;
        }
        else if (signalName == null)
        {
            GD.PrintErr("Failed to connect because passed in signal name was null");
            return false;
        }

        if (!objectToConnectTo.IsConnected(signalName, call))
        {
            objectToConnectTo.Connect(signalName, call);
            return true;
        }

        return false;
    }

    public static bool TryDisconnect(GodotObject objectToDisconnectFrom, StringName signalName, Callable call)
    {
        if (objectToDisconnectFrom == null)
        {
            GD.PrintErr("Failed to disconnect because passed in object was null");
            return false;
        }
        else if (signalName == null)
        {
            GD.PrintErr("Failed to disconnect because passed in signal name was null");
            return false;
        }

        if (objectToDisconnectFrom.IsConnected(signalName, call))
        {
            objectToDisconnectFrom.Disconnect(signalName, call);
        }
        return false;
    }
}
