namespace Perfekt.Core.Net.SqlService
{
    public delegate bool SqlFilter<SqlDataObject>(SqlDataObject sqlDataObject);


    public static class SqlCustomFilter<SqlDataObject>
    {
        public static bool IsCondition(SqlDataObject obj, IEnumerable<SqlFilter<SqlDataObject>> filters)
        {
            return Filter(new[] { obj }, filters).Count() > 0;
        }

        public static IEnumerable<SqlDataObject> Filter(IEnumerable<SqlDataObject> objs, IEnumerable<SqlFilter<SqlDataObject>> filters)
        {
            return objs.Where(x =>
            {
                foreach (var filter in filters)
                {
                    if (!filter(x)) { return false; }
                }

                return true;
            }).ToList();
        }
    }
}
