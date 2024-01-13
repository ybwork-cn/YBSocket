using System;
using System.Linq;

namespace ybwork.YBSocket.YBSocket.Client
{
    internal class ConnectionAction
    {
        internal readonly string Function;
        internal readonly Delegate Action;
        internal readonly Type[] ParaTypes;

        internal ConnectionAction(string function, Delegate action)
        {
            Function = function;
            Action = action;
            ParaTypes = action.Method.GetParameters().Select(info => info.ParameterType).ToArray();
        }
    }
}
