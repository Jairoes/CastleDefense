using UnityEngine;

public class GoblinAnimationEvents : MonoBehaviour
{
    private EnemyMovement enemyMovement;

    void Start()
    {
        enemyMovement = GetComponentInParent<EnemyMovement>();
    }

    public void DealDamage()
    {
        if (enemyMovement != null)
            enemyMovement.DealDamage();
    }
}