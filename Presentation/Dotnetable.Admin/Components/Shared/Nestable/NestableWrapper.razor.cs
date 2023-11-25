using Dotnetable.Admin.Models.Nestable;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Dotnetable.Admin.Components.Shared.Nestable;

public partial class NestableWrapper
{
    [Inject] private IJSRuntime _jsRuntime { get; set; }

    [Parameter] public List<NestableStandardRequest> Model { get; set; }
    [Parameter] public string CssClass { get; set; }
    [Parameter] public EventCallback<List<NestableStandardResponse>> ResponseModel { get; set; }
    [Parameter] public string MenuID { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }

    private string _nestableID { get; set; }

    //private string GenerateChilds(int? ParentID = null)
    //{
    //    if (Model is null || !(from i in Model where i.ParentID == ParentID select i.ItemID).Any()) return "";
    //    var ListString = new System.Text.StringBuilder();
    //    foreach (var j in Model.Where(i => i.ParentID == ParentID).OrderBy(i => i.Priority).ToList())
    //        ListString.Append($"<li class=\"dd-item dd3-item\" data-id=\"{j.ItemID}\"><div class=\"dd-handle dd3-handle\"></div><div class=\"dd3-content\">{j.Title}</div>{GenerateChilds(j.ItemID)}</li>");
    //    return ListString.ToString();
    //}

    protected override void OnInitialized()
    {
        _nestableID = $"NS_{Guid.NewGuid().ToString().Replace("-", "")[..20]}";
    }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _jsRuntime.InvokeVoidAsync("Plugin.SetDivNestable", _nestableID, DotNetObjectReference.Create(this));
        }
    }

    [JSInvokable]
    public async Task SetSortedItems(string listJson)
    {
        var parseObject = JsonSerializer.Deserialize<List<NestableResponse>>(listJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        List<NestableStandardResponse> finalList = new();

        if (parseObject != null && parseObject.Count > 0)
            finalList = ParseChild(parseObject, null);

        await ResponseModel.InvokeAsync(finalList);
        StateHasChanged();
    }

    private List<NestableStandardResponse> ParseChild(List<NestableResponse> childObjects, string parentID)
    {
        var responseItem = new List<NestableStandardResponse>();
        for (int i = 0; i < childObjects.Count; i++)
        {
            responseItem.Add(new NestableStandardResponse()
            {
                ItemID = childObjects[i].id.ToString(),
                ParentID = parentID,
                Priority = i
            });

            if (childObjects[i].children != null && childObjects[i].children.Count > 0)
            {
                responseItem.AddRange(ParseChild(childObjects[i].children, childObjects[i].id.ToString()));
            }
        }
        return responseItem;
    }

}
