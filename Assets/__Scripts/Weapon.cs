using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [Tooltip("Damage per second for lasers")]
    public float damagePerSec = 0;
    [Tooltip("Delay between shots")]
    public float delayBetweenShots = 0;
    [Tooltip("Velocity of projectiles")]
    public float velocity = 50;
    [Tooltip("Wave Frequency (for Phaser)")]
    public float waveFrequency = 2f;
    [Tooltip("Wave Magnitude (for Phaser)")]
    public float waveMagnitude = 0.5f;
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
    public float laserDamage = 1f;
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
    }

    public eWeaponType type
    {
        get { return (_type); }
        set { SetType(value); }
    }

    public void SetType(eWeaponType wt)
    {
        _type = wt;

        if (_type == eWeaponType.none)
        {
            this.gameObject.SetActive(false);
            return;
        }

        this.gameObject.SetActive(true);

        def = Main.GET_WEAPON_DEFINITION(_type);

        if (weaponModel != null) Destroy(weaponModel);

    
        if (def.weaponModelPrefab != null)
        {
            weaponModel = Instantiate<GameObject>(def.weaponModelPrefab, transform);
            weaponModel.transform.localPosition = Vector3.zero;
            weaponModel.transform.localScale = Vector3.one;
        }


        nextShotTime = 0;

        if (_type == eWeaponType.laser)
        {
            laserDamage = def.damagePerSec;
            laserLine = GetComponent<LineRenderer>();
            laserLine.enabled = false;
        }
      
        if (def.weaponModelPrefab != null)
        {
            weaponModel = Instantiate<GameObject>(def.weaponModelPrefab, transform);
            weaponModel.transform.localPosition = Vector3.zero;
            weaponModel.transform.localScale = Vector3.one;
        }

    }

    private void Fire()
    {
        //Debug.Log("Weapon.Fire() called on: " + gameObject.name);

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
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(20, Vector3.back);
                p.vel = p.transform.rotation * vel;
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(-20, Vector3.back);
                p.vel = p.transform.rotation * vel;
                break;
            case eWeaponType.missile:
                p = MakeProjectile();
                Vector3 newScale = transform.localScale;
                newScale.x = 1.0f;
                newScale.y = 1.0f;
                newScale.z = 3.0f;
                p.transform.localScale = newScale;
                p.vel = vel;
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

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.up, out hit, Mathf.Infinity))
        {
            Debug.Log("Laser hit: " + hit.collider.name);
            laserLine.SetPosition(1, hit.point);

            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(laserDamage); // Remove Time.deltaTime for testing

            }
        }
        else
        {
            laserLine.SetPosition(1, transform.position + Vector3.up * 100f);
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
