public class KeyValueMessage
{
    public string TypeName;
    public string MethodName;

    public KeyValueMessage()
    {
        
    }

    public KeyValueMessage (string type, string method)
    {
        TypeName = type;
        MethodName = method;
    }
}