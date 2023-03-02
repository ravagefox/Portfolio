using Perfekt.Core.Net.HttpService.Results;
using Perfekt.Core.Net.HttpService.Serialization;

namespace Perfekt.Core.Net.HttpService
{
    public class ResultFactory
    {
        public static ResultFactory DefaultFactory
        {
            get
            {
                m_factory ??= new ResultFactory();
                return m_factory;
            }
        }


        private static ResultFactory? m_factory;
        

        private readonly Dictionary<Type, IResultCreator> m_converters;


        private ResultFactory()
        {
            this.m_converters = new Dictionary<Type, IResultCreator>()
            {
                [typeof(string)] = new StringResultCreator(),
                [typeof(byte[])] = new BlobResultCreator(),
                [typeof(JObjectResult)] = new JObjectResultCreator(),
            };
        }

        public void Register(Type type, IResultCreator creator)
        {
            this.m_converters[type] = creator;
        }
        public void Unregister(Type type)
        {
            if (this.m_converters.TryGetValue(type, out var _))
            {
                this.m_converters.Remove(type);
            }
        }

        internal IResult<TResult> CreateRequest<TResult>(HttpResponseMessage msg)
        {
            return this.m_converters.TryGetValue(typeof(TResult), out var converter)
                ? (IResult<TResult>)converter.CreateResult(msg)
                : throw new NotSupportedException(
                "The specified TResult is not supported.");
        }
    }
}
