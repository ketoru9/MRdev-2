using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateCalendar : MonoBehaviour
{
    [SerializeField] GameObject calendar;
    // Start is called before the first frame update
    void Start()
    {
        Instantiate(calendar);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
