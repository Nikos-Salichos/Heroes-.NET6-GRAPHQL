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

                   return unitOfWorkRepository.HeroRepository.CreateHeroMsql(hero);
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

                     Task<Hero?>? heroFromDb = unitOfWorkRepository.HeroRepository.GetHeroByIdAsyncMsql(heroId);

                     if (heroFromDb.Result == null)
                     {
                         context.Errors.Add(new ExecutionError("Cannot find hero"));
                         return null;
                     }

                     heroFromDb.Result.Name = hero.Name;
                     heroFromDb.Result.FirstName = hero.FirstName;
                     heroFromDb.Result.LastName = hero.LastName;
                     heroFromDb.Result.Place = hero.Place;

                     return unitOfWorkRepository.HeroRepository.UpdateHeroMsql(heroFromDb.Result);
                 });

            Field<StringGraphType>(
                "deleteHero",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "heroId" }),
                resolve: context =>
                {
                    int heroId = context.GetArgument<int>("heroId");
                    Task<Hero?>? hero = unitOfWorkRepository.HeroRepository.GetHeroByIdAsyncMsql(heroId);

                    if (hero == null || hero.Result == null)
                    {
                        context.Errors.Add(new ExecutionError("Couldn't find owner in db."));
                        return null;
                    }

                    unitOfWorkRepository.HeroRepository.DeleteHero(hero.Result);
                    return $"The hero with the id: {heroId} has been successfully deleted from db.";
                });
        }
    }
}
