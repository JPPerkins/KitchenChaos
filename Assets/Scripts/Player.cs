using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent
{
	public static Player Instance { get; private set; }
	public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
	public class OnSelectedCounterChangedEventArgs : EventArgs
	{
		public ClearCounter selectedCounter;
	}
	
	[SerializeField] private float moveSpeed = 7f;
	[SerializeField] private GameInput gameInput;
	[SerializeField] private LayerMask countersLayerMask;
	[SerializeField] private Transform kitchenObjectHoldPoint;
	
	private bool isWalking;
	private Vector3 lastInteractDir;
	private ClearCounter selectedCounter;
	private KitchenObject kitchenObject;
	
	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogError("There is more than one Player Instance");
			Destroy(gameObject);
		}

		Instance = this;
	}
	
	private void Start()
	{
		gameInput.OnInteractAction += GameInput_OnInteractAction;
	}

	private void GameInput_OnInteractAction(object sender, EventArgs e)
	{
		if (selectedCounter)
		{
			selectedCounter.Interact(this);
		}
	}

	private void Update()
	{
		HandleMovement();
		HandleInteractions();
	}

	private void HandleInteractions()
	{
		Vector2 inputVector = gameInput.GetMovementVectorNormalized();
		Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
		float interactDistance = 2f;

		if (moveDir != Vector3.zero)
		{
			lastInteractDir = moveDir;
		}
		
		if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, countersLayerMask))
		{
			if (raycastHit.transform.TryGetComponent(out ClearCounter clearCounter))
			{
				if (clearCounter != selectedCounter)
				{
					SetSelectedCounter(clearCounter);
				}
			}
			else 
			{
					SetSelectedCounter(null);
			}
		}
		else
		{
			SetSelectedCounter(null);
		}
	}
	
	private void HandleMovement()
	{
		Vector2 inputVector = gameInput.GetMovementVectorNormalized();

		Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

		float moveDistance = moveSpeed * Time.deltaTime;
		float playerRadius = 0.7f;
		float playerHeight = 2f;
		bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
			playerRadius, moveDir, moveDistance);

		if (!canMove)
		{
			// Cannot move towards this direction

			// Attempt only x movement
			Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
			canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius,
				moveDirX, moveDistance);

			if (canMove)
			{
				// can move only on the x
				moveDir = moveDirX;
			}
			else
			{
				// cannot move only on the x
				Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
				canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
					playerRadius, moveDirZ, moveDistance);

				if (canMove)
				{
					moveDir = moveDirZ;
				}
			}
		}

		if (canMove)
		{
			transform.position += moveDir * moveDistance;
		}


		isWalking = moveDir != Vector3.zero;
		float rotateSpeed = 10f;
		transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
	}

	public bool IsWalking()
	{
		return isWalking;
	}

	private void SetSelectedCounter(ClearCounter selectedCounter)
	{
		this.selectedCounter = selectedCounter;
		
		OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
		{
			selectedCounter = selectedCounter
		});
	}

	public Transform GetKitchenObjectFollowTransform()
	{
		return kitchenObjectHoldPoint;
	}

	public void SetKitchenObject(KitchenObject kitchenObject)
	{
		this.kitchenObject = kitchenObject;
	}

	public KitchenObject GetKitchenObject()
	{
		return kitchenObject;
	}

	public void ClearKitchenObject()
	{
		kitchenObject = null;
	}

	public bool HasKitchenObject()
	{
		return kitchenObject != null;
	}
}
