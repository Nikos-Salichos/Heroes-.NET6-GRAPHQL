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

                   if (hero == null)
                   {
                       context.Errors.Add(new ExecutionError("Hero cannot be null"));
                       return null;
                   }

                   return unitOfWorkRepository.HeroRepository.CreateHero(hero);
               });


            Field<HeroType>(
                 "updateHero",
                 arguments: new QueryArguments(
                     new QueryArgument<NonNullGraphType<HeroInputType>> { Name = "hero" },
                     new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "heroId" }),
                 resolve: context =>
                 {
                     Hero? hero = context.GetArgument<Hero>("hero");
                     int heroId = context.GetArgument<int>("heroId");

                     Task<Hero?>? heroFromDb = unitOfWorkRepository.HeroRepository.GetHeroByIdAsync(heroId);

                     if (heroFromDb.Result == null)
                     {
                         context.Errors.Add(new ExecutionError("Cannot find hero"));
                         return null;
                     }

                     heroFromDb.Result.Name = hero.Name;

                     return unitOfWorkRepository.HeroRepository.UpdateHero(heroFromDb.Result);
                 });

        }
    }
}
