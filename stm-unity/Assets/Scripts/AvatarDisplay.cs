//#define USE_SPRITESHEET

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.UI;
using PlayGen.RAGE.SportsTeamManager.Simulation;
using Avatar = PlayGen.RAGE.SportsTeamManager.Simulation.Avatar;

public class AvatarDisplay : MonoBehaviour
{
	public Image Body;
	public Image HairBack;
	public Image HairFront;
	public Image Eyebrow;
	public Image Nose;
	public Image Mouth;
    public Image Teeth;
    public Image Eyes;
	public Image Outfit;
	public Image OutfitHighlight;
	public Image OutfitShadow;

	public RectTransform SpriteParent;
#if USE_SPRITESHEET
	Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
#endif

	private const byte EyebrowAlpha = 125;
	private const float MaleOffsetPercent = 18f;

	/// <summary>
	/// Load the images for each part of the avatar.
	/// <para>
	/// Avatar must be setup using AvatarGenerator.SetAvatarConfiguration() prior to calling this
	/// </para>
	/// </summary>
	/// <param characterName="avatar">The avatar to show</param>
	public void SetAvatar(Avatar avatar, float mood, Color primary, Color secondary, bool isIcon = false)
	{
#if USE_SPRITESHEET
		LoadDictionary();
#endif
		// TODO reference the texture packed images
		// HACK: Just load the images from resources

		var moodStr = "StronglyDisagree";
		/*if (mood >= 3)
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
        }*/

        Body.sprite = Resources.Load<Sprite>(string.Format("Avatars/Body/{0}", avatar.BodyType));
		Outfit.sprite = Resources.Load<Sprite>(string.Format("Avatars/Outfit/{0}", avatar.OutfitBaseType));
		OutfitHighlight.sprite = Resources.Load<Sprite>(string.Format("Avatars/Outfit/{0}", avatar.OutfitHiglightType));
		OutfitShadow.sprite = Resources.Load<Sprite>(string.Format("Avatars/Outfit/{0}", avatar.OutfitShadowType));

		Eyes.sprite = Resources.Load<Sprite>(string.Format("Avatars/Head/{0}_{1}_{2}", avatar.EyeType, avatar.EyeColor, moodStr));
        if (Eyes.sprite == null)
        {
            Eyes.sprite = Resources.Load<Sprite>(string.Format("Avatars/Head/{0}_{1}", avatar.EyeType, moodStr));
            if (Eyes.sprite == null)
            {
                Eyes.sprite = Resources.Load<Sprite>(string.Format("Avatars/Head/{0}_{1}_Neutral", avatar.EyeType, avatar.EyeColor));
            }
        }

		Eyebrow.sprite = Resources.Load<Sprite>(string.Format("Avatars/Head/{0}_{1}", avatar.EyebrowType, moodStr));
		Mouth.sprite = Resources.Load<Sprite>(string.Format("Avatars/Head/{0}_{1}", avatar.MouthType, moodStr));
        Teeth.sprite = Resources.Load<Sprite>(string.Format("Avatars/Head/{0}_{1}", avatar.TeethType, moodStr));
        if (Teeth.sprite != null)
        {
            Teeth.color = Color.white;
        } else
        {
            Teeth.color = new Color(0, 0, 0, 0);
        }
        Nose.sprite = Resources.Load<Sprite>(string.Format("Avatars/Head/{0}", avatar.NoseType));

		HairBack.sprite = Resources.Load<Sprite>(string.Format("Avatars/Hair/{0}_Back", avatar.HairType));
		HairFront.sprite = Resources.Load<Sprite>(string.Format("Avatars/Hair/{0}_Front", avatar.HairType));

		// Set out colours
		Body.color = new Color32(avatar.SkinColor.R, avatar.SkinColor.G, avatar.SkinColor.B, 255);
		Nose.color = new Color32(avatar.SkinColor.R, avatar.SkinColor.G, avatar.SkinColor.B, 255);
		Mouth.color = avatar.IsMale ? new Color32(avatar.SkinColor.R, avatar.SkinColor.G, avatar.SkinColor.B, 255) : (Color32)Color.white;

		HairFront.color = new Color32(avatar.HairColor.R, avatar.HairColor.G, avatar.HairColor.B, 255);
		HairBack.color = new Color32(avatar.HairColor.R, avatar.HairColor.G, avatar.HairColor.B, 255);

		Eyebrow.color = new Color32(avatar.HairColor.R, avatar.HairColor.G, avatar.HairColor.B, EyebrowAlpha);

		// Check the current outfit is not the casual one, we should not be changing the colour of casual
		if (avatar.CustomOutfitColor)
		{
			Outfit.color = primary == Color.clear ? new Color32(avatar.PrimaryOutfitColor.R, avatar.PrimaryOutfitColor.G, avatar.PrimaryOutfitColor.B, 255) : (Color32)primary;
			OutfitHighlight.color = secondary == Color.clear ? new Color32(avatar.SecondaryOutfitColor.R, avatar.SecondaryOutfitColor.G, avatar.SecondaryOutfitColor.B, 255) : (Color32)secondary;
		}
		else
		{
			Outfit.color = Color.white;
		}

		OutfitHighlight.gameObject.SetActive(avatar.CustomOutfitColor);
		OutfitShadow.gameObject.SetActive(avatar.CustomOutfitColor);

		if (isIcon)
		{
			SetIconProperties(avatar);	
		}
		else
		{
			SetFullBodyProperties(avatar);
		}
	}

	/// <summary>
	/// Setup avatar properties that are only common in icons
	/// </summary>
	private void SetIconProperties(Avatar a)
	{
		if (SpriteParent == null)
			return;
		SpriteParent.offsetMax = a.IsMale ? new Vector2(SpriteParent.offsetMax.x, -1f * (SpriteParent.rect.height / MaleOffsetPercent)) : new Vector2(SpriteParent.offsetMax.x, 0);
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
