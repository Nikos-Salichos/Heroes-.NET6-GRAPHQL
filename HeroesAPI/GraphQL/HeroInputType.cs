using GraphQL.Types;

namespace HeroesAPI.GraphQL
{
    public class HeroInputType : InputObjectGraphType
    {
        public HeroInputType()
        {
            Name = "heroInput";
            //  Field<IntGraphType>("Id");
            Field<StringGraphType>("Name");
            Field<StringGraphType>("FirstName");
            Field<StringGraphType>("LastName");
            Field<StringGraphType>("Place");
        }
    }
}
