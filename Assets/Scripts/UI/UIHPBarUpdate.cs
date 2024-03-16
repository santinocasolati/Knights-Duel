using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHPBarUpdate : MonoBehaviour
{
    private static UIHPBarUpdate _instance;

    private Slider slider;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(this);
            return;
        }

        _instance = this;
    }

    private void Start()
    {
        slider = GetComponent<Slider>();
    }

    public static void HPModified(int hp)
    {
        _instance.slider.value = hp;
    }
}
