using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlateKitchenObject : KitchenObject
{
	[SerializeField] private List<KitchenObjectSO> ValidKitchenObjectSOList;
	private List<KitchenObjectSO> KitchenObjectSOList;

	private void Awake()
	{
		KitchenObjectSOList = new List<KitchenObjectSO>();
	}

	public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
	{
		if (!ValidKitchenObjectSOList.Contains(kitchenObjectSO))
		{
			return false;
		}
		
		if (KitchenObjectSOList.Contains(kitchenObjectSO))
		{
			return false;
		}

		KitchenObjectSOList.Add(kitchenObjectSO);
		return true;
	}
}
