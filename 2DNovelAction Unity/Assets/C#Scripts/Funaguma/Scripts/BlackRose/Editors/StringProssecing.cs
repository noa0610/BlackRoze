using BlackRose.Core.Models.SearchSystems;
using BlackRose.Core.Models.Units;
using System.Collections.Generic;

namespace BlackRose.Editors
{
    public static class StringProssecing
    {
        public static string GetFilterSummary(IFilterComponent component, UnitBase target, bool isPertinent, string message = "")
        {
            string result = isPertinent ? "Success" : "Fail";
            string messagePart = string.IsNullOrEmpty(message) ? "" : $"\nMessage: {message}";

            return $"Executing filter: {component.GetType().Name}\n" +
                   $"Target: {target.UnitStatusData.unitName}\n" +
                   $"Result: {result}{messagePart}";
        }


        public static string GetUnitSummary(List<UnitBase> units)
        {
            string r = string.Empty;
            foreach (var u in units)
            {
                var s = u.UnitStatusData;
                r += $"id:{s.id}\nname:{s.unitName}\n";
            }
            return r;
        }
    }
}