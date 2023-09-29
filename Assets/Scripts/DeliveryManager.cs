using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeliveryManager : NetworkBehaviour
{

	public event EventHandler OnRecipeSpawned;
	public event EventHandler OnRecipeCompleted;
	public event EventHandler OnRecipeSuccess;
	public event EventHandler OnRecipeFailed;

	public static DeliveryManager Instance { get; private set; }
	[SerializeField] private RecipeListSO _recipeListSo;
	private List<RecipeSO> waitingRecipeSOList;

	private float spawnRecipeTimer = 4f;
	private float spawnRecipeTimerMax = 4f;
	private int waitingRecipesMax = 4;
	private int successfulRecipesAmount;
	
	private void Awake()
	{
		Instance = this;
		waitingRecipeSOList = new List<RecipeSO>();
	}
	
	private void Update()
	{
		if (!IsServer)
		{
			return; 
		} 
		
		spawnRecipeTimer -= Time.deltaTime;
		if (spawnRecipeTimer <= 0f)
		{
			spawnRecipeTimer = spawnRecipeTimerMax;

			if (KitchenGameManager.Instance.IsGamePlaying() && waitingRecipeSOList.Count < waitingRecipesMax)
			{
				int waitingRecipeSoIndex = Random.Range(0, _recipeListSo.recipeSos.Count);
				
				
				// tell the clients we have a new recipe
				SpawnNewWaitingRecipeClientRpc(waitingRecipeSoIndex);
			}
		}
	}

	[ClientRpc]
	private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSoIndex)
	{
		RecipeSO waitingRecipeSO = _recipeListSo.recipeSos[waitingRecipeSoIndex];
		
		waitingRecipeSOList.Add(waitingRecipeSO);
		OnRecipeSpawned?.Invoke(this, EventArgs.Empty);	
	}

	public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
	{
		for (int i = 0; i < waitingRecipeSOList.Count; i++)
		{
			RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

			if (waitingRecipeSO.kitchenObjectSos.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
			{
				bool plateContentsMatchesRecipe = true;
				foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSos)
				{
					bool ingredientFound = false;
					foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList()) 
					{
						if (plateKitchenObjectSO == recipeKitchenObjectSO)
						{
							ingredientFound = true;
							break;
						}
					}
 
					if (!ingredientFound)
					{
						plateContentsMatchesRecipe = false;
					}
				}

				if (plateContentsMatchesRecipe)
				{
					// player deliver correct recipe
					DeliverCorrectRecipeServerRpc(i);
					return;
				}
			}
		}
		
		DeliverIncorrectRecipeServerRpc();
		// no matches found!
	}

	[ServerRpc(RequireOwnership = false)]
	private void DeliverIncorrectRecipeServerRpc()
	{
		DeliverIncorrectRecipeClientRpc();
	}
	
	[ClientRpc]
	private void DeliverIncorrectRecipeClientRpc()
	{
		OnRecipeFailed?.Invoke(this, EventArgs.Empty);
	}
	
	[ServerRpc(RequireOwnership = false)]
	private void DeliverCorrectRecipeServerRpc(int waitingRecipeSOListIndex)
	{
		DeliverCorrectRecipeClientRpc(waitingRecipeSOListIndex);
	}

	[ClientRpc]
	private void DeliverCorrectRecipeClientRpc(int waitingRecipeSOListIndex)
	{
		// player deliver correct recipe
		successfulRecipesAmount++;
		waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);
		OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
		OnRecipeSuccess?.Invoke(this, EventArgs.Empty);   
	}
	
	public List<RecipeSO> GetWaitingRecipeSOList()
	{
		return waitingRecipeSOList;
	}

	public int GetSuccessfulRecipesAmount()
	{
		return successfulRecipesAmount;
	}
}
