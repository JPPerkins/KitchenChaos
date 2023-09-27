using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateCompleteVisual : MonoBehaviour
{
	[Serializable]
	public struct KitchenSO_GameObject
	{
		public KitchenObjectSO kitchenObjectSO;
		public GameObject gameObject;
	}
	
	[SerializeField] private PlateKitchenObject _plateKitchenObject;
	[SerializeField] private List<KitchenSO_GameObject> kitchenSoGameObjectList;
	
	private void Start()
	{
		_plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;
		
		foreach (KitchenSO_GameObject kitchenSoGameObject in kitchenSoGameObjectList)
		{
			kitchenSoGameObject.gameObject.SetActive(false);
		}
	}

	private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
	{
		foreach (KitchenSO_GameObject kitchenSoGameObject in kitchenSoGameObjectList)
		{
			if (kitchenSoGameObject.kitchenObjectSO == e.kitchenObjectSO)
			{
				kitchenSoGameObject.gameObject.SetActive(true);
			}
		}
	}
}
