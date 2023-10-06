using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.File;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Dotnetable.Admin.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Route("Api/[controller]")]
public class FilesController : ControllerBase
{
    #region CTOR
    private readonly FileService _file;
    private IConfiguration _config;
    public FilesController(FileService file, IConfiguration config)
    {
        _file = file;
        _config = config;
    }
    #endregion

    [AllowAnonymous]
    [ResponseCache(VaryByHeader = "User-Agent", Duration = 300000, Location = ResponseCacheLocation.Client)]
    [HttpGet("Receive/{ImageSize}/{id}/{name}")]
    public async Task<IActionResult> ReceiveFileWithResize(string ImageSize, string id, string name)
    {
        if (!Guid.TryParse(id, out Guid FileStreamID) || string.IsNullOrEmpty(name) || name == "") return BadRequest();
        string FileType = name.Split('.').LastOrDefault().ToLower();
        var FileDetail = await _file.Fetch(new(id));
        if (FileDetail is null) return BadRequest();
        string FileURL = $"{Directory.GetCurrentDirectory()}/FileLibrary/{FileDetail.FileCategoryID}/{FileDetail.FilePath}/{id}";
        if (!System.IO.File.Exists(FileURL)) return BadRequest();
        var CurrentFileStream = await System.IO.File.ReadAllBytesAsync(FileURL);

        //byte[] buffer = null;
        //using (FileStream fs = new FileStream(FileURL, FileMode.Open, FileAccess.Read))
        //{
        //    buffer = new byte[fs.Length];
        //    fs.Read(buffer, 0, (int)fs.Length);
        //}


        var AllowedFileType = new List<string>() { "bmp", "jpeg", "jpg", "png", "gif" };
        if (!(from i in AllowedFileType where i == FileType select i).Any()) return new FileStreamResult(new MemoryStream(CurrentFileStream), new MediaTypeHeaderValue(FileDetail.MIMEType));

        ImageSize = ImageSize.ToLower();
        if (ImageSize == "original") return new FileStreamResult(new MemoryStream(CurrentFileStream), new MediaTypeHeaderValue(FileDetail.MIMEType));

        var ImageSizes = ImageSize.Split('x');
        if (ImageSizes.Length != 2 || !int.TryParse(ImageSizes[0], out int CropImgWidth) || !int.TryParse(ImageSizes[1].Replace("g", "").Replace("c", ""), out int CropImgHeight))
            return new FileStreamResult(new MemoryStream(CurrentFileStream), new MediaTypeHeaderValue(FileDetail.MIMEType));
        try
        {
            var stream = new MemoryStream();
            using (Image<Rgba32> image = Image.Load<Rgba32>(CurrentFileStream))
            {
                int ImgSizeWidth = image.Width;
                int ImgSizeHeight = image.Height;

                //int CropImgWidth = Convert.ToInt32(ImageSizes[0]);
                //int CropImgHeight = Convert.ToInt32(ImageSizes[1].Replace("g", "").Replace("c", ""));

                if (ImgSizeWidth < CropImgWidth) CropImgWidth = ImgSizeWidth;
                if (ImgSizeHeight < CropImgHeight) CropImgHeight = ImgSizeHeight;

                //int ResizeImgWidth = 0;
                //int ResizeImgHeight = 0;

                //if (ImgSizeHeight < ImgSizeWidth)
                //{
                //    ResizeImgHeight = CropImgHeight;
                //    ResizeImgWidth = (CropImgHeight * ImgSizeWidth) / ImgSizeHeight;
                //}
                //else if (ImgSizeWidth < ImgSizeHeight)
                //{
                //    ResizeImgWidth = CropImgWidth;
                //    ResizeImgHeight = (CropImgWidth * ImgSizeHeight) / ImgSizeWidth;
                //}
                //else
                //{
                //    ResizeImgHeight = CropImgHeight;
                //    ResizeImgWidth = CropImgWidth;
                //}

                //int XPoint = (ResizeImgWidth - CropImgWidth) / 2;
                //int YPoint = (ResizeImgHeight - CropImgHeight) / 2;

                image.Mutate(
                    x => x.Resize(new ResizeOptions()
                    {
                        Mode = ImageSize.Contains('c') ? ResizeMode.Crop : ResizeMode.Pad,
                        Size = new Size(CropImgWidth, CropImgHeight)
                    })
                //.Resize(ResizeImgWidth, ResizeImgHeight)
                //.Crop(new SixLabors.Primitives.Rectangle() { Width = CropImgWidth, Height = CropImgHeight, X = XPoint, Y = YPoint })
                );

                if (ImageSize.Contains('g')) image.Mutate(x => x.Grayscale());
                image.SaveAsPng(stream);
                //image.Save(stream, format);
            }
            return new FileStreamResult(new MemoryStream(stream.ToArray()), new MediaTypeHeaderValue(FileDetail.MIMEType));
        }
        catch (Exception)
        {
            return new FileStreamResult(new MemoryStream(CurrentFileStream), new MediaTypeHeaderValue(FileDetail.MIMEType));
        }

    }


