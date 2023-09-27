using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoveCounterSound : MonoBehaviour
{
	[SerializeField] private StoveCounter _stoveCounter;
	private AudioSource _audioSource;

	private void Awake()
	{
		_audioSource = GetComponent<AudioSource>();
	}

	private void Start()
	{
		_stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
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
}
