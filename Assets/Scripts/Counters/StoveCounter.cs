using System;
using System.Collections;
using System.Collections.Generic;
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
	
	private State _state;
	private float _fryingTimer;
	private FryingRecipeSO _fryingRecipeSO;
	private float _burningTimer;
	private BurningRecipeSO _buringRecipeSO;
	
	private void Start()
	{
		_state = State.Idle;
	}

	private void Update()
	{
		if (!HasKitchenObject()) return;
		
		switch (_state)
		{
			case State.Idle:
				break;
			case State.Frying:
				_fryingTimer += Time.deltaTime;
				
				OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
				{
					progressNormalized = (float)_fryingTimer / _fryingRecipeSO.fryingTimerMax
				});
				
				if (_fryingTimer > _fryingRecipeSO.fryingTimerMax)
				{
					GetKitchenObject().DestroySelf();
					KitchenObject.SpawnKitchenObject(_fryingRecipeSO.output, this);
				
					_state = State.Fried;
					_burningTimer = 0f;
					_buringRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

					OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
					{
						_state = _state
					});
				}
				break;
			case State.Fried:
				_burningTimer += Time.deltaTime;
				
				OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
				{
					progressNormalized = (float)_burningTimer / _buringRecipeSO.burningTimerMax
				});
				
				if (_burningTimer > _buringRecipeSO.burningTimerMax)
				{
					GetKitchenObject().DestroySelf();
					
					KitchenObject.SpawnKitchenObject(_buringRecipeSO.output, this);
					
					_state = State.Burned;
					
					OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
					{
						_state = _state
					});
					
					OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
					{
						progressNormalized = 0f
					});
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
			
			// Player carrying something that can be cut
			player.GetKitchenObject().SetKitchenObjectParent(this); 
					
			_fryingRecipeSO = GetFryingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
			
			_state = State.Frying;
			_fryingTimer = 0f;
			
			OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
			{
				_state = _state
			});
			
			OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
			{
				progressNormalized = (float)_fryingTimer / _fryingRecipeSO.fryingTimerMax
			});
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
						_state = State.Idle;
				
						OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
						{
							_state = _state
						});
				
						OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
						{
							progressNormalized = (float)0f
						});
					}
				}
			}
			else
			{
				GetKitchenObject().SetKitchenObjectParent(player);
				
				_state = State.Idle;
				
				OnStateChanged?.Invoke(this, new OnStateChangedEventArgs
				{
					_state = _state
				});
				
				OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
				{
					progressNormalized = (float)0f
				});
			}
		}
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
		return _state == State.Fried;
	}
}
