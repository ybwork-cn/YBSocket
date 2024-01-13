using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ybwork.YBSocket
{
    internal class WebMessage
    {
        public string Function { get; set; }
        public JArray Params { get; set; }

        public override string ToString()
        {
            string json = JsonConvert.SerializeObject(this);
            return json;
        }
    }
}
