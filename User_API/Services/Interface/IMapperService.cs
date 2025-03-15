using System;
using System.Collections;

namespace Users_API.Services.Interface
{
    public interface IMapperService
    {
        TDto MapToDto<TEntity, TDto>(TEntity entity)
            where TDto : new();
        TEntity MapToEntity<TDto, TEntity>(TDto dto)
            where TEntity : new();
    }
}