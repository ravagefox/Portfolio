namespace Perfekt.Core.IO
{
    public sealed class QueryFilter
    {

        private Dictionary<string, Func<string, string>> filters;
        private List<string> ignoreQueries;


        public QueryFilter()
        {
            this.filters = new Dictionary<string, Func<string, string>>();
            this.ignoreQueries = new List<string>();
        }


        public void Add(string key, Func<string, string> func)
        {
            this.filters[key] = func;
        }

        public void AddIgnore(string ignoreKey)
        {
            this.ignoreQueries.Add(ignoreKey);
        }

        public void ClearIgnoreKeys() { this.ignoreQueries.Clear(); }


        public string GetFilteredQuery(string text)
        {
            var result = text;

            foreach (var pair in this.filters)
            {
                if (this.ignoreQueries.Any(k => k.SequenceEqual(pair.Key))) { continue; }

                result = result.Replace(pair.Key, pair.Value.Invoke(pair.Key));
            }

            return result;
        }
    }
}
