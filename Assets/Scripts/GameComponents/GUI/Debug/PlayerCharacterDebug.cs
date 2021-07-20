using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacterDebug : MonoBehaviour
{
	[SerializeField] private Text movementStateValueText = null;

	// Start is called before the first frame update
	void Start()
	{
		StartCoroutine(DelaySubscription());
	}

	// HACK: THIS IS A DIRTY DIRTY HACK!!!
	IEnumerator DelaySubscription()
	{
		yield return new WaitForSeconds(1f);

		GameManager.PlayerCharacter.OnChangeState += PlayerCharacter_OnChangeState;
		PlayerCharacter_OnChangeState(GameManager.PlayerCharacter.GetCurrentState());
	}

	private void PlayerCharacter_OnChangeState(PlayerCharacter.MovementState obj)
	{
		movementStateValueText.text = Enum.GetName(typeof(PlayerCharacter.MovementState), obj);
	}
}
