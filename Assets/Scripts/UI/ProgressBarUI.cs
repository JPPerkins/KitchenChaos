using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarUI : MonoBehaviour
{
	[SerializeField] private GameObject hasProgressGameObject;
	[SerializeField] private Image barImage;

	private IHasProgress _hasProgress;
	
	private void Start()
	{
		_hasProgress = hasProgressGameObject.GetComponent<IHasProgress>();
		if (_hasProgress == null)
		{
			Debug.LogError("GameObject " + hasProgressGameObject + " does not have a component that implements IHasProgress!");
		}
		_hasProgress.OnProgressChanged += HasProgress_OnProgressChanged;
		barImage.fillAmount = 0f;
		Hide();
	}

	private void HasProgress_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
	{
		barImage.fillAmount = e.progressNormalized;
		if (e.progressNormalized == 0f || e.progressNormalized == 1f)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}

	private void Show()
	{
		gameObject.SetActive(true);
	}

	private void Hide()
	{
		gameObject.SetActive(false);	
	}
}
