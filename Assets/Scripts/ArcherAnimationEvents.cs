using UnityEngine;

public class ArcherAnimationEvents : MonoBehaviour
{
    private TowerArcher towerArcher;

    void Start()
    {
        towerArcher = GetComponentInParent<TowerArcher>();
    }

    public void Shoot()
    {
        if (towerArcher != null)
            towerArcher.Shoot();
    }
}