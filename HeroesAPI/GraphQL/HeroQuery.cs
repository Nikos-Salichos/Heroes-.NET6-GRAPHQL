using GraphQL;
using GraphQL.Types;

namespace HeroesAPI.GraphQL
{
    public class HeroQuery : ObjectGraphType
    {
        public HeroQuery(IUnitOfWorkRepository unitOfWorkRepository)
        {
            Field<ListGraphType<HeroType>>("heroes", resolve: context => unitOfWorkRepository.HeroRepository.GetAllHeroesAsync());

            Field<HeroType>(
                "hero",
                arguments: new QueryArguments(new List<QueryArgument>
                {
                    new QueryArgument<IdGraphType> {Name = "id"},
                }),
               resolve: context =>
               {

                   int? id = context.GetArgument<int?>("id");
                   if (id.HasValue)
                   {
                       Task<Models.Hero?>? hero = unitOfWorkRepository.HeroRepository.GetHeroByIdAsyncMsql(id.Value);
                       if (hero.Result != null)
                       {
                           return hero;
                       }
                       else
                       {
                           context.Errors.Add(new ExecutionError($"Hero with {id} not found"));
                           return context.Errors;
                       }
                   }
                   else
                   {
                       context.Errors.Add(new ExecutionError("Id cannot be null"));
                       return context.Errors;
                   }
               });
        }
    }
}
