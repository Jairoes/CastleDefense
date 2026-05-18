using UnityEngine;

public class FireTowerAnimationEvents : MonoBehaviour
{
    private TowerFire towerFire;

    void Start()
    {
        towerFire = GetComponentInParent<TowerFire>();
    }

    public void Shoot()
    {
        if (towerFire != null)
            towerFire.Shoot();
    }
}