using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxTriggerToggle : MonoBehaviour
{
    public enum ToggleType
    {
        turnOn,
        turnOff,
        toggle
    }

    public GameObject[] targetEntities;
    public GameObject toggleObject;
    public ToggleType toggleType;

    public bool destroyOnActivate = false;

    bool hasEntered = false;
    bool hasTriggered = true;

    public bool enter = true;
    public bool stay = false;
    public bool exit = false;

    delegate void TriggerState();
    TriggerState OnEnter;
    TriggerState OnStay;
    TriggerState OnExit;

    SphereCollider[] entitiesColliders;
    BoxCollider boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();

        if(enter)
        {
            OnEnter = Trigger;
        }
        else
        {
            OnEnter = delegate { };
        }

        if (stay)
        {
            OnStay = Trigger;
        }
        else
        {
            OnStay = delegate { };
        }

        if (exit)
        {
            OnExit = Trigger;
        }
        else
        {
            OnExit = delegate { };
        }
    }

    private void Start()
    {
        entitiesColliders = new SphereCollider[targetEntities.Length];
        for (int i = 0; i < targetEntities.Length; i++)
        {
            entitiesColliders[i] = targetEntities[i].GetComponent<SphereCollider>();
        }
    }

    private void Update()
    {
        hasTriggered = false;
        for(int i = 0; i < targetEntities.Length; i++)
        {
            if(hasEntered)
            {
                // Entity is insde
                if(!IsEntityInside(i))
                {
                    // entity has left
                    hasEntered = false;
                    OnExit();
                }
                else
                {
                    // Entity is still inside
                    OnStay();
                }
            }
            else
            {
                if (IsEntityInside(i))
                {
                    hasEntered = true;
                    OnEnter();
                }
            }
        }
    }

    private void LateUpdate()
    {
        if(destroyOnActivate && hasTriggered)
        {
            Destroy(gameObject);
        }
    }

    private bool IsEntityInside(int index)
    {
        Vector3 difference = targetEntities[index].transform.position - boxCollider.ClosestPoint(targetEntities[index].transform.position);
        return (difference.magnitude < entitiesColliders[index].radius);
    }

    void Trigger()
    {
        switch (toggleType)
        {
            case ToggleType.turnOn:
                toggleObject.SetActive(true);
                break;
            case ToggleType.turnOff:
                toggleObject.SetActive(false);
                break;
            case ToggleType.toggle:
                toggleObject.SetActive(!toggleObject.activeSelf);
                break;
        }

        hasTriggered = true;
    }
}
