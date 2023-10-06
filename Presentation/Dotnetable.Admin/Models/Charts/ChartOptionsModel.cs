using System.Text.Json.Serialization;
using Dotnetable.Admin.Models.Charts.Annotations;
using Dotnetable.Admin.Models.Charts.Chart;
using Dotnetable.Admin.Models.Charts.DataLabels;
using Dotnetable.Admin.Models.Charts.Fill;
using Dotnetable.Admin.Models.Charts.ForecastDataPoints;
using Dotnetable.Admin.Models.Charts.Grid;
using Dotnetable.Admin.Models.Charts.Legend;
using Dotnetable.Admin.Models.Charts.NoData;
using Dotnetable.Admin.Models.Charts.PlotOptions;
using Dotnetable.Admin.Models.Charts.Responsive;
using Dotnetable.Admin.Models.Charts.Series;
using Dotnetable.Admin.Models.Charts.States;
using Dotnetable.Admin.Models.Charts.Stroke;
using Dotnetable.Admin.Models.Charts.Subtitle;
using Dotnetable.Admin.Models.Charts.Theme;
using Dotnetable.Admin.Models.Charts.Title;
using Dotnetable.Admin.Models.Charts.Tooltip;
using Dotnetable.Admin.Models.Charts.XAxis;
using Dotnetable.Admin.Models.Charts.YAxis;

namespace Dotnetable.Admin.Models.Charts;

public class ChartOptionsModel<TSeries, TXAxis>
{
    [JsonPropertyName("annotations")] public AnnotationsModel Annotations { get; set; } = new();
    [JsonPropertyName("chart")] public ChartModel Chart { get; set; } = new();
    //
    [JsonPropertyName("colors")]
    public List<string> Colors { get; set; } = new() {"var(--mud-palette-primary)", "var(--mud-palette-secondary)"};
    
    [JsonPropertyName("dataLabels")] public DataLabelsModel DataLabels { get; set; } = new();
    [JsonPropertyName("fill")] public FillModel Fill { get; set; } = new();
    
    [JsonPropertyName("forecastDataPoints")]
    public ForecastDataPointsModel ForecastDataPoints { get; set; } = new();

    // [JsonPropertyName("grid")] public GridModel Grid { get; set; } = new(); // TODO: Yaxis issue...
    [JsonPropertyName("labels")] public List<string> Labels { get; set; } = new();
    [JsonPropertyName("legend")] public LegendModel Legend { get; set; } = new();
    [JsonPropertyName("markers")] public LegendModel.MarkersModel Markers { get; set; } = new();
    [JsonPropertyName("noData")] public NoDataModel NoData { get; set; } = new();
    [JsonPropertyName("plotOptions")] public PlotOptionsModel PlotOptions { get; set; } = new();
    [JsonPropertyName("responsive")] public List<ResponsiveModel> Responsive { get; set; } = new();
    [JsonPropertyName("series")] public List<TSeries> Series { get; set; } = new();
    [JsonPropertyName("states")] public StatesModel States { get; set; } = new();
    [JsonPropertyName("stroke")] public StrokeModel Stroke { get; set; } = new();
    [JsonPropertyName("subtitle")] public SubtitleModel Subtitle { get; set; } = new();
    [JsonPropertyName("theme")] public ThemeModel Theme { get; set; } = new();
    [JsonPropertyName("title")] public TitleModel Title { get; set; } = new();
    [JsonPropertyName("tooltip")] public TooltipModel Tooltip { get; set; } = new();
    [JsonPropertyName("xaxis")] public XAxisModel<TXAxis> XAxis { get; set; } = new();
    [JsonPropertyName("yaxis")] public YAxisModel YAxis { get; set; } = new();
}