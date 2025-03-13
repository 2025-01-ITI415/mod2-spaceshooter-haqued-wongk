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
    [Tooltip("Setting this manually while playing does not work properly.")]
    private eWeaponType _type = eWeaponType.none;
    public WeaponDefinition def;
    public float nextShotTime; // Time the Weapon will fire next

    private GameObject weaponModel;
    private Transform shotPointTrans;

    void Start()
    {
        // Set up PROJECTILE_ANCHOR if it has not already been done
        if (PROJECTILE_ANCHOR == null)
        {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }

        shotPointTrans = transform.GetChild(0);

        // Call SetType() for the default _type set in the Inspector
        SetType(_type);

        // Find the fireEvent of a Hero Component in the parent hierarchy
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
        if (type == eWeaponType.none)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
        {
            this.gameObject.SetActive(true);
        }

        // Get the WeaponDefinition for this type
        def = Main.GET_WEAPON_DEFINITION(_type);

        // Destroy any old model and attach a new one
        if (weaponModel != null) Destroy(weaponModel);
        weaponModel = Instantiate<GameObject>(def.weaponModelPrefab, transform);
        weaponModel.transform.localPosition = Vector3.zero;
        weaponModel.transform.localScale = Vector3.one;

        nextShotTime = 0; // You can fire immediately after _type is set
    }

    private void Fire()
    {
        // If inactive or not enough time has passed, return
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
                p.isPhaser = true; // Enable Phaser behavior
                p.waveFrequency = def.waveFrequency;
                p.waveMagnitude = def.waveMagnitude;
                break;
        }
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
