using System;

namespace ThreeL.Client.Win.Helpers
{
    public class DateTimeHelper
    {
        public string ConvertDateTimeToText(DateTime dateTime)
        {
            if (dateTime.Year == DateTime.Now.Year && dateTime.DayOfYear == DateTime.Now.DayOfYear)
            {
                //今天
                return dateTime.ToString("t");
            }
            else if (dateTime.Year == DateTime.Now.Year && dateTime.DayOfYear == DateTime.Now.DayOfYear - 1)
            {
                //昨天
                return "昨天 " + dateTime.ToString("HH:mm");
            }
            else if (dateTime.Year == DateTime.Now.Year && dateTime.DayOfYear == DateTime.Now.DayOfYear - 2)
            {
                //前天
                return "前天 " + dateTime.ToString("HH:mm");
            }
            else if (IsInSameWeek(dateTime, DateTime.Now))
            {
                //前天
                return $"周{ConvertDayOfWeek(dateTime.DayOfWeek)} " + dateTime.ToString("HH:mm");
            }
            else if (dateTime.Year == DateTime.Now.Year)
            {
                //今年
                return dateTime.ToString("MM-dd HH:mm");
            }
            else
            {
                return dateTime.ToString("yyyy/MM/dd HH:mm");
            }
        }

        private string ConvertDayOfWeek(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "一",
                DayOfWeek.Tuesday => "二",
                DayOfWeek.Wednesday => "三",
                DayOfWeek.Thursday => "四",
                DayOfWeek.Friday => "五",
                DayOfWeek.Saturday => "六",
                DayOfWeek.Sunday => "日",
                _ => throw new NotImplementedException(),
            };
        }

        private bool IsInSameWeek(DateTime dtmS, DateTime dtmE)
        {
            TimeSpan ts = dtmE - dtmS;
            double dbl = ts.TotalDays;
            int intDow = Convert.ToInt32(dtmE.DayOfWeek);
            if (intDow == 0) intDow = 7;
            if (dbl >= 7 || dbl >= intDow) return false;
            else return true;
        }

        public string ConvertDateTimeToShortText(DateTime dateTime)
        {
            if (dateTime.Year == DateTime.Now.Year && dateTime.DayOfYear == DateTime.Now.DayOfYear)
            {
                //今天
                return dateTime.ToString("t");
            }
            else if (dateTime.Year == DateTime.Now.Year && dateTime.DayOfYear == DateTime.Now.DayOfYear - 1)
            {
                //昨天
                return "昨天";
            }
            else if (dateTime.Year == DateTime.Now.Year && dateTime.DayOfYear == DateTime.Now.DayOfYear - 2)
            {
                //前天
                return "前天";
            }
            else if (dateTime.Year == DateTime.Now.Year)
            {
                return dateTime.ToString("MM-dd");
            }
            else
            {
                return dateTime.ToString("yyyy/MM/dd");
            }
        }

        //将秒数转化为时分秒
        public string SecondConvertTime(int seconds)
        {
            TimeSpan ts = new TimeSpan(0, 0, seconds);
            string str = "";
            if (ts.Hours > 0)
            {
                str = string.Format("{0:00}", ts.Hours) + ":" + string.Format("{0:00}", ts.Minutes) + ":" + string.Format("{0:00}", ts.Seconds);
            }
            if (ts.Hours == 0 && ts.Minutes > 0)
            {
                str = "00:" + string.Format("{0:00}", ts.Minutes) + ":" + string.Format("{0:00}", ts.Seconds);
            }
            if (ts.Hours == 0 && ts.Minutes == 0)
            {
                str = "00:00:" + string.Format("{0:00}", ts.Seconds);
            }

            return str;
        }
    }
}
