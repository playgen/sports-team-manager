﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class TransformExtensions
{
	/// <summary>
	/// Find child with name provided, including inactive objects
	/// </summary>
	public static Transform FindInactive(this Transform parent, string name)
	{
		var trs = parent.GetComponentsInChildren<Transform>(true);
		foreach (var t in trs)
		{
			if (t.name == name && t.parent == parent)
			{
				return t;
			}
		}
		return null;
	}

	/// <summary>
	/// Find all children with name provided, including inactive objects
	/// </summary>
	public static List<Transform> FindAll(this Transform parent, string name)
	{
		var found = new List<Transform>();
		var trs = parent.GetComponentsInChildren<Transform>(true);
		foreach (var t in trs)
		{
			if (t.name == name)
			{
				found.Add(t);
			}
		}
		return found;
	}

	/// <summary>
	/// Find if provided recttransform is currently visible within the provided rect
	/// </summary>
	public static bool IsRectTransformVisible(this RectTransform obj, RectTransform visibleRect)
	{
		var objCorners = new Vector3[4];
		obj.GetWorldCorners(objCorners);
		var isVisible = true;
		foreach (var corner in objCorners)
		{
		    if (!RectTransformUtility.RectangleContainsScreenPoint(visibleRect, corner, null))
		    {
		        isVisible = false;
		        break;
		    }
		}
		return isVisible;
	}

	public static RectTransform RectTransform(this Transform transform)
	{
		return (RectTransform)transform;
	}

	public static RectTransform RectTransform(this GameObject gameObject)
	{
		return gameObject.transform.RectTransform();
	}

	public static RectTransform RectTransform(this MonoBehaviour mono)
	{
		return mono.transform.RectTransform();
	}

	public static RectTransform FindRect(this Transform transform, string find)
	{
		return transform.Find(find).RectTransform();
	}

	public static T FindComponent<T>(this Transform transform, string find) where T : Component
	{
		var compTrans = transform.Find(find);
		return compTrans != null ? compTrans.GetComponent<T>() : null;
	}

	public static Image FindImage(this Transform transform, string find)
	{
		return transform.FindComponent<Image>(find);
	}

	public static Text FindText(this Transform transform, string find)
	{
		return transform.FindComponent<Text>(find);
	}

	public static Button FindButton(this Transform transform, string find)
	{
		return transform.FindComponent<Button>(find);
	}

	public static GameObject FindObject(this Transform transform, string find)
	{
		return transform.Find(find).gameObject;
	}

	public static T FindComponentInChildren<T>(this Transform transform, string find)
	{
		return transform.Find(find).GetComponentInChildren<T>();
	}
}