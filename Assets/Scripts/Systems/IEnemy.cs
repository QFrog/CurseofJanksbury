using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public interface IEnemy
{
	void HitWithJankifyGun();

	void SetPatrol(EnemyPatrol patrol, int startingPatrolIndex);
}
