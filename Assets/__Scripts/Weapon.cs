using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enum of various weapon types, including Phaser which moves in a wave.
/// </summary>
public enum eWeaponType
{
    none,       // The default / no weapon
    blaster,    // A simple blaster
    spread,     // Multiple shots simultaneously
    phaser,     // Shots that move in waves
    missile,    // Homing missiles
    laser,      // Damage over time
    shield      // Raise shieldLevel
}

/// <summary>
/// WeaponDefinition defines the properties of each weapon type.
/// </summary>
[System.Serializable]
public class WeaponDefinition
{
    public eWeaponType type = eWeaponType.none;
    [Tooltip("Letter to show on the PowerUp Cube")]
    public string letter;
    [Tooltip("Color of PowerUp Cube")]
    public Color powerUpColor = Color.white;
    [Tooltip("Prefab of Weapon model attached to Player Ship")]
    public GameObject weaponModelPrefab;
    [Tooltip("Prefab of projectile fired by this weapon")]
    public GameObject projectilePrefab;
    [Tooltip("Color of the projectile")]
    public Color projectileColor = Color.white;
    [Tooltip("Damage per hit")]
    public float damageOnHit = 0;
    [Tooltip("Damage per second for lasers (Not Implemented)")]
    public float damagePerSec = 0;
    [Tooltip("Delay between shots")]
    public float delayBetweenShots = 0;
    [Tooltip("Velocity of projectiles")]
    public float velocity = 50;
    [Tooltip("Wave Frequency (for Phaser)")]
    public float waveFrequency = 2f;  // How fast the wave oscillates
    [Tooltip("Wave Magnitude (for Phaser)")]
    public float waveMagnitude = 0.5f; // Size of the wave motion
}

public class Weapon : MonoBehaviour
{
    static public Transform PROJECTILE_ANCHOR;

    [Header("Dynamic")]
    [SerializeField]
    private eWeaponType _type = eWeaponType.none;
    public WeaponDefinition def;
    public float nextShotTime;

    private GameObject weaponModel;
    private Transform shotPointTrans;

    // Laser-related fields
    private LineRenderer laserLine;
    private bool laserFiring = false;
    public float laserRange = 10f;
    public float laserDamage = 20f;
    public float laserDuration = 0.1f;

    void Start()
    {
        if (PROJECTILE_ANCHOR == null)
        {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        shotPointTrans = transform.GetChild(0);
        SetType(_type);

        Hero hero = GetComponentInParent<Hero>();
        if (hero != null) hero.fireEvent += Fire;

        // Laser setup
        if (type == eWeaponType.laser)
        {
            laserLine = GetComponent<LineRenderer>();
            laserLine.enabled = false;
        }
    }

    public eWeaponType type
    {
        get { return (_type); }
        set { SetType(value); }
    }

    public void SetType(eWeaponType wt)
    {
        _type = wt;

        gameObject.SetActive(wt != eWeaponType.none);
        def = Main.GET_WEAPON_DEFINITION(_type);

        if (weaponModel != null) Destroy(weaponModel);
        weaponModel = Instantiate<GameObject>(def.weaponModelPrefab, transform);
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localScale = Vector3.one;

        nextShotTime = 0;
    }

    private void Fire()
    {
        if (!gameObject.activeInHierarchy || Time.time < nextShotTime) return;

        ProjectileHero p;
        Vector3 vel = Vector3.up * def.velocity;

        switch (type)
        {
            case eWeaponType.blaster:
                p = MakeProjectile();
                p.vel = vel;
                break;

            case eWeaponType.spread:
                p = MakeProjectile();
                p.vel = vel;
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
                p.vel = p.transform.rotation * vel;
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
                p.vel = p.transform.rotation * vel;
                break;

            case eWeaponType.phaser:
                p = MakeProjectile();
                p.isPhaser = true;
                p.waveFrequency = def.waveFrequency;
                p.waveMagnitude = def.waveMagnitude;
                break;

            case eWeaponType.laser:
                if (!laserFiring)
                {
                    StartCoroutine(FireLaser());
                }
                break;
        }
    }

    private IEnumerator FireLaser()
    {
        laserFiring = true;
        laserLine.enabled = true;
        laserLine.SetPosition(0, transform.position);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, laserRange);
        if (hit.collider != null)
        {
            laserLine.SetPosition(1, hit.point);

            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(laserDamage);
            }
        }
        else
        {
            laserLine.SetPosition(1, transform.position + Vector3.up * laserRange);
        }

        yield return new WaitForSeconds(laserDuration);

        laserLine.enabled = false;
        laserFiring = false;
    }

    private ProjectileHero MakeProjectile()
    {
        GameObject go = Instantiate<GameObject>(def.projectilePrefab, PROJECTILE_ANCHOR);
        ProjectileHero p = go.GetComponent<ProjectileHero>();

        Vector3 pos = shotPointTrans.position;
        pos.z = 0;
        p.transform.position = pos;

        p.type = type;
        nextShotTime = Time.time + def.delayBetweenShots;
        return (p);
    }
}
