using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	[SerializeField] private float moveSpeed = 7f;
	[SerializeField] private GameInput gameInput;
	[SerializeField] private LayerMask countersLayerMask;
	
	private bool isWalking;
	private Vector3 lastInteractDir;
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
				clearCounter.Interact();
			}
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
}
