using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesIconUI : MonoBehaviour
{
	[SerializeField] private PlateKitchenObject _plateKitchenObject;
	[SerializeField] private Transform iconTempate;

	private void Awake()
	{
		iconTempate.gameObject.SetActive(false);
	}

	private void Start()
	{
		_plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;
	}

	private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
	{
		UpdateVisual();
	}

	private void UpdateVisual()
	{
		foreach (Transform child in transform)
		{
			if (child == iconTempate) continue;
			Destroy(child.gameObject);
		}
		
		foreach (KitchenObjectSO kitchenObjectSo in _plateKitchenObject.GetKitchenObjectSOList())
		{
			Transform iconTransform = Instantiate(iconTempate, transform);
			iconTransform.gameObject.SetActive(true);
			iconTransform.GetComponent<PlateIconsSingleUI>().SetKitchenObjectSo(kitchenObjectSo);
		}
	}
}
