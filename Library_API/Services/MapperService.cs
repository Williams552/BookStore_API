using BookStore_API.Services.Interface;

namespace BookStore_API.Services
{
    public class MapperService : IMapperService
    {
        public TDestination Map<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            var destination = new TDestination();
            var sourceProperties = typeof(TSource).GetProperties();
            var destinationProperties = typeof(TDestination).GetProperties();

            foreach (var sourceProp in sourceProperties)
            {
                var destProp = destinationProperties
                    .FirstOrDefault(p => p.Name == sourceProp.Name && p.CanWrite);
                if (destProp != null)
                {
                    destProp.SetValue(destination, sourceProp.GetValue(source));
                }
            }

            return destination;
        }
    }
}
