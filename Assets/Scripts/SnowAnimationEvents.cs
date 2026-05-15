using UnityEngine;

public class SnowAnimationEvents : MonoBehaviour
{
    private TowerIce towerIce;

    void Start()
    {
        towerIce = GetComponentInParent<TowerIce>();
    }

    public void Shoot()
    {
        if (towerIce != null)
            towerIce.Shoot();
    }
}