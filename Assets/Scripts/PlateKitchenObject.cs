using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PlateKitchenObject : KitchenObject
{
	public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;

	public class OnIngredientAddedEventArgs : EventArgs
	{
		public KitchenObjectSO kitchenObjectSO;
	}
	
	[SerializeField] private List<KitchenObjectSO> ValidKitchenObjectSOList;
	private List<KitchenObjectSO> KitchenObjectSOList;

	protected override void Awake()
	{
		base.Awake();
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

		AddIngredientServerRpc(KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObjectSO));
		
		return true;
	}
	
	[ServerRpc(RequireOwnership = false)]
	private void AddIngredientServerRpc(int kitchenObjectSoIndex)
	{
		AddIngredientClientRpc(kitchenObjectSoIndex);
	}
	[ClientRpc]
	private void AddIngredientClientRpc(int kitchenObjectSoIndex)
	{
		KitchenObjectSO kitchenObjectSo = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSoIndex);
		KitchenObjectSOList.Add(kitchenObjectSo);
		OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs
		{
			kitchenObjectSO = kitchenObjectSo
		});
	}

	public List<KitchenObjectSO> GetKitchenObjectSOList()
	{
		return KitchenObjectSOList;
	}
}
