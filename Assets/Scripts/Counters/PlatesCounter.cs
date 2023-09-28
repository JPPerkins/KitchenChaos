using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
	public event EventHandler OnPlateSpawned;
	public event EventHandler OnPlateRemoved;

	
	[SerializeField] private KitchenObjectSO plateKitchenObjectSO;
	private float spawnPlateTimer;
	private float spawnPlateTimerMax = 4f;
	private int platesSpawnedAmount;
	private int platesSpawnedAmountMax = 4;
	
	private void Update()
	{
		spawnPlateTimer += Time.deltaTime;
		if (!(spawnPlateTimer > spawnPlateTimerMax)) return;
		spawnPlateTimer = 0f;

		if (KitchenGameManager.Instance.IsGamePlaying() && platesSpawnedAmount < platesSpawnedAmountMax)
		{
			platesSpawnedAmount++;
				
			OnPlateSpawned?.Invoke(this, EventArgs.Empty);
		}
	}

	public override void Interact(Player player)
	{
		if (player.HasKitchenObject()) return;
		
		if (platesSpawnedAmount <= 0) return;
		
		platesSpawnedAmount--;
		KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
		OnPlateRemoved?.Invoke(this, EventArgs.Empty);
	}
}
