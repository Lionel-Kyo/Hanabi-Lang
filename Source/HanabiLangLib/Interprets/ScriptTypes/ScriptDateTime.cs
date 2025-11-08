using HanabiLangLib.Interprets.ScriptTypes;
using HanabiLangLib.Interprets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace HanabiLangLib.Interprets.ScriptTypes
{
    public class ScriptDateTime : ScriptClass
    {
        public ScriptDateTime() : 
            base("DateTime", new List<ScriptClass>(), isStatic: false)
        {
            // Constructor
            this.AddFunction(ConstructorName, new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("year", BasicTypes.Int, ScriptValue.Null),
                new FnParameter("month", BasicTypes.Int, ScriptValue.Null),
                new FnParameter("day", BasicTypes.Int, ScriptValue.Null),
                new FnParameter("hour", BasicTypes.Int, new ScriptValue(0)),
                new FnParameter("minute", BasicTypes.Int, new ScriptValue(0)),
                new FnParameter("second", BasicTypes.Int, new ScriptValue(0)),
                new FnParameter("millisecond", BasicTypes.Int, new ScriptValue(0)),
            }, args =>
            {
                ScriptObject _this = args[0].TryObject;
                DateTime? dtNow = null;
                int? year = args[1].IsNull ? null : (int?)ScriptInt.AsCSharp(args[1].TryObject);
                int? month = args[2].IsNull ? null : (int?)ScriptInt.AsCSharp(args[2].TryObject);
                int? day = args[3].IsNull ? null : (int?)ScriptInt.AsCSharp(args[3].TryObject);
                if (year == null)
                {
                    if (dtNow == null)
                        dtNow = DateTime.Now;
                    year = DateTime.Now.Year;
                }
                if (month == null)
                {
                    if (dtNow == null)
                        dtNow = DateTime.Now;
                    month = DateTime.Now.Month;
                }
                if (day == null)
                {
                    if (dtNow == null)
                        dtNow = DateTime.Now;
                    day = DateTime.Now.Day;
                }
                int hour = (int)ScriptInt.AsCSharp(args[4].TryObject);
                int minute = (int)ScriptInt.AsCSharp(args[5].TryObject);
                int second = (int)ScriptInt.AsCSharp(args[6].TryObject);
                int millisecond = (int)ScriptInt.AsCSharp(args[7].TryObject);
                _this.BuildInObject = new DateTime(year.Value, month.Value, day.Value, hour, minute, second, millisecond);
                return ScriptValue.Null;
            });

            // --- Static Properties ---
            AddVariable("Now", _ => new ScriptValue(Create(DateTime.Now)), null, true, null);
            AddVariable("UtcNow", _ => new ScriptValue(Create(DateTime.UtcNow)), null, true, null);
            AddVariable("Today", _ => new ScriptValue(Create(DateTime.Today)), null, true, null);
            AddVariable("MinValue", _ => new ScriptValue(Create(DateTime.MinValue)), null, true, null);
            AddVariable("MaxValue", _ => new ScriptValue(Create(DateTime.MaxValue)), null, true, null);

            // --- Instance Properties ---
            AddVariable("Year", args =>
            {
                var dt = AsCSharp(args[0].TryObject);
                return new ScriptValue(dt.Year);
            }, null, false, null);

            AddVariable("Month", args =>
            {
                var dt = AsCSharp(args[0].TryObject);
                return new ScriptValue(dt.Month);
            }, null, false, null);

            AddVariable("Day", args =>
            {
                var dt = AsCSharp(args[0].TryObject);
                return new ScriptValue(dt.Day);
            }, null, false, null);

            AddVariable("Hour", args =>
            {
                var dt = AsCSharp(args[0].TryObject);
                return new ScriptValue(dt.Hour);
            }, null, false, null);

            AddVariable("Minute", args =>
            {
                var dt = AsCSharp(args[0].TryObject);
                return new ScriptValue(dt.Minute);
            }, null, false, null);

            AddVariable("Second", args =>
            {
                var dt = AsCSharp(args[0].TryObject);
                return new ScriptValue(dt.Second);
            }, null, false, null);

            AddVariable("Ticks", args =>
            {
                var dt = AsCSharp(args[0].TryObject);
                return new ScriptValue(dt.Ticks);
            }, null, false, null);

            AddVariable("DayOfWeek", args =>
            {
                var dt = AsCSharp(args[0].TryObject);
                return new ScriptValue(dt.DayOfWeek.ToString());
            }, null, false, null);

            AddVariable("DayOfYear", args =>
            {
                var dt = AsCSharp(args[0].TryObject);
                return new ScriptValue(dt.DayOfYear);
            }, null, false, null);

            // --- Instance Methods ---
            AddFunction("AddDays", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("days", BasicTypes.Float)
            }, args =>
            {
                var dt = AsCSharp(args[0].TryObject);
                double days = ScriptFloat.AsCSharp(args[1].TryObject);
                return new ScriptValue(Create(dt.AddDays(days)));
            });

            AddFunction("AddMonths", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("months", BasicTypes.Int)
            }, args =>
            {
                var dt = AsCSharp(args[0].TryObject);
                int months = (int)ScriptInt.AsCSharp(args[1].TryObject);
                return new ScriptValue(Create(dt.AddMonths(months)));
            });

            AddFunction("AddYears", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("years", BasicTypes.Int)
            }, args =>
            {
                var dt = AsCSharp(args[0].TryObject);
                int years = (int)ScriptInt.AsCSharp(args[1].TryObject);
                return new ScriptValue(Create(dt.AddYears(years)));
            });

            AddFunction("AddHours", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("hours", BasicTypes.Float)
            }, args =>
            {
                var dt = AsCSharp(args[0].TryObject);
                double hours = ScriptFloat.AsCSharp(args[1].TryObject);
                return new ScriptValue(Create(dt.AddHours(hours)));
            });

            AddFunction("AddMinutes", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("minutes", BasicTypes.Float)
            }, args =>
            {
                var dt = AsCSharp(args[0].TryObject);
                double minutes = ScriptFloat.AsCSharp(args[1].TryObject);
                return new ScriptValue(Create(dt.AddMinutes(minutes)));
            });

            AddFunction("AddSeconds", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("seconds", BasicTypes.Float)
            }, args =>
            {
                var dt = AsCSharp(args[0].TryObject);
                double seconds = ScriptFloat.AsCSharp(args[1].TryObject);
                return new ScriptValue(Create(dt.AddSeconds(seconds)));
            });

            AddFunction("ToStr", new List<FnParameter>()
            {
                new FnParameter("this"),
                new FnParameter("format", BasicTypes.Str, new ScriptValue("yyyy-MM-dd HH:mm:ss"))
            }, args =>
            {
                var dt = AsCSharp(args[0].TryObject);
                string fmt = ScriptStr.AsCSharp(args[1].TryObject);
                return new ScriptValue(dt.ToString(fmt));
            });

            // --- Static Parse ---
            AddFunction("Parse", new List<FnParameter>()
            {
                new FnParameter("value", BasicTypes.Str)
            }, args =>
            {
                string str = ScriptStr.AsCSharp(args[0].TryObject);
                var dt = DateTime.Parse(str, CultureInfo.InvariantCulture);
                return new ScriptValue(Create(dt));
            }, true);

            AddFunction("TryParse", new List<FnParameter>()
            {
                new FnParameter("value", BasicTypes.Str)
            }, args =>
            {
                string str = ScriptStr.AsCSharp(args[0].TryObject);
                if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    return new ScriptValue(Create(dt));
                return ScriptValue.Null;
            }, true);

            AddFunction("FromTicks", new List<FnParameter>()
            {
                new FnParameter("ticks", BasicTypes.Int)
            }, args =>
            {
                long ticks = ScriptInt.AsCSharp(args[0].TryObject);
                return new ScriptValue(Create(new DateTime(ticks)));
            }, true);
        }

        public override ScriptObject Create() => new ScriptObject(this, DateTime.MinValue);
        public ScriptObject Create(DateTime value) => new ScriptObject(this, value);

        private static DateTime AsCSharp(ScriptObject obj)
        {
            return (DateTime)obj.BuildInObject;
        }
    }
}
