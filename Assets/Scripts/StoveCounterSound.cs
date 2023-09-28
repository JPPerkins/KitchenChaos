using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
	[SerializeField] private StoveCounter _stoveCounter;
	private AudioSource _audioSource;

	private float warningSoundTimer;
	private bool playerWarningSound;
	
	private void Awake()
	{
		_audioSource = GetComponent<AudioSource>();
	}

	private void Start()
	{
		_stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
		_stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
	}

	private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
	{
		float burnShowProgressAmount = 0.5f;
		playerWarningSound = _stoveCounter.IsFried() && e.progressNormalized >= burnShowProgressAmount;
	}

	private void StoveCounter_OnStateChanged(object sender, StoveCounter.OnStateChangedEventArgs e)
	{
		bool playSound = e._state == StoveCounter.State.Frying || e._state == StoveCounter.State.Fried;
		if (playSound)
		{
			_audioSource.Play();
		}
		else
		{
			_audioSource.Pause();
		}
	}

	private void Update()
	{
		if (playerWarningSound)
		{
			warningSoundTimer -= Time.deltaTime;
			if (warningSoundTimer < 0f)
			{
				float warningSoundTimerMax = .2f;
				warningSoundTimer = warningSoundTimerMax;
				
				SoundManager.Instance.PlayWarningSound(_stoveCounter.transform.position);
			}
		}
	}
}
