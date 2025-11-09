using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatusTest : MonoBehaviour
{
    [SerializeField, Range(0, 1)]
    private float _HP;
    [SerializeField, Range(0, 1)]
    private float _SL;
    [SerializeField, Range(0, 1)]
    private float _SP;
    [SerializeField]
    private Image _HPBar;
    [SerializeField]
    private Image _SLBar;
    [SerializeField]
    private Image _SPBar;

    void Update()
    {
        _HPBar.fillAmount = _HP;
        _SLBar.fillAmount = _SL;
        _SPBar.fillAmount = _SP;
    }
}
