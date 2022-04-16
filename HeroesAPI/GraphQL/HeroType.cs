using GraphQL.Types;
using HeroesAPI.Entities.Models;

namespace HeroesAPI.GraphQL
{
    public class HeroType : ObjectGraphType<Hero>
    {
        public HeroType()
        {
            Field(h => h.Id);
            Field(h => h.Name);
            Field(h => h.Place);
        }
    }
}
