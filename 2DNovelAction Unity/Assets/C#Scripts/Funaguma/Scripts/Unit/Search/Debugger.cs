using System.Collections.Generic;

namespace Test.Debugs
{
    public static class StringProssecing
    {
        public static string GetFilterSummary(IFilterComponent component, IUnit target, bool isPertinent, string message = "")
        {
            string result = isPertinent ? "Success" : "Fail";
            string messagePart = string.IsNullOrEmpty(message) ? "" : $"\nMessage: {message}";

            return $"Executing filter: {component.GetType().Name}\n" +
                   $"Target: {target.UnitStatus.name}\n" +
                   $"Result: {result}{messagePart}";
        }


        public static string GetUnitSummary(List<IUnit> units)
        {
            string r = string.Empty;
            foreach (var u in units)
            {
                UnitStatus s = u.UnitStatus;
                r += $"id:{s.id}\nname:{s.name}\n";
            }
            return r;
        }
    }
}