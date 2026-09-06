using System;
using System.Collections.Generic;
using System.Globalization;
using MobilDwg.Core.Rendering;
using MobilDwg.Rendering.Camera;
using MobilDwg.Rendering.Coordinates;
using MobilDwg.Rendering.Scene;
using MobilDwg.Rendering.Styles;

namespace MobilDwg.Rendering.Viewer;

public enum MeasurementMode
{
    None = 0,
    Distance = 1,
    Area = 2,
}

public sealed class MeasurementController
{
    private readonly List<WorldPoint2> _points = new();
    private string? _explicitUnit;
    private string? _metadataUnit;

    public MeasurementMode Mode { get; set; } = MeasurementMode.None;
    public IReadOnlyList<WorldPoint2> Points => _points;

    public string? ExplicitUnit
    {
        get => _explicitUnit;
        set => _explicitUnit = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public string? MetadataUnit
    {
        get => _metadataUnit;
        set => _metadataUnit = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public void SetMetadataUnitFromInsUnits(int insUnits)
    {
        _metadataUnit = insUnits switch
        {
            1 => "in",
            2 => "ft",
            3 => "mi",
            4 => "mm",
            5 => "cm",
            6 => "m",
            7 => "km",
            8 => "μin",
            9 => "mil",
            10 => "yd",
            11 => "Å",
            12 => "nm",
            13 => "μm",
            14 => "dm",
            15 => "dam",
            16 => "hm",
            17 => "Gm",
            18 => "AU",
            19 => "ly",
            20 => "pc",
            _ => null
        };
    }

    public void AddPoint(WorldPoint2 point)
    {
        _points.Add(point);
    }

    public CadSnapResult? AddScreenPointWithSnap(
        ScreenPoint2 screenPoint,
        Camera2D camera,
        RenderScene scene,
        LayerTable layerTable,
        double snapRadiusDip = SnapQuery.DefaultSnapRadiusDip,
        double density = 1.0)
    {
        var snap = SnapQuery.FindSnapPoint(screenPoint, camera, scene, layerTable, snapRadiusDip, density);
        var worldPt = snap?.WorldPoint ?? CameraTransform.ScreenToWorld(screenPoint, camera);
        _points.Add(worldPt);
        return snap;
    }

    public void Clear()
    {
        _points.Clear();
    }

    public void UndoLastPoint()
    {
        if (_points.Count > 0)
        {
            _points.RemoveAt(_points.Count - 1);
        }
    }

    public double CalculateDistance()
    {
        if (_points.Count < 2) return 0.0;
        double total = 0.0;
        for (var i = 0; i < _points.Count - 1; i++)
        {
            var dx = _points[i + 1].X - _points[i].X;
            var dy = _points[i + 1].Y - _points[i].Y;
            total += Math.Sqrt((dx * dx) + (dy * dy));
        }
        return total;
    }

    public double CalculateArea()
    {
        if (_points.Count < 3) return 0.0;
        double sum = 0.0;
        for (int i = 0, j = _points.Count - 1; i < _points.Count; j = i++)
        {
            sum += (_points[j].X * _points[i].Y) - (_points[i].X * _points[j].Y);
        }
        return Math.Abs(sum) * 0.5;
    }

    public string FormatDistance(double distance)
    {
        var valueStr = distance.ToString("N2", CultureInfo.InvariantCulture);
        var unitStr = GetUnitString(isArea: false);
        return $"{valueStr} {unitStr}";
    }

    public string FormatArea(double area)
    {
        var valueStr = area.ToString("N2", CultureInfo.InvariantCulture);
        var unitStr = GetUnitString(isArea: true);
        return $"{valueStr} {unitStr}";
    }

    public string GetMeasurementSummary()
    {
        return Mode switch
        {
            MeasurementMode.Distance => FormatDistance(CalculateDistance()),
            MeasurementMode.Area => FormatArea(CalculateArea()),
            _ => string.Empty
        };
    }

    private string GetUnitString(bool isArea)
    {
        var baseUnit = _explicitUnit ?? _metadataUnit;
        if (!string.IsNullOrEmpty(baseUnit))
        {
            return isArea ? $"{baseUnit}²" : baseUnit;
        }

        return isArea ? "çizim birimi²" : "çizim birimi";
    }
}
