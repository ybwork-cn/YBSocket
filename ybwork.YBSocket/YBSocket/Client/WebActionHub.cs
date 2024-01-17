using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using ybwork.YBSocket.YBSocket.Client;

namespace ybwork.YBSocket.Client
{
    public sealed class WebActionHub
    {
        private Dictionary<string, ConnectionAction> Functions { get; } = new();

        public void On(string function, Action action)
            => On(function, (Delegate)action);
        public void On<T>(string function, Action<T> action)
            => On(function, (Delegate)action);
        public void On<T1, T2>(string function, Action<T1, T2> action)
            => On(function, (Delegate)action);
        public void On<T1, T2, T3>(string function, Action<T1, T2, T3> action)
            => On(function, (Delegate)action);
        public void On<T1, T2, T3, T4>(string function, Action<T1, T2, T3, T4> action)
            => On(function, (Delegate)action);
        public void On<T1, T2, T3, T4, T5>(string function, Action<T1, T2, T3, T4, T5> action)
            => On(function, (Delegate)action);
        public void On<T1, T2, T3, T4, T5, T6>(string function,
            Action<T1, T2, T3, T4, T5, T6> action)
            => On(function, (Delegate)action);

        private void On(string function, Delegate @delegate)
        {
            ConnectionAction serverAction = new ConnectionAction(function, @delegate);
            Functions.Add(function, serverAction);
        }

        internal void Invoke(WebMessage webMessage)
        {
            if (!Functions.TryGetValue(webMessage.Function, out ConnectionAction connectionAction))
                return;

            if (connectionAction.ParaTypes.Length != webMessage.Params.Count)
                return;

            List<object> paras = new List<object>();
            for (int i = 0; i < webMessage.Params.Count; i++)
            {
                paras.Add(webMessage.Params[i].ToObject(connectionAction.ParaTypes[i]));
            }
            connectionAction.Action.DynamicInvoke(paras.ToArray());
        }
    }
}
