using Dotnetable.Data.DBContext;
using Dotnetable.Shared.DTO.File;
using Dotnetable.Shared.DTO.Public;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Data.DataAccess;
public class FileDataAccess
{

    public static async Task<FileFetchFromDBResponse> Fetch(Guid fileCode)
    {
        using DotnetableEntity db = new();
        return await (from f in db.TB_Files
                      join ft in db.TB_File_Types on f.FileTypeID equals ft.FileTypeID
                      where f.FileCode == fileCode
                      select new FileFetchFromDBResponse { Approved = f.Approved, FileCategoryID = f.FileCategoryID, FileName = f.FileName, FilePath = f.FilePath, MIMEType = ft.MIMEType }).FirstOrDefaultAsync();
    }

    public static async Task<PublicActionResponse> Insert(FileInsertRequest requestModel)
    {
        using DotnetableEntity db = new();
        Guid fileCode = new(requestModel.FileCode);
        var fetchFile = await (from i in db.TB_Files where i.FileCode == fileCode select i).ToListAsync();

        if (fetchFile != null && fetchFile.Count > 0)
        {
            db.TB_Files.RemoveRange(fetchFile);
            await db.SaveChangesAsync();
        }

        var FileExt = requestModel.FileName.Split('.').LastOrDefault();
        var CheckEXT = await (from i in db.TB_File_Types where i.FileExtention == FileExt select i.FileTypeID).FirstOrDefaultAsync();
        if (CheckEXT < 0)
            return new() { ErrorException = new() { ErrorCode = "D3" } };


        var FileDetail = new TB_File()
        {
            FileCategoryID = requestModel.FileCategoryID,
            Approved = true,
            FileCode = fileCode,
            FileName = requestModel.FileName,
            FilePath = requestModel.FilePath,
            FileTypeID = CheckEXT,
            UploaderID = requestModel.UploaderID.Value,
            UploadTime = DateTime.Now
        };
        db.TB_Files.Add(FileDetail);
        await db.SaveChangesAsync();

        return new() { SuccessAction = true, ObjectID = FileDetail.FileID.ToString() };
    }


    public static async Task<List<Guid>> ExpiredTemporaryFiles()
    {
        using DotnetableEntity db = new();
        DateTime lastTime = DateTime.Now.AddDays(-1);
        byte fileCategoryID = (byte)Shared.DTO.Public.FileCategoryID.Temporary;
        var expiredFiles = await (from i in db.TB_Files where i.FileCategoryID == fileCategoryID && i.UploadTime < lastTime select i).ToListAsync();
        if (expiredFiles is null || expiredFiles.Count == 0)
            return null;

        var responseList = new List<Guid>();
        foreach (var j in expiredFiles)
        {
            responseList.Add(j.FileCode);
            db.TB_Files.Remove(j);
            db.Entry(j).State = EntityState.Deleted;
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception) { }

        return responseList;
    }



}