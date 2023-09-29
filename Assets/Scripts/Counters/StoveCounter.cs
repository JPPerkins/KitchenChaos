using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using Unity.Netcode;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
	public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
	public event EventHandler<OnStateChangedEventArgs> OnStateChanged;

	public class OnStateChangedEventArgs : EventArgs
	{
		public State _state;
	}
	
	public enum State
	{
		Idle, 
		Frying,
		Fried,
		Burned,
	}
	
	[SerializeField] private FryingRecipeSO[] fryingRecipeSOArray;
	[SerializeField] private BurningRecipeSO[] burningRecipeSOArray;
	
	private NetworkVariable<State> _state = new NetworkVariable<State>(State.Idle);
	private NetworkVariable<float> _fryingTimer = new NetworkVariable<float>(0f);
	private FryingRecipeSO _fryingRecipeSO;
	private NetworkVariable<float> _burningTimer = new NetworkVariable<float>(0f);
	private BurningRecipeSO _burningRecipeSO;
	
	public override void OnNetworkSpawn()
	{
		_fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
		_burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
		_state.OnValueChanged += State_OnValueChanged;
	}

	private void FryingTimer_OnValueChanged(float previousValue, float newValue)
	{
		float fryingTimerMax = _fryingRecipeSO != null ? _fryingRecipeSO.fryingTimerMax : 1f;
		OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
		{
			progressNormalized = (float)_fryingTimer.Value / fryingTimerMax
		});
	}
	
	private void BurningTimer_OnValueChanged(float previousValue, float newValue)
	{
		float burningTimerMax = _burningRecipeSO != null ? _burningRecipeSO.burningTimerMax : 1f;
		OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
		{
			progressNormalized = (float)_burningTimer.Value / burningTimerMax
		});
	}

	private void State_OnValueChanged(State previousState, State newState)
	{
		OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
		{
			_state = _state.Value
		});

		if (_state.Value == State.Burned || _state.Value == State.Idle)
		{
			OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
			{
				progressNormalized = 0f
			});
		}
	}
	
	private void Update()
	{
		if (!IsServer) return;
		
		if (!HasKitchenObject()) return;
		
		switch (_state.Value)
		{
			case State.Idle:
				break;
			case State.Frying:
				_fryingTimer.Value += Time.deltaTime;
				
				if (_fryingTimer.Value > _fryingRecipeSO.fryingTimerMax)
				{
					KitchenObject.DestroyKitchenObject(GetKitchenObject());
					KitchenObject.SpawnKitchenObject(_fryingRecipeSO.output, this);
				
					_state.Value = State.Fried;
					_burningTimer.Value = 0f;
					SetBurningRecipeSOClientRpc(
						KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectSO())
					);
				}
				break;
			case State.Fried:
				_burningTimer.Value += Time.deltaTime;
				
				if (_burningTimer.Value > _burningRecipeSO.burningTimerMax)
				{
					KitchenObject.DestroyKitchenObject(GetKitchenObject());
					
					KitchenObject.SpawnKitchenObject(_burningRecipeSO.output, this);
					
					_state.Value = State.Burned;
					

				}
				break;
			case State.Burned:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

	}

	public override void Interact(Player player)
	{
		if (!HasKitchenObject())
		{
			// There is no kitchen object here	
			if (!player.HasKitchenObject()) return;
			if (!HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) return;

			KitchenObject kitchenObject = player.GetKitchenObject();
			// Player carrying something that can be cut
			kitchenObject.SetKitchenObjectParent(this);

			InteractLogicPlaceObjectOnCounterServerRpc(
				KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectSO())
			);
			

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
						GetKitchenObject().DestroySelf();
						_state.Value = State.Idle;
					}
				}
			}
			else
			{
				GetKitchenObject().SetKitchenObjectParent(player);

				SetStateIdleServerRpc();
			}
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetStateIdleServerRpc()
	{
		_state.Value  = State.Idle;
	}

	[ServerRpc(RequireOwnership = false)]
	private void InteractLogicPlaceObjectOnCounterServerRpc(int kitchenObjectSOIndex)
	{
		_fryingTimer.Value = 0f;
		_state.Value = State.Frying;
		SetFryingRecipeSOClientRpc(kitchenObjectSOIndex);
	}
	
	[ClientRpc]
	private void SetFryingRecipeSOClientRpc(int kitchenObjectSOIndex)
	{
		KitchenObjectSO kitchenObjectSO =
			KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
		_fryingRecipeSO = GetFryingRecipeSOWithInput(kitchenObjectSO);
		
	}
    
	[ClientRpc]
	private void SetBurningRecipeSOClientRpc(int kitchenObjectSOIndex)
	{
		KitchenObjectSO kitchenObjectSO =
			KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
		_burningRecipeSO = GetBurningRecipeSOWithInput(kitchenObjectSO);
		
	}
	
	private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
	{
		FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
		return fryingRecipeSO != null;
	}
	
	private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
	{
		FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
		return fryingRecipeSO != null ? fryingRecipeSO.output : null;
	}
	
	private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
	{
		foreach (FryingRecipeSO fryingRecipeSO in fryingRecipeSOArray)
		{
			if (fryingRecipeSO.input == inputKitchenObjectSO)
			{
				return fryingRecipeSO;
			}
		}

		return null;
	}
	
	private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
	{
		foreach (BurningRecipeSO burningRecipeSO in burningRecipeSOArray)
		{
			if (burningRecipeSO.input == inputKitchenObjectSO)
			{
				return burningRecipeSO;
			}
		}

		return null;
	}

	public bool IsFried()
	{
		return _state.Value == State.Fried;
	}
}
