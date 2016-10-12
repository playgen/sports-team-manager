//#define USE_SPRITESHEET
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
	/// Load the images for each part of the avatar.
	/// </summary>
	public void SetAvatar(Avatar avatar, float mood, bool isIcon = false)
	{
#if USE_SPRITESHEET
		LoadDictionary();
#endif
		// HACK: Just load the images from resources 
		_body.sprite = Resources.Load<Sprite>(string.Format("Avatars/Body/{0}", avatar.BodyType));
		_outfit.sprite = Resources.Load<Sprite>(string.Format("Avatars/Outfit/{0}", avatar.OutfitBaseType));
		_outfitHighlight.sprite = Resources.Load<Sprite>(string.Format("Avatars/Outfit/{0}", avatar.OutfitHighlightType));
		_outfitShadow.sprite = Resources.Load<Sprite>(string.Format("Avatars/Outfit/{0}", avatar.OutfitShadowType));
		_nose.sprite = Resources.Load<Sprite>(string.Format("Avatars/Head/{0}", avatar.NoseType));
		_hairBack.sprite = Resources.Load<Sprite>(string.Format("Avatars/Hair/{0}_Back", avatar.HairType));
		_hairFront.sprite = Resources.Load<Sprite>(string.Format("Avatars/Hair/{0}_Front", avatar.HairType));

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
		switch (reaction.Replace(" ", ""))
		{
			case "StronglyAgree":
				UpdateMood(avatar, 4);
				return;
			case "Agree":
				UpdateMood(avatar, 2);
				return;
			case "Disagree":
				UpdateMood(avatar, -2);
				return;
			case "StronglyDisagree":
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
			moodStr = "StronglyDisAgree";
		}
		else if (mood <= -1)
		{
			moodStr = "Disagree";
		}
		_eyes.sprite = Resources.Load<Sprite>(string.Format("Avatars/Head/{0}_{1}_{2}", avatar.EyeType, avatar.EyeColor, moodStr));
		_eyes.sprite = _eyes.sprite ?? Resources.Load<Sprite>(string.Format("Avatars/Head/{0}_{1}", avatar.EyeType, moodStr));
		_eyes.sprite = _eyes.sprite ?? Resources.Load<Sprite>(string.Format("Avatars/Head/{0}_{1}_Neutral", avatar.EyeType, avatar.EyeColor));

		_eyebrow.sprite = Resources.Load<Sprite>(string.Format("Avatars/Head/{0}_{1}", avatar.EyebrowType, moodStr));
		_mouth.sprite = Resources.Load<Sprite>(string.Format("Avatars/Head/{0}_{1}", avatar.MouthType, moodStr));
		_teeth.sprite = Resources.Load<Sprite>(string.Format("Avatars/Head/{0}_{1}", avatar.TeethType, moodStr));
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
		GetComponent<RectTransform>().localScale = new Vector3(a.Weight, a.Height, 1f);
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
