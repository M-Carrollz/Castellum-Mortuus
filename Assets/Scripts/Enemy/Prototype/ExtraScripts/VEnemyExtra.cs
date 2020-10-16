﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class VEnemyExtra : MonoBehaviour
{
    public GameObject visionExample = null;
    Vision visioner = null;

    public MeshRenderer cylinder;

    public Material unseenMat;
    public Material spottedMat;

    Material[] spotMats;
    Material[] neutralMats;

    private void Awake()
    {
        visioner = visionExample.GetComponent<Vision>();

        spotMats = new Material[cylinder.materials.Length];
        neutralMats = new Material[cylinder.materials.Length];

        for (int i = 0; i < cylinder.materials.Length; i++)
        {
            spotMats[i] = spottedMat;
            neutralMats[i] = unseenMat;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(IsSpotted())
        {
            cylinder.materials = spotMats;
        }
        else
        {
            cylinder.materials = neutralMats;
        }
    }

    bool IsSpotted()
    {
        if (visioner.TargetInside(transform))
        {
            // Shoot ray to visioner
            Ray ray = new Ray(transform.position, (visioner.transform.position - transform.position).normalized);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, visioner.radius, LayerMask.GetMask("Agent")))
            {
                if (hit.collider.tag == "Player")
                {
                    // This has been seen.
                    return true;
                }
            }

        }

        // This is not seen.
        return false;
    }
}

