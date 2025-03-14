using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [Header("Inscribed")]
    public float rotationsPerSecond = 0.1f;

    [Header("Dynamic")]
    public int levelShown = 0;

    private Material mat; // Shield material

    void Start()
    {
        // Check if Renderer component exists before accessing it
        Renderer rend = GetComponent<Renderer>();
        if (rend == null)
        {
            Debug.LogError("Shield: Renderer component is missing!");
            return;
        }

        // Get the material
        mat = rend.material;
        if (mat == null)
        {
            Debug.LogError("Shield: Material not found on Renderer!");
            return;
        }
    }

    void Update()
    {
        // Check if Hero Singleton is available before accessing shieldLevel
        if (Hero.S == null)
        {
            Debug.LogError("Shield: Hero Singleton (Hero.S) is null!");
            return;
        }

        int currLevel = Mathf.FloorToInt(Hero.S.shieldLevel);

        // Update texture offset only if shield level changes
        if (levelShown != currLevel)
        {
            levelShown = currLevel;
            if (mat.HasProperty("_MainTex")) // Ensure shader supports texture offsets
            {
                mat.mainTextureOffset = new Vector2(0.2f * levelShown, 0);
            }
            else
            {
                Debug.LogWarning("Shield: Material does not support texture offset!");
            }
        }

        // Rotate the shield in a time-based way
        float rZ = -(rotationsPerSecond * Time.time * 360) % 360f;
        transform.rotation = Quaternion.Euler(0, 0, rZ);
    }
}

