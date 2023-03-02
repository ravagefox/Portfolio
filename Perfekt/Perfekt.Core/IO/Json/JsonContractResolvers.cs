using Newtonsoft.Json.Serialization;

namespace Perfekt.Core.IO.Json
{
    public static class JsonContractResolvers
    {
        public static readonly DefaultContractResolver IgnoreIsSpecifiedMembersResolver =
            new DefaultContractResolver() { IgnoreIsSpecifiedMembers = true, NamingStrategy = new CamelCaseNamingStrategy() };
    }
}
