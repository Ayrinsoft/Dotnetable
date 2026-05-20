using Asp.Versioning;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.PixelFormats;
//using SixLabors.ImageSharp.Processing;
using Dotnetable.Admin.Models.DTO.File;
using Dotnetable.Admin.SharedServices;
using Dotnetable.Service;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SkiaSharp;

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
        //try
        //{
        //    var stream = new MemoryStream();
        //    using (Image<Rgba32> image = Image.Load<Rgba32>(CurrentFileStream))
        //    {
        //        int ImgSizeWidth = image.Width;
        //        int ImgSizeHeight = image.Height;

        //        if (ImgSizeWidth < CropImgWidth) CropImgWidth = ImgSizeWidth;
        //        if (ImgSizeHeight < CropImgHeight) CropImgHeight = ImgSizeHeight;

        //        image.Mutate(
        //            x => x.Resize(new ResizeOptions()
        //            {
        //                Mode = ImageSize.Contains('c') ? ResizeMode.Crop : ResizeMode.Pad,
        //                Size = new Size(CropImgWidth, CropImgHeight)
        //            })
        //        );

        //        if (ImageSize.Contains('g')) image.Mutate(x => x.Grayscale());
        //        image.SaveAsPng(stream);
        //    }
        //    return new FileStreamResult(new MemoryStream(stream.ToArray()), new MediaTypeHeaderValue(FileDetail.MIMEType));
        //}
        //catch (Exception)
        //{
        //    return new FileStreamResult(new MemoryStream(CurrentFileStream), new MediaTypeHeaderValue(FileDetail.MIMEType));
        //}

        try
        {
            // Load image from stream
            using var skStream = new SKManagedStream(new MemoryStream(CurrentFileStream));
            using var original = SKBitmap.Decode(skStream);

            int srcWidth = original.Width;
            int srcHeight = original.Height;

            if (CropImgWidth > srcWidth) CropImgWidth = srcWidth;
            if (CropImgHeight > srcHeight) CropImgHeight = srcHeight;

            bool isCrop = ImageSize.Contains('c');
            bool isGray = ImageSize.Contains('g');

            // --- Resize: Crop or Pad ---
            SKBitmap resized;
            if (isCrop)
            {
                float scaleX = (float)CropImgWidth / srcWidth;
                float scaleY = (float)CropImgHeight / srcHeight;
                float scale = Math.Max(scaleX, scaleY);         

                int scaledW = (int)Math.Ceiling(srcWidth * scale);
                int scaledH = (int)Math.Ceiling(srcHeight * scale);

                var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
                using var scaled = original.Resize(new SKImageInfo(scaledW, scaledH), sampling);

                int offsetX = (scaledW - CropImgWidth) / 2;
                int offsetY = (scaledH - CropImgHeight) / 2;

                resized = new SKBitmap(CropImgWidth, CropImgHeight);
                using var canvas = new SKCanvas(resized);
                canvas.DrawBitmap(scaled,
                    new SKRect(offsetX, offsetY,
                               offsetX + CropImgWidth,
                               offsetY + CropImgHeight),
                    new SKRect(0, 0, CropImgWidth, CropImgHeight));
            }
            else
            {
                float scale = Math.Min((float)CropImgWidth / srcWidth,
                                        (float)CropImgHeight / srcHeight);
                int fittedW = (int)(srcWidth * scale);
                int fittedH = (int)(srcHeight * scale);
                int padX = (CropImgWidth - fittedW) / 2;
                int padY = (CropImgHeight - fittedH) / 2;

                var sampling = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear);
                using var scaled = original.Resize(new SKImageInfo(fittedW, fittedH), sampling);

                resized = new SKBitmap(CropImgWidth, CropImgHeight);
                using var canvas = new SKCanvas(resized);
                canvas.Clear(SKColors.Black);                     
                canvas.DrawBitmap(scaled, padX, padY);
            }

            // --- Grayscale (optional) ---
            SKBitmap finalBitmap = resized;
            if (isGray)
            {
                var grayInfo = resized.Info.WithColorType(SKColorType.Gray8);
                finalBitmap = new SKBitmap(grayInfo);
                using var canvas = new SKCanvas(finalBitmap);

                // ColorMatrix that converts RGB → luminance
                using var paint = new SKPaint
                {
                    ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
                    {
                0.299f, 0.587f, 0.114f, 0, 0,
                0.299f, 0.587f, 0.114f, 0, 0,
                0.299f, 0.587f, 0.114f, 0, 0,
                0,      0,      0,      1, 0
                    })
                };
                canvas.DrawBitmap(resized, 0, 0, paint);
                resized.Dispose();
            }

            // --- Encode to PNG and return ---
            using var image = SKImage.FromBitmap(finalBitmap);
            using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
            finalBitmap.Dispose();

            var outStream = new MemoryStream();
            encoded.SaveTo(outStream);
            outStream.Position = 0;

            return new FileStreamResult(outStream,
                new MediaTypeHeaderValue(FileDetail.MIMEType));
        }
        catch (Exception)
        {
            return new FileStreamResult(
                new MemoryStream(CurrentFileStream),
                new MediaTypeHeaderValue(FileDetail.MIMEType));
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
