using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager singleton;

    void Start()
    {
        if (!singleton) singleton = this; 
    }

    public string ConvertToTimer(int totalSecond)
    {
        int day = 86400;
        int hour = 3600;
        int minutes = 60;

        int tDay = 0;
        int tHours = 0;
        int tMinutes = 0;


        tDay = totalSecond / day;
        totalSecond = (totalSecond - (tDay * day));

        tHours = totalSecond / hour;
        totalSecond = (totalSecond - (tHours * hour));

        tMinutes = totalSecond / minutes;
        totalSecond = (totalSecond - (tMinutes * minutes));

        string Sday = tDay < 10 ? "0" + tDay : tDay.ToString();
        string Shours = tHours < 10 ? "0" + tHours : tHours.ToString();
        string SMinute = tMinutes < 10 ? "0" + tMinutes : tMinutes.ToString();
        string SSeconds = totalSecond < 10 ? "0" + totalSecond : totalSecond.ToString();

        return Sday + " : " + Shours + " : " + SMinute + " : " + SSeconds;

    }

}
