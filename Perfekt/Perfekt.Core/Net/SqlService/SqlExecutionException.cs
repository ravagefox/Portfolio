namespace Perfekt.Core.Net.SqlService
{
    internal class SqlExecutionException : Exception
    {
        public SqlExecutionException(string msg) 
            : base(msg)
        {
        }
    }
}
