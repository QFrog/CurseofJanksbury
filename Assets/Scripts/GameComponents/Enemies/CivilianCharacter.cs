using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CivilianCharacter : EnemyCharacter
{
	protected override void Update()
	{
		position = gameObject.transform.position;
		directionChangeFrequency = patrolPath.directionChangeFrequency;
		
		if (ai.reachedDestination)
		{
			NextPatrolPoint();
			reactionWait = true;
		}
		
		time += Time.deltaTime;

		if(time <= pauseTime)
		{
			ai.isStopped = true;
		}
		else
		{
			ai.isStopped = false;
		}
	}

	protected override void OnTriggerStay(Collider other)
	{
	}
	
	protected override void OnTriggerExit(Collider other)
	{
	}
	
	protected override void OnCollisionEnter(Collision collision)
	{
	}

	public override void HitWithJankifyGun()
	{
	}
}
