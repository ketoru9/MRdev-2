using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class header : MonoBehaviour
{
    // Start is called before the first frame update
    int year, month;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void UpdateOwn(int year_in, int month_in)
    {
        year = year_in;
        month = month_in;

        GameObject objyear = this.transform.GetChild(0).gameObject;
        GameObject objmonth = this.transform.GetChild(1).gameObject;

        objyear.GetComponentInChildren<TextMeshPro>().text = year.ToString("0000");
        objmonth.GetComponentInChildren<TextMeshPro>().text = month.ToString("00");
    }
}
