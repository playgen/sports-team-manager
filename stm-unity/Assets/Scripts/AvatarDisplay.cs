using System.Collections.Generic;
using System.Linq;
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

	/// <summary>
	/// Load all avatar sprites from resources.
	/// </summary>
	public static void LoadSprites()
	{
		avatarSprites = Resources.LoadAll(string.Empty, typeof(Sprite)).Cast<Sprite>().ToDictionary(a => a.name.ToLower(), a => a);
	}

	public static Color MoodColor(float value)
	{
		var moodColor = Color.cyan;
		if (value > 2)
		{
			moodColor = Color.green;
		}
		else if (value > 0)
		{
			moodColor = new Color(0, 1, 0.5f);
		}
		else if (value < -2)
		{
			moodColor = Color.red;
		}
		else if (value < 0)
		{
			moodColor = new Color(1, 0.5f, 0);
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
			_body = transform.FindImage("IconMask/AvatarSprites/Body") ?? transform.FindImage("AvatarSprites/Body");
			_hairBack = transform.FindImage("IconMask/AvatarSprites/HairBack") ?? transform.FindImage("AvatarSprites/HairBack");
			_hairFront = transform.FindImage("IconMask/AvatarSprites/HairFront") ?? transform.FindImage("AvatarSprites/HairFront");
			_eyebrow = transform.FindImage("IconMask/AvatarSprites/Eyebrows") ?? transform.FindImage("AvatarSprites/Eyebrows");
			_nose = transform.FindImage("IconMask/AvatarSprites/Nose") ?? transform.FindImage("AvatarSprites/Nose");
			_mouth = transform.FindImage("IconMask/AvatarSprites/Mouth") ?? transform.FindImage("AvatarSprites/Mouth");
			_teeth = transform.FindImage("IconMask/AvatarSprites/Teeth") ?? transform.FindImage("AvatarSprites/Teeth");
			_eyes = transform.FindImage("IconMask/AvatarSprites/Eyes") ?? transform.FindImage("AvatarSprites/Eyes");
			_eyePupils = transform.FindImage("IconMask/AvatarSprites/Eye Pupils") ?? transform.FindImage("AvatarSprites/Eye Pupils");
			_outfit = transform.FindImage("IconMask/AvatarSprites/Outfit") ?? transform.FindImage("AvatarSprites/Outfit");
			_outfitHighlight = transform.FindImage("IconMask/AvatarSprites/OutfitHighlight") ?? transform.FindImage("AvatarSprites/OutfitHighlight");
			_outfitShadow = transform.FindImage("IconMask/AvatarSprites/OutfitShadow") ?? transform.FindImage("AvatarSprites/OutfitShadow");
			_spriteParent = transform.FindRect("IconMask/AvatarSprites") ?? transform.FindRect("AvatarSprites");
		}
		_body.sprite = avatarSprites[avatar.BodyType.ToLower()];
		_outfit.sprite = avatarSprites[avatar.OutfitBaseType.ToLower()];
		if (avatarSprites.ContainsKey(avatar.OutfitHighlightType.ToLower()))
		{
			_outfitHighlight.sprite = avatarSprites[avatar.OutfitHighlightType.ToLower()];
		}
		if (avatarSprites.ContainsKey(avatar.OutfitShadowType.ToLower()))
		{
			_outfitShadow.sprite = avatarSprites[avatar.OutfitShadowType.ToLower()];
		}
		_nose.sprite = avatarSprites[avatar.NoseType.ToLower()];
		_hairBack.sprite = avatarSprites[string.Format("{0}_Back", avatar.HairType).ToLower()];
		_hairFront.sprite = avatarSprites[string.Format("{0}_Front", avatar.HairType).ToLower()];

		// Set colors
		_body.color = new Color32(avatar.SkinColor.R, avatar.SkinColor.G, avatar.SkinColor.B, 255);
		_nose.color = new Color32(avatar.SkinColor.R, avatar.SkinColor.G, avatar.SkinColor.B, 255);
		_mouth.color = avatar.IsMale ? new Color32(avatar.SkinColor.R, avatar.SkinColor.G, avatar.SkinColor.B, 255) : (Color32)Color.white;
		_eyePupils.color = new Color32(avatar.EyeColor.R, avatar.EyeColor.G, avatar.EyeColor.B, 255);
		_hairFront.color = new Color32(avatar.HairColor.R, avatar.HairColor.G, avatar.HairColor.B, 255);
		_hairBack.color = new Color32(avatar.HairColor.R, avatar.HairColor.G, avatar.HairColor.B, 255);
		_eyebrow.color = new Color32(avatar.HairColor.R, avatar.HairColor.G, avatar.HairColor.B, _eyebrowAlpha);

		var primary = new Color32(avatar.PrimaryOutfitColor.R, avatar.PrimaryOutfitColor.G, avatar.PrimaryOutfitColor.B, avatar.PrimaryOutfitColor.A);
		var secondary = new Color32(avatar.SecondaryOutfitColor.R, avatar.SecondaryOutfitColor.G, avatar.SecondaryOutfitColor.B, avatar.SecondaryOutfitColor.A);

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
	/// update the avatar's facial expression based on their agreement with the statement passed to them
	/// </summary>
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
		if (avatarSprites.ContainsKey(string.Format("{0}_{1}_{2}", avatar.EyeType, "Brown", moodStr).ToLower()))
		{
			_eyes.sprite = avatarSprites[string.Format("{0}_{1}_{2}", avatar.EyeType, "Brown", moodStr).ToLower()];
		}
		else if (avatarSprites.ContainsKey(string.Format("{0}_{1}", avatar.EyeType, moodStr).ToLower()))
		{
			_eyes.sprite = avatarSprites[string.Format("{0}_{1}", avatar.EyeType, moodStr).ToLower()];
		}
		else
		{
			_eyes.sprite = avatarSprites[string.Format("{0}_{1}_Neutral", avatar.EyeType, "Brown").ToLower()];
		}
		_eyePupils.sprite = _eyes.sprite.name.Contains("Brown") ? avatarSprites.ContainsKey(_eyes.sprite.name.Replace("Brown", "Pupil").ToLower()) ? avatarSprites[_eyes.sprite.name.Replace("Brown", "Pupil").ToLower()] : null : null;
		_eyebrow.sprite = avatarSprites[string.Format("{0}_{1}", avatar.EyebrowType, moodStr).ToLower()];
		_mouth.sprite = avatarSprites[string.Format("{0}_{1}", avatar.MouthType, moodStr).ToLower()];
		_teeth.sprite = avatarSprites.ContainsKey(string.Format("{0}_{1}", avatar.TeethType, moodStr).ToLower()) ? avatarSprites[string.Format("{0}_{1}", avatar.TeethType, moodStr).ToLower()] : null;
		_eyePupils.enabled = _eyePupils.sprite != null;
		_teeth.enabled = _teeth.sprite != null;
		_lastMood = mood;
	}

	/// <summary>
	/// Setup avatar properties that are only common in icons
	/// </summary>
	private void SetIconProperties(Avatar a)
	{
		if (_spriteParent == null)
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