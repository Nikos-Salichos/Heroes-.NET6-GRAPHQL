using GraphQL;
using GraphQL.Types;

namespace HeroesAPI.GraphQL
{
    public class HeroQuery : ObjectGraphType
    {
        public HeroQuery(IHeroRepository heroRepository)
        {
            Field<ListGraphType<HeroType>>("heroes", resolve: context => heroRepository.GetAllHeroesAsync());

            Field<HeroType>(
                "hero",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<IdGraphType>>
                { Name = "id" }),
                    resolve: context =>
                    {
                        context.Errors.Add(new ExecutionError("Execution error in GetHeroByIdAsync"));
                        int id = context.GetArgument<int>("id");
                        return heroRepository.GetHeroByIdAsync(id);
                    });
        }
    }
}
