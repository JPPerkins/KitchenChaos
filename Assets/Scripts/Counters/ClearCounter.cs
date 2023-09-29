using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
	[SerializeField] private KitchenObjectSO kitchenObjectSO;

	public override void Interact(Player player)
	{
		if (!HasKitchenObject())
		{
			if (player.HasKitchenObject())
			{
				player.GetKitchenObject().SetKitchenObjectParent(this);
			}
		}
		else
		{
			if (player.HasKitchenObject())
			{
				if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
				{
					if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
					{
						KitchenObject.DestroyKitchenObject(GetKitchenObject());
					}
				}
				else
				{
					//player is not carrying plate but something else
					if (GetKitchenObject().TryGetPlate(out plateKitchenObject))
					{
						// counter is holding a plate
						if (plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
						{
							KitchenObject.DestroyKitchenObject(player.GetKitchenObject());
						}
					}
				}
			}
			else
			{
				GetKitchenObject().SetKitchenObjectParent(player);
			}
		}
		
	}
}
