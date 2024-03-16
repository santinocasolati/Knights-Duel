using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBarUpdate : MonoBehaviour
{
    private PlayerHealthController healthController;
    private Slider slider;

    private void Start()
    {
        healthController = transform.parent.GetComponentInParent<PlayerHealthController>();
        slider = GetComponent<Slider>();
        healthController.OnPlayerHealthModified.AddListener(HealthModifiedHandler);
    }

    private void HealthModifiedHandler(int health)
    {
        slider.value = health;
    }
}
