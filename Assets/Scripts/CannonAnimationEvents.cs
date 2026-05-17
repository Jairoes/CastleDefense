using UnityEngine;

public class CannonAnimationEvents : MonoBehaviour
{
    private TowerCannon towerCannon;

    void Start()
    {
        towerCannon = GetComponentInParent<TowerCannon>();
    }

    public void Shoot()
    {
        if (towerCannon != null)
            towerCannon.Shoot();
    }
}