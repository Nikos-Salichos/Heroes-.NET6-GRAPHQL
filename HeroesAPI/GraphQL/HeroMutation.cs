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
                "createHero",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<HeroInputType>> { Name = "hero" }),
                resolve: context =>
               {
                   Hero? hero = context.GetArgument<Hero>("hero");
                   return unitOfWorkRepository.HeroRepository.CreateHero(hero);
                   //return unitOfWorkRepository.HeroRepository.CreateOwner(hero);
               });

        }
    }
}
