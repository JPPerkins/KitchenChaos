using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlateIconsSingleUI : MonoBehaviour
{
	[SerializeField] private Image image;
	
	public void SetKitchenObjectSo(KitchenObjectSO kitchenObjectSo)
	{
		image.sprite = kitchenObjectSo.sprite;
	}
}
