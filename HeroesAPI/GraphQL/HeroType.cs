using GraphQL.Types;
using HeroesAPI.Models;

namespace HeroesAPI.GraphQL
{
    public class HeroType : ObjectGraphType<Hero>
    {
        public HeroType()
        {
            Field(h => h.Id).Description("The id of the Hero");
            Field(h => h.Name).Description("The name of the product");
            Field(h => h.FirstName).Description("The first name of the Hero");
            Field(h => h.LastName).Description("The last name of the Hero");
            Field(h => h.Place).Description("The place of the Hero");
        }
    }
}
