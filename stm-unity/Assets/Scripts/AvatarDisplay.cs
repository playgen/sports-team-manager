using System;
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
	private static readonly Dictionary<AvatarMood, int> AvatarMoodMapping = new Dictionary<AvatarMood, int>
	{
		{AvatarMood.StronglyAgree, 3},
		{AvatarMood.Agree, 1},
		{AvatarMood.Neutral, 0},
		{AvatarMood.Disagree, -1},
		{AvatarMood.StronglyDisagree, -3}
	};

	private static readonly Color _veryGood = Color.green;
	private static readonly Color _good = new Color(0, 1, 0.5f);
	private static readonly Color _neutral = Color.cyan;
	private static readonly Color _bad = new Color(1, 0.5f, 0);
	private static readonly Color _veryBad = Color.red;
	private const byte _eyebrowAlpha = 128;
	private const float _maleOffsetPercent = 15.5f;
	private readonly string _avatarPrefix = "AvatarSprites";
	private string _avatarIconPrefix => "IconMask/" + _avatarPrefix;

	private static Dictionary<string, Sprite> avatarSprites;
	private AvatarMood _lastMood;
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
	private bool _isIcon;

	/// <summary>
	/// Load all avatar sprites from resources.
	/// </summary>
	public static void LoadSprites()
	{
		avatarSprites = Resources.LoadAll("Avatars", typeof(Sprite)).Cast<Sprite>().ToDictionary(a => a.name, a => a, StringComparer.OrdinalIgnoreCase);
	}

	public void SetAvatar(Avatar avatar, float mood)
	{
		SetAvatar(avatar);
		UpdateMood(avatar, mood);
	}

	public void SetAvatar(Avatar avatar, AvatarMood mood)
	{
		SetAvatar(avatar);
		UpdateMood(avatar, mood);
	}

	/// <summary>
	/// Load the images for each part of the avatar.
	/// </summary>
	private void SetAvatar(Avatar avatar)
	{
		if (!_body)
		{
			_spriteParent = transform.FindRect(_avatarIconPrefix);
			_isIcon = true;
			if (!_spriteParent)
			{
				_spriteParent = transform.FindRect(_avatarPrefix);
				_isIcon = false;
			}

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

		_mouth.color = avatar.IsMale ? skinColor : (Color32)Color.white;
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

		if (_isIcon)
		{
			SetIconProperties(avatar);	
		}
		else
		{
			SetFullBodyProperties(avatar);
		}
	}

	public static AvatarMood GetMood(float mood)
	{
		if (Mathf.Approximately(mood, AvatarMoodMapping[AvatarMood.Neutral]))
		{
			return AvatarMood.Neutral;
		}
		if (mood > AvatarMoodMapping[AvatarMood.Neutral])
		{
			// Positive
			return mood < AvatarMoodMapping[AvatarMood.StronglyAgree] ? AvatarMood.Agree : AvatarMood.StronglyAgree;
		}
		// Negative
		return mood > AvatarMoodMapping[AvatarMood.StronglyDisagree] ? AvatarMood.Disagree : AvatarMood.StronglyDisagree;
	}

	public static Color MoodColor(AvatarMood avatarMood)
	{
		switch (avatarMood)
		{
			case AvatarMood.StronglyAgree:
				return _veryGood;
			case AvatarMood.Agree:
				return _good;
			case AvatarMood.Disagree:
				return _bad;
			case AvatarMood.StronglyDisagree:
				return _veryBad;
			default:
				return _neutral;
		}
	}

	public static Color MoodColor(float avatarMood)
	{
		return MoodColor(GetMood(avatarMood));
	}

	/// <summary>
	/// update the displayed avatar to currently stored values (usually used to switch between causal and non-causal outfits)
	/// </summary>
	public void UpdateAvatar(Avatar avatar)
	{
		SetAvatar(avatar, _lastMood);
	}

	/// <summary>
	/// update the avatar's facial expression based on their agreement with the statement passed to them
	/// </summary>
	public void UpdateMood(Avatar avatar, string reaction)
	{
		UpdateMood(avatar, (AvatarMood)Enum.Parse(typeof(AvatarMood), reaction));
	}

	/// <summary>
	/// update the avatar's facial expression based on their agreement with the statement passed to them
	/// </summary>
	public void UpdateMood(Avatar avatar, float mood)
	{
		var moodStr = GetMood(mood);
		UpdateMood(avatar, moodStr);
	}

	/// <summary>
	/// update the avatar's facial expression based on the mood value provided
	/// </summary>
	public void UpdateMood(Avatar avatar, AvatarMood mood)
	{
		var moodStr = mood.ToString();

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
			// No disagree eyes, so default to neutral
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
	/// Get the an image from the avatar, checks which version is currently shown, masked or normal
	/// </summary>
	private Image GetAvatarImage(string image)
	{
		return _spriteParent.FindImage(image);
	}

	/// <summary>
	/// Setup avatar properties that are only common in icons
	/// </summary>
	private void SetIconProperties(Avatar a)
	{
		if (_spriteParent)
		{
			_spriteParent.offsetMax = a.IsMale ? new Vector2(_spriteParent.offsetMax.x, -1f * (_spriteParent.rect.height / _maleOffsetPercent)) : new Vector2(_spriteParent.offsetMax.x, 0);
		}
	}

	/// <summary>
	/// Setup avatar properties that are only common in full body avatars
	/// </summary>
	private void SetFullBodyProperties(Avatar a)
	{
		_spriteParent.localScale = new Vector3(a.Weight, a.Height, 1f);
	}
}