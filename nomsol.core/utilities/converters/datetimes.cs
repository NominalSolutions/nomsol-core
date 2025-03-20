using System;
using System.Collections.Generic;
using System.Text.Json;

namespace nomsol.core.utilities.converters
{
    public static class datetimes
    {
        /// <summary>
        /// Calculates the age based on the given birth date.
        /// </summary>
        /// <param name="dateTime">The birth date to calculate age from.</param>
        /// <returns>The calculated age in years.</returns>
        public static int CalculateAge(this DateTime dateTime)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - dateTime.Year;
            if (today < dateTime.AddYears(age))
                age--;
            return age;
        }

        /// <summary>
        /// Determines if the date is not a weekend (Saturday or Sunday).
        /// </summary>
        /// <param name="dateTime">The date to check.</param>
        /// <returns>True if the date is a weekday, false otherwise.</returns>
        public static bool IsNotWeekend(this DateTime dateTime)
        {
            return dateTime.DayOfWeek != DayOfWeek.Saturday && dateTime.DayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>
        /// Determines if the date is a weekend (Saturday or Sunday).
        /// </summary>
        /// <param name="dateTime">The date to check.</param>
        /// <returns>True if the date is a weekend, false otherwise.</returns>
        public static bool IsWeekend(this DateTime dateTime)
        {
            return dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        /// Returns the current date if it's a weekday, or the next weekday if it's a weekend.
        /// </summary>
        /// <param name="dateTime">The date to evaluate.</param>
        /// <returns>The same date if a weekday, or the next Monday if a weekend.</returns>
        public static DateTime NextWeekday(this DateTime dateTime)
        {
            if (dateTime.DayOfWeek == DayOfWeek.Saturday)
                return dateTime.AddDays(2);
            if (dateTime.DayOfWeek == DayOfWeek.Sunday)
                return dateTime.AddDays(1);
            return dateTime;
        }

        /// <summary>
        /// Serializes a list of holidays for the specified year into a JSON string.
        /// </summary>
        /// <param name="thisYear">The year for which to retrieve holidays.</param>
        /// <returns>A JSON string of holidays.</returns>
        public static string GetHolidays(this int thisYear)
        {
            var holidayResults = new HolidayResults
            {
                Results = new List<Result>
            {
                new Result { Year = thisYear, HolidayName = "New Years", HolidayDate = GetHolidate(thisYear, Holidays.New_Years).ToString("d") },
                new Result { Year = thisYear, HolidayName = "Martin Luther King Day", HolidayDate = GetHolidate(thisYear, Holidays.Martin_Luther_King_Day).ToString("d") },
                new Result { Year = thisYear, HolidayName = "President Day", HolidayDate = GetHolidate(thisYear, Holidays.President_Day).ToString("d") },
                new Result { Year = thisYear, HolidayName = "Memorial Day", HolidayDate = GetHolidate(thisYear, Holidays.Memorial_Day).ToString("d") },
                new Result { Year = thisYear, HolidayName = "Independence Day", HolidayDate = GetHolidate(thisYear, Holidays.Independence_Day).ToString("d") },
                new Result { Year = thisYear, HolidayName = "Labor Day", HolidayDate = GetHolidate(thisYear, Holidays.Labor_Day).ToString("d") },
                new Result { Year = thisYear, HolidayName = "Columbus Day", HolidayDate = GetHolidate(thisYear, Holidays.Columbus_Day).ToString("d") },
                new Result { Year = thisYear, HolidayName = "Veterans Day", HolidayDate = GetHolidate(thisYear, Holidays.Veterans_Day).ToString("d") },
                new Result { Year = thisYear, HolidayName = "Thanksgiving Day", HolidayDate = GetHolidate(thisYear, Holidays.Thanksgiving_Day).ToString("d") },
                new Result { Year = thisYear, HolidayName = "Christmas Day", HolidayDate = GetHolidate(thisYear, Holidays.Christmas_Day).ToString("d") }
            }
            };

            return JsonSerializer.Serialize(holidayResults, new JsonSerializerOptions { WriteIndented = false });
        }

        /// <summary>
        /// Retrieves the date of a specific holiday for the given year.
        /// </summary>
        /// <param name="year">The year of the holiday.</param>
        /// <param name="holidayEnum">The holiday to retrieve.</param>
        /// <returns>The holiday date, or null if the holiday is invalid.</returns>
        public static DateTime? GetHolidayDate(int year, Holidays holidayEnum)
        {
            try
            {
                return GetHolidate(year, holidayEnum);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// Formats a date into a readable string, optionally including time.
        /// </summary>
        /// <param name="dateAndTime">The date to format.</param>
        /// <param name="includeTime">Whether to include time in the output.</param>
        /// <returns>A formatted date string.</returns>
        public static string ToFormattedDateTime(this DateTime dateAndTime, bool includeTime)
        {
            string day = dateAndTime.Day.ToString();
            string suffix = GetOrdinalSuffix(dateAndTime.Day);
            string datePart = $"{dateAndTime:MMMM} {day}{suffix}, {dateAndTime:yyyy}";
            return includeTime ? $"{datePart} at {dateAndTime:h:mm tt}" : datePart;
        }

        /// <summary>
        /// Determines the quarter of the year for the given date.
        /// </summary>
        /// <param name="fromDate">The date to evaluate.</param>
        /// <returns>The quarter (1-4).</returns>
        public static int GetQuarter(this DateTime fromDate)
        {
            return ((fromDate.Month - 1) / 3) + 1;
        }

        private static string GetOrdinalSuffix(int day)
        {
            if (day >= 11 && day <= 13)
                return "<sup>th</sup>";
            return (day % 10) switch
            {
                1 => "<sup>st</sup>",
                2 => "<sup>nd</sup>",
                3 => "<sup>rd</sup>",
                _ => "<sup>th</sup>"
            };
        }

        private static DateTime AdjustForWeekendHoliday(DateTime holiday)
        {
            if (holiday.DayOfWeek == DayOfWeek.Saturday)
                return holiday.AddDays(-1);
            if (holiday.DayOfWeek == DayOfWeek.Sunday)
                return holiday.AddDays(1);
            return holiday;
        }

        private static DateTime GetHolidate(int year, Holidays holidayEnum)
        {
            switch (holidayEnum)
            {
                case Holidays.New_Years:
                    return AdjustForWeekendHoliday(new DateTime(year, 1, 1));
                case Holidays.Martin_Luther_King_Day: // Third Monday in January
                    DateTime mlkDay = new DateTime(year, 1, 1);
                    int mlkOffset = (DayOfWeek.Monday - mlkDay.DayOfWeek + 7) % 7;
                    return mlkDay.AddDays(mlkOffset + 14);
                case Holidays.President_Day: // Third Monday in February
                    DateTime presDay = new DateTime(year, 2, 1);
                    int presOffset = (DayOfWeek.Monday - presDay.DayOfWeek + 7) % 7;
                    return presDay.AddDays(presOffset + 14);
                case Holidays.Memorial_Day: // Last Monday in May
                    DateTime memorialDay = new DateTime(year, 5, 31);
                    int memorialOffset = (memorialDay.DayOfWeek - DayOfWeek.Monday + 7) % 7;
                    return memorialDay.AddDays(-memorialOffset);
                case Holidays.Independence_Day:
                    return AdjustForWeekendHoliday(new DateTime(year, 7, 4));
                case Holidays.Labor_Day: // First Monday in September
                    DateTime laborDay = new DateTime(year, 9, 1);
                    int laborOffset = (DayOfWeek.Monday - laborDay.DayOfWeek + 7) % 7;
                    return laborDay.AddDays(laborOffset);
                case Holidays.Columbus_Day: // Second Monday in October
                    DateTime columbusDay = new DateTime(year, 10, 1);
                    int columbusOffset = (DayOfWeek.Monday - columbusDay.DayOfWeek + 7) % 7;
                    return columbusDay.AddDays(columbusOffset + 7);
                case Holidays.Veterans_Day:
                    return AdjustForWeekendHoliday(new DateTime(year, 11, 11));
                case Holidays.Thanksgiving_Day: // Fourth Thursday in November
                    DateTime thanksgiving = new DateTime(year, 11, 1);
                    int thanksgivingOffset = (DayOfWeek.Thursday - thanksgiving.DayOfWeek + 7) % 7;
                    return thanksgiving.AddDays(thanksgivingOffset + 21);
                case Holidays.Christmas_Day:
                    return AdjustForWeekendHoliday(new DateTime(year, 12, 25));
                default:
                    throw new ArgumentException("Invalid holiday enum value.");
            }
        }

        public enum Holidays
        {
            New_Years = 1,
            Martin_Luther_King_Day = 2,
            President_Day = 3,
            Memorial_Day = 4,
            Independence_Day = 5,
            Labor_Day = 6,
            Columbus_Day = 7,
            Veterans_Day = 8,
            Thanksgiving_Day = 9,
            Christmas_Day = 10
        }
    }

    public class HolidayResults
    {
        public List<Result> Results { get; set; }
    }

    public class Result
    {
        public int Year { get; set; }
        public string HolidayName { get; set; }
        public string HolidayDate { get; set; }
    }
}
