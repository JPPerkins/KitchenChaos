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
}
