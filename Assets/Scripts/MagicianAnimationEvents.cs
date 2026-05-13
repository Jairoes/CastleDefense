using UnityEngine;

public class MagicianAnimationEvents : MonoBehaviour
{
    private TowerMage towerMage;

    void Start()
    {
        towerMage = GetComponentInParent<TowerMage>();
    }

    public void Shoot()
    {
        if (towerMage != null)
            towerMage.Shoot();
    }
}