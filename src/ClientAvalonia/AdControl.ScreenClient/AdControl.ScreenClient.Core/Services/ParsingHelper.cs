using System.Dynamic;
using Newtonsoft.Json;

namespace AdControl.ScreenClient.Core.Services
{
    public static class ParsingHelper
    {
        public static Task<List<ExpandoObject>?> GetDynamicListFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return Task.FromResult<List<ExpandoObject>?>(null);

            var rows = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
            if (rows is null || rows.Count == 0)
                return Task.FromResult<List<ExpandoObject>?>(null);

            var list = new List<ExpandoObject>();
            foreach (var dict in rows)
            {
                var exp = new ExpandoObject() as IDictionary<string, object?>;
                foreach (var pair in dict)
                {
                    object? value = pair.Value switch
                    {
                        long l => l,
                        int i => i,
                        double d => d,
                        float f => f,
                        bool b => b,
                        string s => s,
                        null => null,
                        _ => pair.Value.ToString()
                    };
                    exp[pair.Key] = value;
                }

                list.Add((ExpandoObject)exp);
            }

            return Task.FromResult<List<ExpandoObject>?>(list);
        }
    }
}
