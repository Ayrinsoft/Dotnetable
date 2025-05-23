﻿using Dotnetable.Shared.DTO.Post;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dotnetable.Admin.Components.PageComponents.Posts.SlideShow;

public partial class SlideshowLanguageDialog
{
    [CascadingParameter] IMudDialogInstance MudDialog { get; set; }

    [Parameter] public SlideShowInsertLanguageRequest FormModel { get; set; }

    private void SubmitDialog(SlideShowInsertLanguageRequest requestModel) => MudDialog.Close(requestModel);

}
