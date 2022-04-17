using GraphQL.Types;

namespace HeroesAPI.GraphQL
{
    public class HeroDataSchema : Schema
    {
        public HeroDataSchema(IServiceProvider resolver) : base(resolver)
        {
            Query = resolver.GetRequiredService<HeroQuery>();
        }
    }
}
