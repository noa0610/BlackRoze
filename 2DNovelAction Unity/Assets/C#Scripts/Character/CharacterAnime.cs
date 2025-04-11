using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnime : MonoBehaviour
{
    public Animator Animator { get; private set; }

    void Awake()
    {
        Animator = GetComponent<Animator>();
    }
}