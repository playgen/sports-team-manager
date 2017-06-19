using System;

/// <summary>
/// Class used for managing information involved in post-race events
/// </summary>
[Serializable]
public class KeyValueMessage
{
    public string TypeName;
    public string MethodName;
    public object[] Additional;

    public KeyValueMessage(string type, string method)
    {
        TypeName = type;
        MethodName = method;
    }

    public KeyValueMessage (string type, string method, params object[] obj)
    {
        TypeName = type;
        MethodName = method;
        Additional = obj;
    }
}