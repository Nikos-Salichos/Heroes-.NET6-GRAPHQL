using GraphQL.Types;

namespace HeroesAPI.GraphQL
{
    public class HeroInputType : InputObjectGraphType
    {
        public HeroInputType()
        {
            Name = "HeroInput";
            Field<NonNullGraphType<StringGraphType>>("Name");
            Field<NonNullGraphType<StringGraphType>>("FirstName");
            Field<NonNullGraphType<StringGraphType>>("LastName");
            Field<NonNullGraphType<StringGraphType>>("Place");
        }
    }
}
