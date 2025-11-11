using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

//[ExecuteAlways]
public class daymanager : MonoBehaviour
{
    public int index;
    //int date_tmp;
    int date;
    //DateTime selected;
    public DateTime today;
    public int year, month;
    // Start is called before the first frame update
    public void Start()
    {
        // GenerateDate();
        // StartCoroutine(Change());
    }
    public void GenerateDate(int year_in, int month_in)
    {
        year = year_in;
        month = month_in;

        UpdateDate(year, month);
    }
    public void UpdateDate(int year, int month)
    {

        bool last_month = false, next_month = false;
        int days = DateTime.DaysInMonth(year, month);
        int days_before = (month == 1) ? DateTime.DaysInMonth(year - 1, 12)
                                       : DateTime.DaysInMonth(year, month - 1);

        // 当月1日の曜日を取得（0:日曜, 6:土曜）
        int firstDayOfWeek = (int)new DateTime(year, month, 1).DayOfWeek;

        // indexは0～41（6×7のカレンダー）を想定
        int startIndex = firstDayOfWeek;  // 当月1日が配置されるインデックス
        int endIndex = startIndex + days - 1; // 当月最終日が配置されるインデックス

        int date;
        int target_year = year;
        int target_month = month;

        if (index < startIndex)
        {
            // 前月の日付
            last_month = true;
            target_month = month - 1;
            if (target_month == 0)
            {
                target_month = 12;
                target_year--;
            }
            int diff = startIndex - index;
            date = days_before - diff + 1;
        }
        else if (index > endIndex)
        {
            // 翌月の日付
            next_month = true;
            target_month = month + 1;
            if (target_month == 13)
            {
                target_month = 1;
                target_year++;
            }
            date = index - endIndex;
        }
        else
        {
            // 当月の日付
            date = index - startIndex + 1;
        }

        today = new DateTime(target_year, target_month, date);

        // --- 表示設定 ---
        TextMeshPro text = GetComponentInChildren<TextMeshPro>();
        text.text = date.ToString();

        // 曜日による色分け
        if (today.DayOfWeek == DayOfWeek.Sunday)
        {
            text.faceColor = new Color32(255, 0, 0, 255);
        }
        else if (today.DayOfWeek == DayOfWeek.Saturday)
        {
            text.faceColor = new Color32(0, 0, 255, 255);
        }
        else
        {
            text.faceColor = new Color32(0, 0, 0, 255);
        }

        // 前後月は半透明化
        if (last_month || next_month)
        {
            text.faceColor = new Color32(text.faceColor.r, text.faceColor.g, text.faceColor.b, 127);
        }
    }
}
