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
        // Debug.Log("cannon animation event ejecutado!");
        if (towerCannon != null)
        {
            towerCannon.ResetTimer();
            towerCannon.Shoot();
        }
    }
}