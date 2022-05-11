using GraphQL;
using GraphQL.Types;
using HeroesAPI.Models;

namespace HeroesAPI.GraphQL
{
    public class HeroMutation : ObjectGraphType
    {
        public HeroMutation(IUnitOfWorkRepository unitOfWorkRepository)
        {
            Field<HeroType>(
                "CreateHero",
                arguments: new QueryArguments(new List<QueryArgument> {
                    new QueryArgument<NonNullGraphType<HeroInputType>>{Name = "HeroInput"}
                }),
                resolve: context =>
               {
                   Hero? component = context.GetArgument<Hero>("HeroInput");
                   return unitOfWorkRepository.HeroRepository.CreateHero(component);
               });

        }
    }
}
