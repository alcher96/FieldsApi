using AutoMapper;
using FieldsApi.Application.DTO;
using FieldsApi.Domain.Entities;
#pragma warning disable CS8604 // Possible null reference argument.

namespace FieldsApi;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Field, FieldDto>()
            .ForMember(dest => dest.Locations, opt => opt.MapFrom(src => new LocationDto
            {
                Center = new[] { src.Centroid.Latitude, src.Centroid.Longitude },
                Polygon = src.Polygon.Select(p => new[] { p.Latitude, p.Longitude }).ToArray()
            }));
    }
}