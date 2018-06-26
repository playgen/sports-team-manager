﻿using System;
using System.Collections.Generic;
using System.Linq;

using PlayGen.Unity.Utilities.Extensions;

using UnityEngine;
using UnityEngine.UI;
using Avatar = PlayGen.RAGE.SportsTeamManager.Simulation.Avatar;

/// <summary>
/// Used to layout displayed avatars
/// </summary>
public class AvatarDisplay : MonoBehaviour
{
	private const byte _eyebrowAlpha = 128;
	private const float _maleOffsetPercent = 18f;
	private static Dictionary<string, Sprite> avatarSprites;
	private float _lastMood;
	private Image _body;
	private Image _hairBack;
	private Image _hairFront;
	private Image _eyebrow;
	private Image _nose;
	private Image _mouth;
	private Image _teeth;
	private Image _eyes;
	private Image _eyePupils;
	private Image _outfit;
	private Image _outfitHighlight;
	private Image _outfitShadow;
	private RectTransform _spriteParent;

	private static readonly Color _veryGood = Color.green;
	private static readonly Color _good = new Color(0, 1, 0.5f);
	private static readonly Color _neutral = Color.cyan;
	private static readonly Color _bad = new Color(1, 0.5f, 0);
	private static readonly Color _veryBad = Color.red;

	private readonly string _avatarImagePrefix = "AvatarSprites";
	private string _avatarPrefix => "IconMask/" + _avatarImagePrefix;
	/// <summary>
	/// Load all avatar sprites from resources.
	/// </summary>
	public static void LoadSprites()
	{
		avatarSprites = Resources.LoadAll(string.Empty, typeof(Sprite)).Cast<Sprite>().ToDictionary(a => a.name, a => a, StringComparer.OrdinalIgnoreCase);
	}

	public static Color MoodColor(float value)
	{
		var moodColor = _neutral;
		if (value > 2)
		{
			moodColor = _veryGood;
		}
		else if (value > 0)
		{
			moodColor = _good;
		}
		else if (value < -2)
		{
			moodColor = _veryBad;
		}
		else if (value < 0)
		{
			moodColor = _bad;
		}
		return moodColor;
	}

	/// <summary>
	/// Load the images for each part of the avatar.
	/// </summary>
	public void SetAvatar(Avatar avatar, float mood, bool isIcon = false)
	{
		if (!_body)
		{
			_spriteParent = transform.FindRect(_avatarPrefix) ?? transform.FindRect(_avatarImagePrefix);

			_body = GetAvatarImage("Body");
			_hairBack = GetAvatarImage("HairBack");
			_hairFront = GetAvatarImage("HairFront");
			_eyebrow = GetAvatarImage("Eyebrows");
			_nose = GetAvatarImage("Nose");
			_mouth = GetAvatarImage("Mouth");
			_teeth = GetAvatarImage("Teeth");
			_eyes = GetAvatarImage("Eyes");
			_eyePupils = GetAvatarImage("Eye Pupils");
			_outfit = GetAvatarImage("Outfit");
			_outfitHighlight = GetAvatarImage("OutfitHighlight");
			_outfitShadow = GetAvatarImage("OutfitShadow");
		}
		_body.sprite = avatarSprites[avatar.BodyType];
		_outfit.sprite = avatarSprites[avatar.OutfitBaseType];
		if (avatarSprites.ContainsKey(avatar.OutfitHighlightType))
		{
			_outfitHighlight.sprite = avatarSprites[avatar.OutfitHighlightType];
		}
		if (avatarSprites.ContainsKey(avatar.OutfitShadowType))
		{
			_outfitShadow.sprite = avatarSprites[avatar.OutfitShadowType];
		}
		_nose.sprite = avatarSprites[avatar.NoseType];
		_hairBack.sprite = avatarSprites[$"{avatar.HairType}_Back"];
		_hairFront.sprite = avatarSprites[$"{avatar.HairType}_Front"];

		// Set colors
		var skinColor = new Color32(avatar.SkinColor.R, avatar.SkinColor.G, avatar.SkinColor.B, 255);
		var eyeColor = new Color32(avatar.EyeColor.R, avatar.EyeColor.G, avatar.EyeColor.B, 255);
		var hairColor = new Color32(avatar.HairColor.R, avatar.HairColor.G, avatar.HairColor.B, 255);
		var eyebrowColor = new Color32(avatar.HairColor.R, avatar.HairColor.G, avatar.HairColor.B, _eyebrowAlpha);

		var primary = new Color32(avatar.PrimaryOutfitColor.R, avatar.PrimaryOutfitColor.G, avatar.PrimaryOutfitColor.B, avatar.PrimaryOutfitColor.A);
		var secondary = new Color32(avatar.SecondaryOutfitColor.R, avatar.SecondaryOutfitColor.G, avatar.SecondaryOutfitColor.B, avatar.SecondaryOutfitColor.A);

		_body.color = skinColor;
		_nose.color = skinColor;

		_mouth.color = avatar.IsMale ? hairColor : (Color32)Color.white;
		_eyePupils.color = eyeColor;
		_hairFront.color = hairColor;
		_hairBack.color = hairColor;
		_eyebrow.color = eyebrowColor;


		// Check the current outfit is not the casual one, we should not be changing the color of casual
		if (avatar.CustomOutfitColor)
		{
			_outfit.color = primary;
			_outfitHighlight.color = secondary;
		}
		else
		{
			_outfit.color = Color.white;
			_outfitHighlight.color = Color.white;
		}

		_outfitHighlight.gameObject.Active(avatar.CustomOutfitColor);
		_outfitShadow.gameObject.Active(avatar.CustomOutfitColor);

		if (isIcon)
		{
			SetIconProperties(avatar);	
		}
		else
		{
			SetFullBodyProperties(avatar);
		}
		//update avatar facial expression
		UpdateMood(avatar, mood);
	}

