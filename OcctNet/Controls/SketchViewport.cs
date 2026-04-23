using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using OcctNet.Models;
using OcctNet.Wrapper;

namespace OcctNet.Controls;

public sealed class SketchViewport : Control
{
    private static readonly SolidColorBrush BackgroundBrush = new(Color.FromRgb(246, 247, 248));
    private static readonly SolidColorBrush GridBrush = new(Color.FromRgb(220, 224, 228));
    private static readonly SolidColorBrush AxisXBrush = new(Color.FromRgb(190, 60, 56));
    private static readonly SolidColorBrush AxisYBrush = new(Color.FromRgb(54, 142, 88));
    private static readonly SolidColorBrush AxisZBrush = new(Color.FromRgb(72, 104, 178));
    private static readonly SolidColorBrush EdgeBrush = new(Color.FromRgb(40, 48, 58));
    private static readonly SolidColorBrush PreviewBrush = new(Color.FromArgb(110, 52, 126, 202));
    private static readonly SolidColorBrush FaceBrush = new(Color.FromArgb(130, 170, 196, 223));
    private static readonly SolidColorBrush TopFaceBrush = new(Color.FromArgb(165, 148, 185, 220));
    private static readonly SolidColorBrush SideFaceBrush = new(Color.FromArgb(95, 104, 132, 156));

    private readonly Pen gridPen = new(GridBrush, 1);
    private readonly Pen edgePen = new(EdgeBrush, 1.6);
    private readonly Pen previewPen = new(PreviewBrush, 2);
    private readonly Pen xAxisPen = new(AxisXBrush, 2);
    private readonly Pen yAxisPen = new(AxisYBrush, 2);
    private readonly Pen zAxisPen = new(AxisZBrush, 2);

    private SketchPoint? pendingPoint;
    private SketchPoint? hoverPoint;
    private SketchFace? selectedFace;

    public SketchViewport()
    {
        Focusable = true;
        ClipToBounds = true;
        PointerPressed += OnPointerPressed;
        PointerMoved += OnPointerMoved;
        PointerWheelChanged += OnPointerWheelChanged;
    }

    public SketchDocument Document { get; } = new();

    public SketchTool Tool { get; set; } = SketchTool.Select;

    public double Scale { get; private set; } = 34;

    public event EventHandler<SketchStatusChangedEventArgs>? StatusChanged;

    public override void Render(DrawingContext context)
    {
        var bounds = Bounds;
        context.DrawRectangle(BackgroundBrush, null, bounds);

        DrawGrid(context);
        DrawAxes(context);
        DrawFaces(context);
        DrawEdges(context);
        DrawPreview(context);
    }

    public void Clear()
    {
        pendingPoint = null;
        hoverPoint = null;
        selectedFace = null;
        Document.Clear();
        PublishStatus("场景已清空");
        InvalidateVisual();
    }

    public void PushPullSelected(double delta)
    {
        if (selectedFace is null)
        {
            PublishStatus("先用“选择”或“推拉”工具点选一个矩形面");
            return;
        }

        Document.PushPull(selectedFace, delta);
        PublishStatus(DescribeExtrusion(selectedFace));
        InvalidateVisual();
    }

    public void SetTool(SketchTool tool)
    {
        Tool = tool;
        pendingPoint = null;
        hoverPoint = null;
        PublishStatus(tool switch
        {
            SketchTool.Select => "选择工具：点击矩形面后可推拉",
            SketchTool.Line => "直线工具：点击两个位置创建一条线",
            SketchTool.Rectangle => "矩形工具：点击两个角点创建矩形面",
            SketchTool.PushPull => "推拉工具：点击矩形面，每次增加 1 m 高度",
            SketchTool.Measure => "测量工具：点击两个位置，使用 OCCT 计算距离",
            _ => string.Empty
        });
        InvalidateVisual();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Focus();

        var pointer = e.GetCurrentPoint(this);
        if (!pointer.Properties.IsLeftButtonPressed)
        {
            return;
        }

        var point = ToGroundPoint(pointer.Position);
        hoverPoint = point;

        switch (Tool)
        {
            case SketchTool.Select:
                selectedFace = FindFace(point);
                PublishStatus(selectedFace is null ? "没有选中实体" : $"已选择矩形面，高度 {selectedFace.Height:0.##} m");
                break;
            case SketchTool.Line:
                PlaceLinePoint(point);
                break;
            case SketchTool.Rectangle:
                PlaceRectanglePoint(point);
                break;
            case SketchTool.PushPull:
                selectedFace = FindFace(point);
                if (selectedFace is not null)
                {
                    Document.PushPull(selectedFace, 1);
                    PublishStatus(DescribeExtrusion(selectedFace));
                }
                else
                {
                    PublishStatus("请点击一个矩形面进行推拉");
                }

                break;
            case SketchTool.Measure:
                PlaceMeasurePoint(point);
                break;
        }

        InvalidateVisual();
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        hoverPoint = ToGroundPoint(e.GetPosition(this));
        InvalidateVisual();
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        Scale = Math.Clamp(Scale + e.Delta.Y * 3, 18, 72);
        InvalidateVisual();
    }

    private void PlaceLinePoint(SketchPoint point)
    {
        if (pendingPoint is null)
        {
            pendingPoint = point;
            PublishStatus($"线段起点: {FormatPoint(point)}");
            return;
        }

        Document.AddLine(pendingPoint.Value, point);
        var distance = SketchDocument.Distance(pendingPoint.Value, point);
        PublishStatus($"已创建线段，OCCT 长度 {distance:0.###} m");
        pendingPoint = null;
    }

