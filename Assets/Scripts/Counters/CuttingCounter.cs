using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class CuttingCounter : BaseCounter, IHasProgress
{
	public static event EventHandler OnAnyCut;

	new public static void ResetStaticData()
	{
		OnAnyCut = null;
	}
	public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
	public event EventHandler OnCut;
	
	[SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

	private int cuttingProgress;
	
	public override void Interact(Player player)
	{
		if (!HasKitchenObject())
		{
			// There is no kitchen object here	
			if (player.HasKitchenObject())
			{
				if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
				{
					// Player carrying something that can be cut
					KitchenObject kitchenObject = player.GetKitchenObject();
					kitchenObject.SetKitchenObjectParent(this);
					InteractLogicPlaceObjectOnCounterServerRpc();
				}
				
			}
		}
		else
		{
			//there is an kitchen object here
			if (player.HasKitchenObject())
			{
				if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
				{
					if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
					{
						KitchenObject.DestroyKitchenObject(GetKitchenObject());
					}
				}
			}
			else
			{
				GetKitchenObject().SetKitchenObjectParent(player);
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void InteractLogicPlaceObjectOnCounterServerRpc()
	{
		InteractLogicPlaceObjectOnCounterClientRpc();
	}
	
	[ClientRpc]
	private void InteractLogicPlaceObjectOnCounterClientRpc()
	{
		cuttingProgress = 0;
			
		OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
		{
			progressNormalized = 0f
		});
	}
	
	public override void InteractAlternate(Player player)
	{
		if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
		{
			CutObjectServerRpc();
			TestCuttingProgressDoneServerRpc();
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void CutObjectServerRpc()
	{
		if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
		{
			CutObjectClientRpc();
		}
	}

	[ClientRpc]
	private void CutObjectClientRpc()
	{
		// There is a kitchen object here
		cuttingProgress++;
		OnCut?.Invoke(this, EventArgs.Empty);
		OnAnyCut?.Invoke(this, EventArgs.Empty);
		CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
		OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
		{
			progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMax
		});
			

	}

	[ServerRpc(RequireOwnership = false)]
	private void TestCuttingProgressDoneServerRpc()
	{
		if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
		{
			CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

			if (cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
			{
				KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());
				KitchenObject.DestroyKitchenObject(GetKitchenObject());
				KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
			}
		}
	}
	
	private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
	{
		CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
		return cuttingRecipeSO != null;
	}
	
	private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
	{
		CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
		if (cuttingRecipeSO != null)
		{
			return cuttingRecipeSO.output;
		}
		else
		{
			return null;
		}
	}
	
	private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
	{
		foreach (CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOArray)
		{
			if (cuttingRecipeSO.input == inputKitchenObjectSO)
			{
				return cuttingRecipeSO;
			}
		}

		return null;
	}
}
