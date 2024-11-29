using Dotnetable.Data.DataAccess;
using Dotnetable.Shared.DTO.File;
using Dotnetable.Shared.DTO.Public;

namespace Dotnetable.Service;
public class FileService
{

    public async Task<FileFetchFromDBResponse> Fetch(Guid flieCode)
    {
        return await FileDataAccess.Fetch(flieCode);
    }

    public static async Task<PublicActionResponse> Remove(FileRemoveRequest requestModel)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.PostManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        string fileURL = $"{Directory.GetCurrentDirectory()}/FileLibrary/{requestModel.FileCategoryID}/{requestModel.FilePath}/{requestModel.FileCode}";

        if (!File.Exists(fileURL))
            return new() { ErrorException = new() { ErrorCode = "F1" } };        

        try
        {
            File.Delete(fileURL);
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "F2", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }

    public async Task<PublicActionResponse> Insert(FileInsertRequest requestModel)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.PostManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        string fileURL = $"{Directory.GetCurrentDirectory()}\\FileLibrary\\{requestModel.FileCategoryID}/{requestModel.FilePath}";

        try
        {
            if (!Directory.Exists($"{Directory.GetCurrentDirectory()}\\FileLibrary"))
                Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\FileLibrary");

            if (!Directory.Exists($"{Directory.GetCurrentDirectory()}\\FileLibrary\\{requestModel.FileCategoryID}"))
                Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\FileLibrary\\{requestModel.FileCategoryID}");

            if (!Directory.Exists(fileURL))
                Directory.CreateDirectory(fileURL);

            if (File.Exists($"{fileURL}\\{requestModel.FileCode}"))
                File.Delete($"{fileURL}\\{requestModel.FileCode}");

            await File.WriteAllBytesAsync($"{fileURL}\\{requestModel.FileCode}", requestModel.FileStream).ConfigureAwait(false);
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "F2", Message = x.Message } };
        }

        if (requestModel.FilePath == "TMP")
        {
            var fileList = await FileDataAccess.ExpiredTemporaryFiles();
            if (fileList is not null && fileList.Count > 0)
                foreach (var f in fileList)
                    File.Delete($"{Directory.GetCurrentDirectory()}\\FileLibrary\\0\\{requestModel.FilePath}/{f}");
        }

        return await FileDataAccess.Insert(requestModel);
    }

    public static async Task<PublicActionResponse> FileMoveFromTMPFolder(FileMoveFromTMPFolderRequest requestModel)
    {
        if (!await AuthenticationDataAccess.UserValidatePolicyServiceLayer(requestModel.CurrentMemberID, nameof(MemberRole.PostManager)))
            return new() { ErrorException = new() { ErrorCode = "C19", Message = "No Policy on this action" } };

        string mainPath = $"{Directory.GetCurrentDirectory()}\\FileLibrary\\";
        if (!File.Exists($"{mainPath}0\\TMP\\{requestModel.FileCode}"))
            return new() { ErrorException = new() { ErrorCode = "F1" } };

        try
        {
            if (!Directory.Exists($"{mainPath}{requestModel.NewFileCategory}"))
                Directory.CreateDirectory($"{mainPath}{requestModel.NewFileCategory}");

            if (!Directory.Exists($"{mainPath}{requestModel.NewFileCategory}\\{requestModel.NewFilePath}"))
                Directory.CreateDirectory($"{mainPath}{requestModel.NewFileCategory}\\{requestModel.NewFilePath}");

            File.Move($"{mainPath}0\\TMP\\{requestModel.FileCode}", $"{mainPath}{requestModel.NewFileCategory}\\{requestModel.NewFilePath}\\{requestModel.FileCode}");
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "SX", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }


}