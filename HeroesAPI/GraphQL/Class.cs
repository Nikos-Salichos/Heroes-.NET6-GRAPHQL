using GraphQL.Types;

namespace HeroesAPI.GraphQL
{
    public class HeroQuery : ObjectGraphType
    {
        public HeroQuery(IHeroRepository heroRepository)
        {
            Field<ListGraphType<HeroType>>("heroes", resolve: context => heroRepository.GetAllHeroesAsync());
        }
    }
}
