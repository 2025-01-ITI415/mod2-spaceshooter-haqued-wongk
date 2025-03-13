using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoundsCheck))]
public class ProjectileHero : MonoBehaviour
{
    private BoundsCheck bndCheck;
    private Renderer rend;

    [Header("Dynamic")]
    public Rigidbody rigid;
    [SerializeField]
    private eWeaponType _type;

    // Phaser-specific properties
    public bool isPhaser = false;
    public float waveFrequency = 2f;  // Speed of wave oscillation
    public float waveMagnitude = 0.5f; // Size of wave motion
    private float time;

    // This public property masks the private field _type
    public eWeaponType type
    {
        get { return (_type); }
        set { SetType(value); }
    }

    void Awake()
    {
        bndCheck = GetComponent<BoundsCheck>();
        rend = GetComponent<Renderer>();
        rigid = GetComponent<Rigidbody>();
        time = 0;
    }

    void Update()
    {
        // Destroy if out of bounds
        if (bndCheck.LocIs(BoundsCheck.eScreenLocs.offUp))
        {
            Destroy(gameObject);
            return;
        }

        time += Time.deltaTime;

        if (isPhaser)
        {
            // Apply sinusoidal wave movement while moving forward
            Vector3 moveDirection = vel * Time.deltaTime;
            float waveOffset = Mathf.Sin(time * waveFrequency) * waveMagnitude;
            transform.position += moveDirection + transform.right * waveOffset;
        }
    }

    /// <summary>
    /// Sets the _type private field and colors this projectile to match the 
    /// WeaponDefinition.
    /// </summary>
    /// <param name="eType">The eWeaponType to use.</param>
    public void SetType(eWeaponType eType)
    {
        _type = eType;
        WeaponDefinition def = Main.GET_WEAPON_DEFINITION(_type);
        rend.material.color = def.projectileColor;

        // Enable phaser behavior if the weapon type is phaser
        if (_type == eWeaponType.phaser)
        {
            isPhaser = true;
            waveFrequency = def.waveFrequency;
            waveMagnitude = def.waveMagnitude;
        }
    }

    /// <summary>
    /// Allows Weapon to easily set the velocity of this ProjectileHero.
    /// </summary>
    public Vector3 vel
    {
        get { return rigid.velocity; }
        set { rigid.velocity = value; }
    }
}
