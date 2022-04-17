using GraphQL.Types;
using HeroesAPI.Entities.Models;

namespace HeroesAPI.GraphQL
{
    public class HeroType : ObjectGraphType<Hero>
    {
        public HeroType()
        {
            Field(h => h.Id).Description("The id of the Hero");
            Field(h => h.Name).Description("The name of the product");
            Field(h => h.Place).Description("The place of the Hero");
        }
    }
}
