using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserWeapon : Weapon
{
    public float laserRange = 10f; // Max distance the laser can reach
    public float laserDamage = 5f; // Damage applied per second
    public LineRenderer lineRenderer;
    public LayerMask enemyLayer; // LayerMask to detect enemies
    private bool isFiring = false;
    private Transform shotPointTrans;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        shotPointTrans = transform;
    }

    public IEnumerator LaserAttack()
    {
        isFiring = true;
        lineRenderer.enabled = true;

        while (isFiring)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, laserRange, enemyLayer);

            if (hit.collider != null)
            {
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, hit.point);

                // Apply damage if an enemy is hit
                EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(laserDamage * Time.deltaTime);
                }
            }
            else
            {
                lineRenderer.SetPosition(0, shotPointTrans.position);
                lineRenderer.SetPosition(1, shotPointTrans.position + transform.up * laserRange);
            }

            yield return null; // Keep updating each frame
        }

        lineRenderer.enabled = false;
    }

    public void StopLaser()
    {
        isFiring = false;
    }
}
