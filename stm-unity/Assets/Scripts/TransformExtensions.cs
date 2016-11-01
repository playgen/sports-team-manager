using System.Collections.Generic;

using UnityEngine;

public static class TransformExtensions
{
    public static Transform FindInactive(this Transform parent, string name)
    {
        var trs = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in trs)
        {
            if (t.name == name && t.parent == parent)
            {
                return t;
            }
        }
        return null;
    }

    public static List<Transform> FindAll(this Transform parent, string name)
    {
        var found = new List<Transform>();
        var trs = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in trs)
        {
            if (t.name == name)
            {
                found.Add(t);
            }
        }
        return found;
    }
}
