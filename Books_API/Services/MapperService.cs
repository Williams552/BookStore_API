using System.Collections;
using System.Reflection;
using Books_API.Services.Interface;

namespace Books_API.Services
{
    public class MapperService : IMapperService
    {
        // Chiều Entity -> DTO
        public TDto MapToDto<TEntity, TDto>(TEntity entity)
            where TDto : new()
        {
            var dto = new TDto();
            MapToDtoInternal(entity, dto);
            return dto;
        }

        // Chiều DTO -> Entity
        public TEntity MapToEntity<TDto, TEntity>(TDto dto)
            where TEntity : new()
        {
            var entity = new TEntity();
            MapToEntityInternal(dto, entity);
            return entity;
        }

        // Xử lý ánh xạ cho chiều Entity -> DTO
        private void MapToDtoInternal<TEntity, TDto>(TEntity source, TDto destination)
        {
            var sourceProperties = typeof(TEntity).GetProperties();
            var destinationProperties = typeof(TDto).GetProperties();

            foreach (var sourceProp in sourceProperties)
            {
                var destProp = destinationProperties.FirstOrDefault(p =>
                    p.Name == sourceProp.Name && p.CanWrite);

                if (destProp == null) continue;

                var sourceValue = sourceProp.GetValue(source);
                if (sourceValue == null) continue;

                ProcessPropertyForDto(sourceValue, sourceProp, destProp, destination);
            }
        }

        // Xử lý ánh xạ cho chiều DTO -> Entity
        private void MapToEntityInternal<TDto, TEntity>(TDto source, TEntity destination)
        {
            var sourceProperties = typeof(TDto).GetProperties();
            var destinationProperties = typeof(TEntity).GetProperties();

            foreach (var sourceProp in sourceProperties)
            {
                var destProp = destinationProperties.FirstOrDefault(p =>
                    p.Name == sourceProp.Name && p.CanWrite);

                if (destProp == null) continue;

                var sourceValue = sourceProp.GetValue(source);
                if (sourceValue == null) continue;

                ProcessPropertyForEntity(sourceValue, sourceProp, destProp, destination);
            }
        }

        private void ProcessPropertyForDto(object sourceValue,
                                          PropertyInfo sourceProp,
                                          PropertyInfo destProp,
                                          object destination)
        {
            // Xử lý collection
            if (IsCollection(sourceProp) && IsCollection(destProp))
            {
                var sourceCollection = (IEnumerable)sourceValue;
                var destItemType = destProp.PropertyType.GetGenericArguments()[0];
                var destCollection = (IList)Activator.CreateInstance(
                    typeof(List<>).MakeGenericType(destItemType));

                foreach (var item in sourceCollection)
                {
                    var destItem = MapDynamicToDto(item, destItemType);
                    destCollection.Add(destItem);
                }

                destProp.SetValue(destination, destCollection);
            }
            // Xử lý nested object
            else if (IsComplexType(sourceProp.PropertyType) &&
                    IsComplexType(destProp.PropertyType))
            {
                var destValue = Activator.CreateInstance(destProp.PropertyType);
                MapToDtoInternal(sourceValue, destValue);
                destProp.SetValue(destination, destValue);
            }
            // Xử lý primitive type
            else
            {
                destProp.SetValue(destination, sourceValue);
            }
        }

        private void ProcessPropertyForEntity(object sourceValue,
                                             PropertyInfo sourceProp,
                                             PropertyInfo destProp,
                                             object destination)
        {
            // Logic tương tự nhưng cho chiều ngược lại
            if (IsCollection(sourceProp) && IsCollection(destProp))
            {
                var sourceCollection = (IEnumerable)sourceValue;
                var destItemType = destProp.PropertyType.GetGenericArguments()[0];
                var destCollection = (IList)Activator.CreateInstance(
                    typeof(List<>).MakeGenericType(destItemType));

                foreach (var item in sourceCollection)
                {
                    var destItem = MapDynamicToEntity(item, destItemType);
                    destCollection.Add(destItem);
                }

                destProp.SetValue(destination, destCollection);
            }
            else if (IsComplexType(sourceProp.PropertyType) &&
                    IsComplexType(destProp.PropertyType))
            {
                var destValue = Activator.CreateInstance(destProp.PropertyType);
                MapToEntityInternal(sourceValue, destValue);
                destProp.SetValue(destination, destValue);
            }
            else
            {
                destProp.SetValue(destination, sourceValue);
            }
        }

        private object MapDynamicToDto(object source, Type destinationType)
        {
            var method = typeof(MapperService).GetMethod(nameof(MapToDto))
                .MakeGenericMethod(source.GetType(), destinationType);
            return method.Invoke(this, new[] { source });
        }

        private object MapDynamicToEntity(object source, Type destinationType)
        {
            var method = typeof(MapperService).GetMethod(nameof(MapToEntity))
                .MakeGenericMethod(source.GetType(), destinationType);
            return method.Invoke(this, new[] { source });
        }

        private bool IsCollection(PropertyInfo prop)
        {
            return prop.PropertyType.IsGenericType &&
                   prop.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>);
        }

        private bool IsComplexType(Type type)
        {
            return !type.IsValueType && type != typeof(string);
        }
    }
}