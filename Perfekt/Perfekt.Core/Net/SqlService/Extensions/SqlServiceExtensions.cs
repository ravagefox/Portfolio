using FastMember;
using System.Data.SqlClient;

namespace Perfekt.Core.Net.SqlService.Extensions
{
    public static class SqlServiceExtensions
    {
        public static IEnumerable<SqlDataObject> ReadObjects<SqlDataObject>(
            this SqlDataReader dataReader)
            where SqlDataObject : class, new()
        {
            return ReadObjects(dataReader, Array.Empty<SqlFilter<SqlDataObject>>());
        }

        public static IEnumerable<SqlDataObject> ReadObjects<SqlDataObject>(
            this SqlDataReader dataReader,
            IEnumerable<SqlFilter<SqlDataObject>> useFilters)
            where SqlDataObject : class, new()
        {
            var result = new List<SqlDataObject>();

            while (dataReader.Read())
            {
                var obj = new SqlDataObject();
                dataReader.MapDataToObject(obj);

                if (useFilters.Any())
                {
                    if (SqlCustomFilter<SqlDataObject>.IsCondition(obj, useFilters))
                    {
                        result.Add(obj);
                    }
                }
                else { result.Add(obj); }
            }

            return result.AsEnumerable();
        }

        public static void MapDataToObject<T>(this SqlDataReader dataReader, T newObject)
        {
            if (newObject == null) throw new ArgumentNullException(nameof(newObject));

            // Fast Member Usage
            var objectMemberAccessor = TypeAccessor.Create(newObject.GetType());
            var propertiesHashSet =
                    objectMemberAccessor
                    .GetMembers()
                    .Select(mp => mp.Name)
                    .ToHashSet(StringComparer.InvariantCultureIgnoreCase);

            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                var name = propertiesHashSet.FirstOrDefault(a => a.Equals(dataReader.GetName(i), StringComparison.InvariantCultureIgnoreCase));
                if (!String.IsNullOrEmpty(name))
                {
                    objectMemberAccessor[newObject, name]
                        = dataReader.IsDBNull(i) ? null : dataReader.GetValue(i);
                }
            }
        }

    }
}
