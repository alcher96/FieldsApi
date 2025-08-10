using AutoMapper;
using FieldsApi.Application.DTO;
using FieldsApi.Application.Services;
using FieldsApi.Domain.Repositories;
using NetTopologySuite.Geometries;
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace FieldsApi.Infrastructure.Services;

 public class FieldService(IFieldRepository fieldRepository, IMapper mapper) : IFieldService
 {
     public async Task<List<FieldDto>> GetAllFields(CancellationToken cancellationToken = default)
        {
            var fields = await fieldRepository.GetAllFields(cancellationToken);
            return mapper.Map<List<FieldDto>>(fields);
        }

        public async Task<double> GetFieldSize(int id,CancellationToken cancellationToken = default)
        {
            var field = await fieldRepository.GetFieldById(id,cancellationToken);
            return field.Size;
        }

        public async Task<double> GetDistanceToCentroid(int id, double lat, double lng,CancellationToken cancellationToken = default)
        {
            var field = await fieldRepository.GetFieldById(id,cancellationToken);

            var centroidLat = field.Centroid.Latitude;
            var centroidLng = field.Centroid.Longitude;

            const double earthRadius = 6371e3; // Радиус Земли в метрах
            var phi1 = lat * Math.PI / 180;
            var phi2 = centroidLat * Math.PI / 180;
            var deltaPhi = (centroidLat - lat) * Math.PI / 180;
            var deltaLambda = (centroidLng - lng) * Math.PI / 180;

            var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                    Math.Cos(phi1) * Math.Cos(phi2) *
                    Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadius * c;
        }

        public async Task<(int Id, string Name)?> GetFieldContainingPoint(double lat, double lng,CancellationToken cancellationToken = default)
        {
            var fields = await fieldRepository.GetAllFields(cancellationToken);
            var point = new Point(lng, lat);

            foreach (var field in from field in fields 
                     let coordinates = field.Polygon
                         .Select(p => new Coordinate(p.Longitude, p.Latitude)).ToArray() 
                     let polygon = new GeometryFactory().CreatePolygon(new LinearRing(coordinates)) 
                     where polygon.Contains(point) select field)
            {
                return (field.Id, field.Name);
            }
            return null;
        }
    }
    
