using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerateArrange : MonoBehaviour
{
    [SerializeField] GameObject day;
    [SerializeField] GameObject header;
    [SerializeField] GameObject page_btn;

    float rows = 6.0f;
    float cols = 7.0f;
    public float xgrid = 1.8f, ygrid = 0.8f;
    public float xbtn = 550.0f, ybtn = 0.0f;
    public int btn_width = 50, btn_height = 50;
    public float hight = 3.0f;
    int num = 0;

    CalendarManager calendar;
    YearMonth ym;

    void Start()
    {
        ym = FindObjectOfType<YearMonth>();
        calendar = FindObjectOfType<CalendarManager>();
        Arrange();
    }

    void Arrange()
    {
        GenerateHeader();
        GenerateList();
    }

    public void GenerateHeader()
    {
        Instantiate(header, transform).transform.localPosition = new Vector3(0, hight, 0);
    }

    public void GenerateList()
    {
        GameObject btn = Instantiate(page_btn, transform);

        RectTransform prev = btn.transform.GetChild(0).GetComponent<RectTransform>();
        prev.localPosition = new Vector3(-xbtn, ybtn, 0);
        prev.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, btn_width);
        prev.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, btn_height);

        RectTransform next = btn.transform.GetChild(1).GetComponent<RectTransform>();
        next.localPosition = new Vector3(xbtn, ybtn, 0);
        next.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, btn_width);
        next.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, btn_height);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                GameObject obj = Instantiate(day, transform);
                obj.transform.localPosition = new Vector3((c - 3.0f) * xgrid, -(r - 2.5f) * ygrid, 0);

                var dm = obj.GetComponent<daymanager>();
                dm.index = num;

                if (calendar != null)
                {
                    dm.GenerateDate(ym.year, ym.month);
                }
                else
                {
                    Debug.LogWarning("CalendarManager ��������܂���B�f�t�H���g�N�����g�p���܂��B");
                    dm.GenerateDate(System.DateTime.Now.Year, System.DateTime.Now.Month);
                }

                num++;
            }
        }
    }
}
