// Source: ApiLibrary
/* 
   ---------------------------------------------------------------
                        CREXIUM PTY LTD
   ---------------------------------------------------------------

     The software is provided 'AS IS', without warranty of any kind,
   express or implied, including but not limited to the warrenties
   of merchantability, fitness for a particular purpose and
   noninfringement. In no event shall the authors or copyright
   holders be liable for any claim, damages, or other liability,
   whether in an action of contract, tort, or otherwise, arising
   from, out of or in connection with the software or the use of
   other dealings in the software.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WebLib.Api;

namespace WebLib.Collections
{
    internal sealed class ApiLibrary
    {
        private sealed class ApiMethodWrapper
        {
            public MethodInfo TargetMethod { get; }

            public IEnumerable<string> Names { get; }

            public bool IsAsync =>
                this.TargetMethod.GetCustomAttribute<AsyncStateMachineAttribute>() != null &&
                this.TargetMethod.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;


            public ApiMethodWrapper(IEnumerable<string> callingNames, MethodInfo method)
            {
                this.Names = callingNames;
                this.TargetMethod = method;
            }
        }


        private IEnumerable<ApiMethodWrapper> _methods;
        private ApiContentNode parent;


        public ApiLibrary(ApiContentNode node)
        {
            this._methods = this.GetMethods(node);
            this.parent = node;
        }


        public void Call(string name, UrlEncodedData urlEncodedData)
        {
            static bool IsEqual(string i, string j)
            {
                return i.ToLower().SequenceEqual(j.ToLower());
            };

            foreach (var method in this._methods)
            {
                var paramInfos = method.TargetMethod.GetParameters();

                if (method.Names.Any(n => IsEqual(n, name)) || IsEqual(method.TargetMethod.Name, name))
                {
                    if (paramInfos.Length == 1 && paramInfos[0].ParameterType == typeof(UrlEncodedData))
                    {
                        this.Call(method, new object[] { urlEncodedData });
                    }
                    else
                    {
                        var parameters = this.GetParameters(paramInfos, urlEncodedData);
                        this.Call(method, parameters);
                    }
                }
            }
        }

        private object[] GetParameters(
            ParameterInfo[] infos,
            UrlEncodedData urlEncodedData)
        {
            var values = new object[infos.Length];
            var idx = 0;
            foreach (var paramInfo in infos)
            {
                if (urlEncodedData.TryGetValue(
                    paramInfo.Name,
                    out var value))
                {
                    values[idx++] = value;
                }
            }

            return values;
        }

        private void Call(ApiMethodWrapper method, object[] parameters)
        {
            _ = method.IsAsync
                ? Task.Run(async () =>
                {
                    var m = method.TargetMethod;

                    var result = await (dynamic)method.TargetMethod.Invoke(this.parent, parameters);

                    // this.parent.InvokeAsyncCallback(m, result);
                })
                : method.TargetMethod.Invoke(this.parent, parameters);
        }

        private IEnumerable<ApiMethodWrapper> GetMethods(ApiContentNode node)
        {
            var methods = node.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes<ApiMethodAttribute>() != null);
            if (!methods.Any()) { yield break; }

            foreach (var method in methods)
            {
                var callingNames = this.GetCallingNames(method.GetCustomAttributes<ApiMethodAttribute>());
                yield return new ApiMethodWrapper(callingNames, method);
            }
        }

        private IEnumerable<string> GetCallingNames(IEnumerable<ApiMethodAttribute> enumerable)
        {
            foreach (var attribute in enumerable)
            {
                if (!string.IsNullOrEmpty(attribute.Name))
                {
                    yield return attribute.Name;
                }
            }
        }
    }
}
