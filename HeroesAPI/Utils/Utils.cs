using Newtonsoft.Json;

namespace HeroesAPI.Utils
{
    public static class Utils
    {
        public static string ToJSON(this object @object) => JsonConvert.SerializeObject(@object, Formatting.None);

    }
}
