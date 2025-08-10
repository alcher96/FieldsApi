using System.Globalization;
using FieldsApi.Domain.Entities;
using FieldsApi.Domain.Repositories;
using SharpKml.Dom;
using SharpKml.Engine;
using Point = FieldsApi.Domain.Entities.Point;
// ReSharper disable MemberCanBeMadeStatic.Local
// ReSharper disable IdentifierTypo
// ReSharper disable ConvertIfStatementToSwitchStatement
// ReSharper disable InvertIf
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8601 // Possible null reference assignment.

namespace FieldsApi.Infrastructure.Repositories;

 public class FieldRepository(string fieldsKmlPath, string centroidsKmlPath) : IFieldRepository
 {
     public async Task<List<Field>> GetAllFields(CancellationToken cancellationToken = default)
        {
            var fields = await ParseFieldsKml(fieldsKmlPath, cancellationToken);
            var centroids = await ParseCentroidsKml(centroidsKmlPath, cancellationToken);

            foreach (var field in fields)
            {
                var centroidMatch = centroids.FirstOrDefault(c => c.Id == field.Id);
                field.Centroid = centroidMatch.Id != 0 ? centroidMatch.Centroid : null;
            }

            return fields;
        }

        public async Task<Field> GetFieldById(int id, CancellationToken cancellationToken = default)
        {
            var fields = await GetAllFields(cancellationToken);
            return fields.FirstOrDefault(f => f.Id == id);
        }

        private async Task<List<Field>> ParseFieldsKml(string path, CancellationToken cancellationToken)
        {
            var fields = new List<Field>();
            try
            {
                if (!File.Exists(path))
                    return fields;

                await using var stream = File.OpenRead(path);
                var kmlFile = await Task.Run(() => KmlFile.Load(stream), cancellationToken);
                var kml = kmlFile.Root as Kml;
                if (kml == null)
                    return fields;

                var document = kml.Feature as Document;
                if (document == null)
                    return fields;

                var placemarks = GetAllPlacemarks(document).ToList();

                foreach (var placemark in placemarks)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var field = new Field
                    {
                        Name = placemark.Name ?? "Unnamed",
                        Polygon = []
                    };

                    if (placemark.ExtendedData?.SchemaData?.Any() == true)
                    {
                        var schemaData = placemark.ExtendedData.SchemaData.First();
                        foreach (var simpleData in schemaData.SimpleData)
                        {
                            if (simpleData.Name == "fid")
                            {
                                if (int.TryParse(simpleData.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var id))
                                    field.Id = id;
                            }
                            if (simpleData.Name == "size")
                            {
                                if (double.TryParse(simpleData.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var size))
                                    field.Size = size;
                            }
                        }
                    }

                    if (placemark.Geometry is Polygon polygon)
                    {
                        var coordinates = polygon.OuterBoundary.LinearRing.Coordinates;
                        field.Polygon.AddRange(coordinates.Select(c => new Point
                        {
                            Latitude = c.Latitude,
                            Longitude = c.Longitude
                        }));
                    }

                    fields.Add(field);
                }
            }
            catch (OperationCanceledException)
            {
                throw; // Пропускаем отмену наверх
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing KML: {ex.Message}");
            }

            return fields;
        }

        private async Task<List<(int Id, Centroid Centroid)>> ParseCentroidsKml(string path, CancellationToken cancellationToken)
        {
            var centroids = new List<(int, Centroid)>();
            try
            {
                if (!File.Exists(path))
                    return centroids;

                await using var stream = File.OpenRead(path);
                var kmlFile = await Task.Run(() => KmlFile.Load(stream), cancellationToken);
                if (kmlFile.Root is not Kml kml)
                    return centroids;

                if (kml.Feature is not Document document)
                    return centroids;

                var placemarks = GetAllPlacemarks(document).ToList();

                foreach (var placemark in placemarks)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var centroid = new Centroid();
                    int id = 0;

                    if (placemark.ExtendedData?.SchemaData?.Any() == true)
                    {
                        foreach (var simpleData in placemark.ExtendedData.SchemaData.First().SimpleData)
                        {
                            if (simpleData.Name == "fid")
                            {
                                int.TryParse(simpleData.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out id);
                            }
                        }
                    }

                    if (placemark.Geometry is SharpKml.Dom.Point point)
                    {
                        centroid.Latitude = point.Coordinate.Latitude;
                        centroid.Longitude = point.Coordinate.Longitude;
                    }

                    centroids.Add((id, centroid));
                }
            }
            catch (OperationCanceledException)
            {
                throw; // Пропускаем отмену наверх
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing KML: {ex.Message}");
            }

            return centroids;
        }

        private IEnumerable<Placemark> GetAllPlacemarks(Feature feature)
        {
            switch (feature)
            {
                case Placemark placemark:
                    yield return placemark;
                    break;
                case Container container:
                {
                    foreach (var childFeature in container.Features)
                    {
                        foreach (var childPlacemark in GetAllPlacemarks(childFeature))
                        {
                            yield return childPlacemark;
                        }
                    }

                    break;
                }
            }
        }
 }
    