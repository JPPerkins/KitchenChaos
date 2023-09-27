using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
	[SerializeField] private KitchenObjectSO cutKitchenObjectSO;
	public override void Interact(Player player)
	{
		if (!HasKitchenObject())
		{
			// There is no kitchen object here	
			if (player.HasKitchenObject())
			{
				player.GetKitchenObject().SetKitchenObjectParent(this);
			}
		}
		else
		{
			//there is an kitchen object here
			if (player.HasKitchenObject())
			{

			}
			else
			{
				GetKitchenObject().SetKitchenObjectParent(player);
			}
		}
	}

	public override void InteractAlternate(Player player)
	{
		if (HasKitchenObject())
		{
			// There is a kitchen object here
			GetKitchenObject().DestroySelf();
			KitchenObject.SpawnKitchenObject(cutKitchenObjectSO, this);
		}
	}
}
