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
}
