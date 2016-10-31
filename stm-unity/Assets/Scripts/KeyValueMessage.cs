using UnityEngine;

[System.Serializable]
public class KeyValueMessage
{
    public string TypeName;
    public string MethodName;
    [HideInInspector]
    public GameObject SourceObject;

    public KeyValueMessage(string type, string method)
    {
        TypeName = type;
        MethodName = method;
    }

    public KeyValueMessage (string type, string method, GameObject obj)
    {
        TypeName = type;
        MethodName = method;
        SourceObject = obj;
    }
}