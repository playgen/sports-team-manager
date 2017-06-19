//#define USE_SPRITESHEET

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
	[SerializeField]
	private Image _body;
	[SerializeField]
	private Image _hairBack;
	[SerializeField]
	private Image _hairFront;
	[SerializeField]
	private Image _eyebrow;
	[SerializeField]
	private Image _nose;
	[SerializeField]
	private Image _mouth;
	[SerializeField]
	private Image _teeth;
	[SerializeField]
	private Image _eyes;
	[SerializeField]
	private Image _outfit;
	[SerializeField]
	private Image _outfitHighlight;
	[SerializeField]
	private Image _outfitShadow;
	[SerializeField]
	private RectTransform _spriteParent;
#if USE_SPRITESHEET
	Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
#endif

	/// <summary>
	/// Load all avatar sprites from resources.
	/// </summary>
	public static void LoadSprites()
	{
		avatarSprites = Resources.LoadAll(string.Empty, typeof(Sprite)).Cast<Sprite>().ToDictionary(a => a.name.ToLower(), a => a);
	}

	/// <summary>
	/// Load the images for each part of the avatar.
	/// </summary>
	public void SetAvatar(Avatar avatar, float mood, bool isIcon = false)
	{
#if USE_SPRITESHEET
		LoadDictionary();
#endif
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

		_outfitHighlight.gameObject.SetActive(avatar.CustomOutfitColor);
		_outfitShadow.gameObject.SetActive(avatar.CustomOutfitColor);

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
				UpdateMood(avatar, 4);
				return;
			case "Agree":
				UpdateMood(avatar, 2);
				return;
			case "Disagree":
				UpdateMood(avatar, -2);
				return;
			case "StrongDisagree":
				UpdateMood(avatar, -4);
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
		if (mood >= 3)
		{
			moodStr = "StronglyAgree";
		}
		else if (mood >= 1)
		{
			moodStr = "Agree";
		}
		else if (mood <= -3)
		{
			moodStr = "StronglyDisagree";
		}
		else if (mood <= -1)
		{
			moodStr = "Disagree";
		}
		if (avatarSprites.ContainsKey(string.Format("{0}_{1}_{2}", avatar.EyeType, avatar.EyeColor, moodStr).ToLower()))
		{
			_eyes.sprite = avatarSprites[string.Format("{0}_{1}_{2}", avatar.EyeType, avatar.EyeColor, moodStr).ToLower()];
		}
		else if (avatarSprites.ContainsKey(string.Format("{0}_{1}", avatar.EyeType, moodStr).ToLower()))
		{
			_eyes.sprite = avatarSprites[string.Format("{0}_{1}", avatar.EyeType, moodStr).ToLower()];
		}
		else
		{
			_eyes.sprite = avatarSprites[string.Format("{0}_{1}_Neutral", avatar.EyeType, avatar.EyeColor).ToLower()];
		}
		_eyebrow.sprite = avatarSprites[string.Format("{0}_{1}", avatar.EyebrowType, moodStr).ToLower()];
		_mouth.sprite = avatarSprites[string.Format("{0}_{1}", avatar.MouthType, moodStr).ToLower()];
		_teeth.sprite = avatarSprites.ContainsKey(string.Format("{0}_{1}", avatar.TeethType, moodStr).ToLower()) ? avatarSprites[string.Format("{0}_{1}", avatar.TeethType, moodStr).ToLower()] : null;
		_teeth.enabled = _teeth.sprite != null;
		_lastMood = mood;
	}

	/// <summary>
	/// Setup avatar properties that are only common in icons
	/// </summary>
	private void SetIconProperties(Avatar a)
	{
		if (_spriteParent == null)
			return;
		_spriteParent.offsetMax = a.IsMale ? new Vector2(_spriteParent.offsetMax.x, -1f * (_spriteParent.rect.height / _maleOffsetPercent)) : new Vector2(_spriteParent.offsetMax.x, 0);
	}

	/// <summary>
	/// Setup avatar properties that are only common in full body avatars
	/// </summary>
	private void SetFullBodyProperties(Avatar a)
	{
		((RectTransform)transform).localScale = new Vector3(a.Weight, a.Height, 1f);
	}

#if USE_SPRITESHEET
	// Code for texture atlas
	private void LoadDictionary()
	{
		var SpritesData = Resources.LoadAll<Sprite>("Head/SpriteSheet_Head");
		Sprites = new Dictionary<string, Sprite>();

		foreach (var s in SpritesData)
		{
			Sprites.Add(s.characterName, s);
		}
	}
	public Sprite GetSpriteByName(string spriteName)
	{
		return Sprites.ContainsKey(spriteName) ? Sprites[spriteName] : null;
	}
#endif
}
