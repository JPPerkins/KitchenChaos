using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SoundManager : MonoBehaviour
{

	private const string PLAYER_PREFS_SOUND_EFFECTS_VOLUME = "SoundEffectsVolume";
	public static SoundManager Instance { get; private set; }
	
	[SerializeField] private AudioClipRefsSO _audioClipRefsSo;

	private float volume = 1f;
	
	private void Awake()
	{
		Instance = this;

		volume = PlayerPrefs.GetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, 1f);
	}

	private void Start()
	{
		DeliveryManager.Instance.OnRecipeSuccess += DeliveryManager_OnRecipeSuccess;
		DeliveryManager.Instance.OnRecipeFailed += DeliveryManager_OnRecipeFailed;
		CuttingCounter.OnAnyCut += CuttingCounter_OnAnyCut;
		Player.OnAnyPickedSomething += Player_OnAnyPickedSomething;
		BaseCounter.OnAnyObjectPlacedHere += BaseCounter_OnAnyObjectPlacedHere;
		TrashCounter.OnAnyObjectTrashed += TrashCounter_OnAnyObjectTrashed;
	}

	private void TrashCounter_OnAnyObjectTrashed(object sender, EventArgs e)
	{
		TrashCounter trashCounter = sender as TrashCounter;
		PlaySound(_audioClipRefsSo.trash, trashCounter.transform.position);
	}

	private void BaseCounter_OnAnyObjectPlacedHere(object sender, EventArgs e)
	{
		BaseCounter baseCounter = sender as BaseCounter;
		PlaySound(_audioClipRefsSo.objectDrop, baseCounter.transform.position);
	}

	private void Player_OnAnyPickedSomething(object sender, EventArgs e)
	{
		Player player = sender as Player;
		PlaySound(_audioClipRefsSo.objectPickup, player.transform.position);
	}

	private void CuttingCounter_OnAnyCut(object sender, EventArgs e)
	{
		CuttingCounter cuttingCounter = sender as CuttingCounter;
		PlaySound(_audioClipRefsSo.chop, cuttingCounter.transform.position);
	}

	private void DeliveryManager_OnRecipeFailed(object sender, EventArgs e)
	{
		DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
		PlaySound(_audioClipRefsSo.deliveryFail, deliveryCounter.transform.position);
	}

	private void DeliveryManager_OnRecipeSuccess(object sender, EventArgs e)
	{
		DeliveryCounter deliveryCounter = DeliveryCounter.Instance;
		PlaySound(_audioClipRefsSo.deliverySuccess, deliveryCounter.transform.position);
	}

	private void PlaySound(AudioClip[] audioClipArray, Vector3 position, float volumeMultiplier = 1f)
	{
		PlaySound(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volumeMultiplier);
	}
    
	private void PlaySound(AudioClip audioClip, Vector3 position, float volumeMultiplier = 1f)
	{
		AudioSource.PlayClipAtPoint(audioClip, position, volumeMultiplier * volume);
	}

	public void PlayFootstepsSound(Vector3 position, float volume)
	{
		PlaySound(_audioClipRefsSo.footstep, position, volume);
	}
	
	public void PlayCountdownSound()
	{
		PlaySound(_audioClipRefsSo.warning, Vector3.zero);
	}

	public void PlayWarningSound(Vector3 position)
	{
		PlaySound(_audioClipRefsSo.warning, position);
	}
	
	public void ChangeVolume()
	{
		volume += .1f;

		if (volume > 1f)
		{
			volume = 0f;
		}
		
		PlayerPrefs.SetFloat(PLAYER_PREFS_SOUND_EFFECTS_VOLUME, volume);
		PlayerPrefs.Save();
	}

	public float GetVolume()
	{
		return volume;
	}
}
