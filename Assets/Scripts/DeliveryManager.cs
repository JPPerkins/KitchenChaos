using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeliveryManager : MonoBehaviour
{

	public event EventHandler OnRecipeSpawned;
	public event EventHandler OnRecipeCompleted;
	public event EventHandler OnRecipeSuccess;
	public event EventHandler OnRecipeFailed;

	public static DeliveryManager Instance { get; private set; }
	[SerializeField] private RecipeListSO _recipeListSo;
	private List<RecipeSO> waitingRecipeSOList;

	private float spawnRecipeTimer;
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
		spawnRecipeTimer -= Time.deltaTime;
		if (spawnRecipeTimer <= 0f)
		{
			spawnRecipeTimer = spawnRecipeTimerMax;

			if (waitingRecipeSOList.Count < waitingRecipesMax)
			{
				RecipeSO waitingRecipeSO = _recipeListSo.recipeSos[Random.Range(0, _recipeListSo.recipeSos.Count)];
				waitingRecipeSOList.Add(waitingRecipeSO);
				OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
			}

		}
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
					successfulRecipesAmount++;
					waitingRecipeSOList.RemoveAt(i);
					OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
					OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
					return;
				}
			}
		}
		OnRecipeFailed?.Invoke(this, EventArgs.Empty);
		// no matches found!
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