	/// <summary>
	/// update the displayed avatar to currently stored values (usually used to switch between causal and non-causal outfits)
	/// </summary>
	public void UpdateAvatar(Avatar avatar, bool isIcon = false)
	{
		SetAvatar(avatar, _lastMood, isIcon);
	}

	/// <summary>
	/// Get the an image from the avatar, checks which version is currently shown, masked or normal
	/// </summary>
	private Image GetAvatarImage(string image)
	{
		return transform.FindImage(_avatarPrefix + "/" + image) ?? transform.FindImage(_avatarImagePrefix + "/" + image);
	}

	/// <summary>
	/// update the avatar's facial expression based on their agreement with the statement passed to them
	/// </summary>
	/// //TODO move the mapping to a config
	public void UpdateMood(Avatar avatar, string reaction)
	{
		switch (reaction.Replace(" ", string.Empty))
		{
			case "StrongAgree":
				UpdateMood(avatar, 3);
				return;
			case "Agree":
				UpdateMood(avatar, 1);
				return;
			case "Disagree":
				UpdateMood(avatar, -1);
				return;
			case "StrongDisagree":
				UpdateMood(avatar, -3);
				return;
			default:
				UpdateMood(avatar, 0);
				break;
		}
	}

	/// <summary>
	/// update the avatar's facial expression based on the mood value provided
	/// </summary>
	public void UpdateMood(Avatar avatar, float mood)
	{
		var moodStr = "Neutral";
		if (mood > 2)
		{
			moodStr = "StronglyAgree";
		}
		else if (mood > 0)
		{
			moodStr = "Agree";
		}
		else if (mood < -2)
		{
			moodStr = "StronglyDisagree";
		}
		else if (mood < 0)
		{
			moodStr = "Disagree";
		}
		if (avatarSprites.ContainsKey($"{avatar.EyeType}_Brown_{moodStr}"))
		{
			_eyes.sprite = avatarSprites[$"{avatar.EyeType}_Brown_{moodStr}"];
		}
		else if (avatarSprites.ContainsKey($"{avatar.EyeType}_{moodStr}"))
		{
			_eyes.sprite = avatarSprites[$"{avatar.EyeType}_{moodStr}"];
		}
		else
		{
			_eyes.sprite = avatarSprites[$"{avatar.EyeType}_Brown_Neutral"];
		}
		_eyePupils.sprite = _eyes.sprite.name.Contains("Brown") ? avatarSprites.ContainsKey(_eyes.sprite.name.Replace("Brown", "Pupil")) ? avatarSprites[_eyes.sprite.name.Replace("Brown", "Pupil")] : null : null;
		_eyebrow.sprite = avatarSprites[$"{avatar.EyebrowType}_{moodStr}"];
		_mouth.sprite = avatarSprites[$"{avatar.MouthType}_{moodStr}"];
		_teeth.sprite = avatarSprites.ContainsKey($"{avatar.TeethType}_{moodStr}") ? avatarSprites[$"{avatar.TeethType}_{moodStr}"] : null;
		_eyePupils.enabled = _eyePupils.sprite != null;
		_teeth.enabled = _teeth.sprite != null;
		_lastMood = mood;
	}

	/// <summary>
	/// Setup avatar properties that are only common in icons
	/// </summary>
	private void SetIconProperties(Avatar a)
	{
		if (!_spriteParent)
		{
			return;
		}
		_spriteParent.offsetMax = a.IsMale ? new Vector2(_spriteParent.offsetMax.x, -1f * (_spriteParent.rect.height / _maleOffsetPercent)) : new Vector2(_spriteParent.offsetMax.x, 0);
	}

	/// <summary>
	/// Setup avatar properties that are only common in full body avatars
	/// </summary>
	private void SetFullBodyProperties(Avatar a)
	{
		_spriteParent.localScale = new Vector3(a.Weight, a.Height, 1f);
	}
}