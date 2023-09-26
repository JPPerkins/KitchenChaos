using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KitchenObject : MonoBehaviour
{
	[SerializeField] private KitchenObjectSO kitchenObjectSO;

	public KitchenObjectSO GetKitchenObjectSO()
	{
		return kitchenObjectSO;
	}

	public void SetKitchenObjectSO(KitchenObjectSO kitchenObjectSO)
	{
		this.kitchenObjectSO = kitchenObjectSO;
	}
}
