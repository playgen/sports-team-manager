using UnityEngine;

[System.Serializable]
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