using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCreatures : MonoBehaviour
{
    [SerializeField]
    GameObject CreaturePrefab;

    public GameObject SpawnCreature()
    {
        return Instantiate(CreaturePrefab, transform.localPosition, Quaternion.identity);
    }
}
