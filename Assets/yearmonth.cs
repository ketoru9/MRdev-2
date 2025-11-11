using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 年月の状態を保持（Model）
public class YearMonth : MonoBehaviour
{
    public int year;
    public int month;

    void Awake()
    {
        var now = DateTime.Now;
        year = now.Year;
        month = now.Month;
    }

    public void Set(int y, int m)
    {
        year = y;
        month = m;
        Debug.Log($"[YearMonth] Set to {year}/{month}");
    }
}