    [Authorize(nameof(MemberRole.PostManager))]
    [RequestSizeLimit(10_000_000)]
    [HttpPost("UploadImage")]
    public async Task<ActionResult<FileUploadImageCKEditorResponse>> UploadImage()
    {
        var UploadedFiles = Request.Form.Files;
        if (UploadedFiles is null) return null;

        var FileStream = UploadedFiles[0].OpenReadStream();
        var FileBytes = new byte[FileStream.Length];
        FileStream.Read(FileBytes, 0, Convert.ToInt32(FileStream.Length));
        string FileName = UploadedFiles[0].FileName.FileNameCorrection();

        int MemberID = await Tools.GetRequesterMemberID(Request, _config);
        Guid FileCode = Guid.NewGuid();
        var responseData = await _file.Insert(new()
        {
            FileCategoryID = 0,
            FileCode = FileCode.ToString(),
            FileName = FileName,
            FileStream = FileBytes,
            FilePath = "TMP",
            UploaderID = MemberID
        });

        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;

        if (responseData is not null && responseData.ErrorException is null && responseData.SuccessAction)
        {
            return new FileUploadImageCKEditorResponse() { ImageUrl = $"{(Request.IsHttps ? "https" : "http")}://{Request.Host}/api/Files/Receive/original/{FileCode}/{FileName}", FileCode = FileCode.ToString(), Success = true };
        }
        else
        {
            return new FileUploadImageCKEditorResponse() { ErrorException = responseData.ErrorException ?? new() { ErrorCode = "SX" }, Success = false };
        }
    }


    [Authorize(nameof(MemberRole.PostManager))]
    [RequestSizeLimit(10_000_000)]
    [HttpPost("UploadTemporary")]
    public async Task<ActionResult<PublicControllerResponse>> UploadTemporary(FileTemporaryUploadRequest requestModel)
    {
        int MemberID = await Tools.GetRequesterMemberID(Request, _config);
        Guid FileCode = Guid.NewGuid();
        if (!string.IsNullOrEmpty(requestModel.FileCode) && requestModel.FileCode != "")
            Guid.TryParse(requestModel.FileCode, out FileCode);

        var responseData = await _file.Insert(new()
        {
            FileCategoryID = 0,
            FileCode = FileCode.ToString(),
            FileName = requestModel.FileName,
            FileStream = requestModel.FileStream,
            FilePath = "TMP",
            UploaderID = MemberID
        });

        string responseExeption = responseData.ErrorException is null ? string.Empty : responseData.ErrorException.ToJsonString();
        responseData.ErrorException = null;
        return new PublicControllerResponse()
        {
            ResponseData = responseData,
            Success = responseData is not null && string.IsNullOrEmpty(responseExeption),
            ErrorException = string.IsNullOrEmpty(responseExeption) ? null : responseExeption.JsonToObject<ErrorExceptionResponse>()
        };
    }


}
