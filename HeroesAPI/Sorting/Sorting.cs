namespace HeroesAPI.Sorting
{
    using System.Linq.Dynamic.Core;

    public static class Sorting
    {
        public static IEnumerable<T> OrderByProperty<T>(this IEnumerable<T> list, string property)
        {
            return list.AsQueryable().OrderBy(property);
        }
    }

}
