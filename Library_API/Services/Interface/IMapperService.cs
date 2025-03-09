namespace BookStore_API.Services.Interface
{
    public interface IMapperService
    {
        TDestination Map<TSource, TDestination>(TSource source)
            where TDestination : new();
    }
}