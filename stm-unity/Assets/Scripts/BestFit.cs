using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

public class BestFit : MonoBehaviour
{
	public static event Action ResolutionChange = delegate { };
	private Vector2 _previousResolution;

	private void Awake()
	{
		_previousResolution = new Vector2(Screen.width, Screen.height);
	}

	private void FixedUpdate()
	{
		if (_previousResolution.x != Screen.width || _previousResolution.y != Screen.height)
		{
			Invoke("CallResolutionChange", 0.05f);
			_previousResolution = new Vector2(Screen.width, Screen.height);
		}
	}

	private void CallResolutionChange()
	{
		ResolutionChange();
	}
}

public static class BestFitExtensions {
	public static void BestFit(this GameObject go)
	{
		BestFit(go.GetComponentsInChildren<Text>());
	}

	public static void BestFit(this List<Text> textObjects)
	{
		BestFit(textObjects.ToArray());
	}

	public static void BestFit(this IEnumerable<Text> textObjects)
	{
		BestFit(textObjects.ToArray());
	}

	public static void BestFit(this Text[] textObjects)
	{
		BestFit(textObjects.Select(text => text.gameObject).ToArray());
	}

	public static void BestFit(this List<GameObject> gameObjects)
	{
		BestFit(gameObjects.ToArray());
	}

	public static void BestFit(this IEnumerable<GameObject> gameObjects)
	{
		BestFit(gameObjects.ToArray());
	}

	public static void BestFit(this GameObject[] gameObjects)
	{
		int smallestFontSize = 0;
		foreach (var go in gameObjects) {
			LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)go.transform);
			
			var textObj = go.GetComponentsInChildren<Text>();
			foreach (var text in textObj)
			{
				text.resizeTextForBestFit = true;
				text.resizeTextMinSize = 1;
				text.resizeTextMaxSize = 25;//100;//text.size;
				text.cachedTextGenerator.Invalidate();
				text.cachedTextGenerator.Populate(text.text, text.GetGenerationSettings(text.rectTransform.rect.size));
				text.resizeTextForBestFit = false;
				var newSize = text.cachedTextGenerator.fontSizeUsedForBestFit;
				var newSizeRescale = text.rectTransform.rect.size.x / text.cachedTextGenerator.rectExtents.size.x;
				if (text.rectTransform.rect.size.y / text.cachedTextGenerator.rectExtents.size.y < newSizeRescale)
				{
					newSizeRescale = text.rectTransform.rect.size.y / text.cachedTextGenerator.rectExtents.size.y;
				}
				newSize = Mathf.FloorToInt(newSize * newSizeRescale);
				if (newSize < smallestFontSize || smallestFontSize == 0)
				{
					smallestFontSize = newSize;
				}
			}
		}
		foreach (var go in gameObjects)
		{
			var textObj = go.GetComponentsInChildren<Text>();
			foreach (var text in textObj)
			{
				text.fontSize = smallestFontSize;
			}
		}
	}
}