    private void PlaceRectanglePoint(SketchPoint point)
    {
        if (pendingPoint is null)
        {
            pendingPoint = point;
            PublishStatus($"矩形第一个角点: {FormatPoint(point)}");
            return;
        }

        selectedFace = Document.AddRectangle(pendingPoint.Value, point);
        PublishStatus("已创建矩形面；切换到推拉工具可生成体块");
        pendingPoint = null;
    }

    private void PlaceMeasurePoint(SketchPoint point)
    {
        if (pendingPoint is null)
        {
            pendingPoint = point;
            PublishStatus($"测量起点: {FormatPoint(point)}");
            return;
        }

        var distance = SketchDocument.Distance(pendingPoint.Value, point);
        PublishStatus($"OCCT 测量距离: {distance:0.###} m");
        pendingPoint = null;
    }

    private void DrawGrid(DrawingContext context)
    {
        for (var i = -12; i <= 12; i++)
        {
            context.DrawLine(gridPen, Project(new SketchPoint(i, -12, 0)), Project(new SketchPoint(i, 12, 0)));
            context.DrawLine(gridPen, Project(new SketchPoint(-12, i, 0)), Project(new SketchPoint(12, i, 0)));
        }
    }

    private void DrawAxes(DrawingContext context)
    {
        var origin = Project(SketchPoint.Origin);
        context.DrawLine(xAxisPen, origin, Project(new SketchPoint(8, 0, 0)));
        context.DrawLine(yAxisPen, origin, Project(new SketchPoint(0, 8, 0)));
        context.DrawLine(zAxisPen, origin, Project(new SketchPoint(0, 0, 6)));
    }

    private void DrawEdges(DrawingContext context)
    {
        foreach (var edge in Document.Edges)
        {
            context.DrawLine(edgePen, Project(edge.Start), Project(edge.End));
        }
    }

    private void DrawFaces(DrawingContext context)
    {
        foreach (var face in Document.Faces)
        {
            var baseCorners = face.BaseCorners;
            DrawPolygon(context, baseCorners, FaceBrush, edgePen);

            if (face.Height <= 0)
            {
                continue;
            }

            var topCorners = face.TopCorners;
            DrawPolygon(context, [baseCorners[0], baseCorners[1], topCorners[1], topCorners[0]], SideFaceBrush, edgePen);
            DrawPolygon(context, [baseCorners[1], baseCorners[2], topCorners[2], topCorners[1]], SideFaceBrush, edgePen);
            DrawPolygon(context, [baseCorners[2], baseCorners[3], topCorners[3], topCorners[2]], SideFaceBrush, edgePen);
            DrawPolygon(context, topCorners, TopFaceBrush, edgePen);
        }
    }

    private void DrawPreview(DrawingContext context)
    {
        if (pendingPoint is null || hoverPoint is null)
        {
            return;
        }

        if (Tool is SketchTool.Line or SketchTool.Measure)
        {
            context.DrawLine(previewPen, Project(pendingPoint.Value), Project(hoverPoint.Value));
            return;
        }

        if (Tool == SketchTool.Rectangle)
        {
            var face = new SketchFace(pendingPoint.Value, hoverPoint.Value);
            DrawPolygon(context, face.BaseCorners, PreviewBrush, previewPen);
        }
    }

    private void DrawPolygon(DrawingContext context, IReadOnlyList<SketchPoint> points, IBrush fill, Pen stroke)
    {
        if (points.Count < 3)
        {
            return;
        }

        var geometry = new StreamGeometry();
        using (var stream = geometry.Open())
        {
            stream.BeginFigure(Project(points[0]), isFilled: true);
            for (var i = 1; i < points.Count; i++)
            {
                stream.LineTo(Project(points[i]));
            }

            stream.EndFigure(isClosed: true);
        }

        context.DrawGeometry(fill, stroke, geometry);
    }

    private SketchFace? FindFace(SketchPoint point)
    {
        return Document.Faces.LastOrDefault(face => face.ContainsGroundPoint(point));
    }

    private Point Project(SketchPoint point)
    {
        var centerX = Bounds.Width * 0.5;
        var centerY = Bounds.Height * 0.62;
        var screenX = centerX + (point.X - point.Y) * Scale;
        var screenY = centerY + (point.X + point.Y) * Scale * 0.5 - point.Z * Scale;
        return new Point(screenX, screenY);
    }

    private SketchPoint ToGroundPoint(Point point)
    {
        var centerX = Bounds.Width * 0.5;
        var centerY = Bounds.Height * 0.62;
        var a = (point.X - centerX) / Scale;
        var b = (point.Y - centerY) / (Scale * 0.5);

        var x = (a + b) * 0.5;
        var y = (b - a) * 0.5;

        return new SketchPoint(Math.Round(x * 2) / 2, Math.Round(y * 2) / 2, 0);
    }

    private void PublishStatus(string message)
    {
        StatusChanged?.Invoke(this, new SketchStatusChangedEventArgs(message, Document.EntityCount));
    }

    private static string DescribeExtrusion(SketchFace face)
    {
        if (face.Height <= 0 || face.SizeX <= 0 || face.SizeY <= 0)
        {
            return $"推拉高度: {face.Height:0.##} m";
        }

        using var box = new OcctBox(face.SizeX, face.SizeY, face.Height);
        var bounds = box.BoundingBox;
        return $"OCCT Box: {bounds.SizeX:0.##} x {bounds.SizeY:0.##} x {bounds.SizeZ:0.##} m";
    }

    private static string FormatPoint(SketchPoint point)
    {
        return $"({point.X:0.##}, {point.Y:0.##}, {point.Z:0.##})";
    }
}

public sealed class SketchStatusChangedEventArgs : EventArgs
{
    public SketchStatusChangedEventArgs(string message, int entityCount)
    {
        Message = message;
        EntityCount = entityCount;
    }

    public string Message { get; }

    public int EntityCount { get; }
}
