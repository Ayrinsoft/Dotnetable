using Dotnetable.Data.DataAccess;
using Dotnetable.Shared.DTO.Post;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.Extensions.Caching.Memory;

namespace Dotnetable.Service;
public class PostService
{
    private readonly IMemoryCache _mmc;
    public PostService(IMemoryCache mmc)
    {
        _mmc = mmc;
    }

    #region PostCategory
    public async Task<PublicActionResponse> PostCategoryInsert(PostCategoryInsertRequest requestModel)
    {
        return await PostDataAccess.PostCategoryInsert(requestModel);
    }

    public async Task<PostCategoryPublicListResponse> PublicPostCategoryList()
    {
        return await PostDataAccess.PublicPostCategoryList();
    }

    public async Task<PostCategoryListResponse> PostCategoryList()
    {
        return await PostDataAccess.PostCategoryList();
    }

    public async Task<PostCategoryDetailResponse> PostCategoryDetail(PostCategoryDetailRequest requestModel)
    {
        requestModel.LanguageCode = requestModel.LanguageCode.ToUpper();
        return await PostDataAccess.PostCategoryDetail(requestModel);
    }

    public async Task<PublicActionResponse> PostCategoryUpdate(PostCategoryUpdateRequest requestModel)
    {
        return await PostDataAccess.PostCategoryUpdate(requestModel);
    }

    public async Task<PublicActionResponse> PostCategoryChangeStatus(PostCategoryChangeStatusRequest requestModel)
    {
        return await PostDataAccess.PostCategoryChangeStatus(requestModel);
    }

    public async Task<PublicActionResponse> PostCategoryUpdatePriorityAndParent(List<PostCategoryUpdatePriorityAndParentRequest> requestModel)
    {
        return await PostDataAccess.PostCategoryUpdatePriorityAndParent(requestModel);
    }

    public async Task<PublicActionResponse> PostCategoryUpdateOtherLanguage(PostCategoryUpdateOtherLanguageRequest requestModel)
    {
        requestModel.LanguageCode = requestModel.LanguageCode.ToUpper();
        return await PostDataAccess.PostCategoryUpdateOtherLanguage(requestModel);
    }
    #endregion

    #region Post

    public async Task<PostListFetchResponse> AdminPostList(PostListFetchRequest requestModel)
    {
        requestModel.OrderbyParams = requestModel.OrderbyParams.CheckForInjection(new List<string>() { "PostID", "Title" });
        return await PostDataAccess.AdminPostList(requestModel);
    }

    public async Task<PublicActionResponse> Insert(PostInsertRequest requestModel)
    {
        var responseItem = await PostDataAccess.Insert(requestModel);
        if (responseItem.SuccessAction && requestModel.FileCodes is not null && requestModel.FileCodes.Count > 0)
            foreach (var j in requestModel.FileCodes)
                FileService.FileMoveFromTMPFolder(new() { FileCode = j, NewFileCategory = ((byte)FileCategoryID.Post).ToString(), NewFilePath = responseItem.ObjectID });

        return responseItem;
    }

    public async Task<PublicActionResponse> Update(PostUpdateRequest requestModel)
    {
        var responseItem = await PostDataAccess.Update(requestModel);
        if (responseItem.SuccessAction && requestModel.FileCodes is not null && requestModel.FileCodes.Count > 0)
            foreach (var j in requestModel.FileCodes)
                FileService.FileMoveFromTMPFolder(new() { FileCode = j, NewFileCategory = ((byte)FileCategoryID.Post).ToString(), NewFilePath = requestModel.PostID.ToString() });

        return responseItem;
    }

    public async Task<PublicActionResponse> PostAddLanguage(PostUpdateRequest requestModel)
    {
        requestModel.LanguageCode = requestModel.LanguageCode.ToUpper();
        return await PostDataAccess.PostAddLanguage(requestModel);
    }

    public async Task<PostLanguageDetailResponse> PostLanguageDetail(PostLanguageDetailRequest requestModel)
    {
        requestModel.LanguageCode = requestModel.LanguageCode.ToUpper();
        return await PostDataAccess.PostLanguageDetail(requestModel);
    }

    public async Task<PublicActionResponse> PostDeleteLangauge(PostLanguageDeleteRequest requestModel)
    {
        requestModel.LanguageCode = requestModel.LanguageCode.ToUpper();
        return await PostDataAccess.PostDeleteLangauge(requestModel);
    }

    public async Task<PostDetailResponse> AdminDetail(PostDetailRequest requestModel)
    {
        return await PostDataAccess.AdminDetail(requestModel);
    }

    public async Task<PublicActionResponse> RemovePostFile(PostFileRemoveRequest requestModel)
    {
        var dbResponse = await PostDataAccess.RemovePostFile(requestModel);
        if (dbResponse is null || !dbResponse.SuccessAction || dbResponse.ErrorException is not null) return dbResponse;

        FileService.Remove(new() { FileCode = requestModel.FileCode, FileCategoryID = (byte)FileCategoryID.Post, FilePath = requestModel.PostID.ToString() });
        return dbResponse;
    }

    public async Task<PublicActionResponse> ChangeStatus(PostChangeStatusRequest requestModel)
    {
        return await PostDataAccess.ChangeStatus(requestModel);
    }

    public async Task<PublicActionResponse> ContactusUpdate(ContactUsUpdateRequest requestModel)
    {
        requestModel.PublicPostDetail.LanguageCode = requestModel.PublicPostDetail.LanguageCode.ToUpper();
        return await PostDataAccess.PublicPostUpdate(new PostPublicUpdateRequest() { PostCode = "ContactUs", PublicPostDetail = requestModel.PublicPostDetail, FinalPostBody = requestModel.ContactUsDetail.ToJsonString() });
    }

    public async Task<PublicActionResponse> AboutusUpdate(AboutUsUpdateRequest requestModel)
    {
        requestModel.PublicPostDetail.LanguageCode = requestModel.PublicPostDetail.LanguageCode.ToUpper();
        return await PostDataAccess.PublicPostUpdate(new PostPublicUpdateRequest() { PostCode = "AboutUs", PublicPostDetail = requestModel.PublicPostDetail, FinalPostBody = requestModel.AboutusDetail.ToJsonString() });
    }

    public async Task<PublicActionResponse> QRCodeUpdate(QRCodeUpdateRequest requestModel)
    {
        requestModel.PublicPostDetail.LanguageCode = requestModel.PublicPostDetail.LanguageCode.ToUpper();
        return await PostDataAccess.PublicPostUpdate(new PostPublicUpdateRequest() { PostCode = "QRCode", PublicPostDetail = requestModel.PublicPostDetail, FinalPostBody = requestModel.QRCodeDetail.ToJsonString() });
    }

    #endregion



    public async Task<PostCategoryPublicDetailRsesponse> PublicPostCategoryDetail(PostCategoryPublicDetailRequest requestModel)
    {
        if (!_mmc.TryGetValue($"PublicPostCategoryDetail_{requestModel.PostCategoryID}", out List<PostCategoryPublicDetailRsesponse.PostCategoryDetail> cachedPostCategoryDetails))
        {
            var dbPostCatDetails = await PostDataAccess.PublicPostCategoryDetail(requestModel);
            if (dbPostCatDetails is null || dbPostCatDetails.ErrorException is not null || dbPostCatDetails.PostCategories is null || dbPostCatDetails.PostCategories.Count == 0)
                return new() { ErrorException = dbPostCatDetails?.ErrorException ?? new() { ErrorCode = "D0" } };

            cachedPostCategoryDetails = dbPostCatDetails.PostCategories;
                _mmc.Set($"PublicPostCategoryDetail_{requestModel.PostCategoryID}", cachedPostCategoryDetails, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(3)));
        }
        return new() { PostCategories = cachedPostCategoryDetails };
    }

    public async Task<PostListPublicResponse> PublicPostList(PostListPublicRequest requestModel)
    {
        if (!_mmc.TryGetValue($"PublicPostList_{requestModel.PostCategoryID}", out List<PostListPublicResponse.PostDetail> cachedPostList))
        {
            var postList = await PostDataAccess.PublicPostList(requestModel);
            if (postList is null || postList.ErrorException is not null || postList.Posts is null || postList.Posts.Count == 0)
                return new() { ErrorException = postList?.ErrorException ?? new() { ErrorCode = "D0" } };

            cachedPostList = postList.Posts;
            _mmc.Set($"PublicPostList_{requestModel.PostCategoryID}", cachedPostList, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(1)));
        }
        return new() { Posts = cachedPostList };
    }

    public async Task<PostDetailPublicResponse> PublicPostDetail(PostDetailPublicRequest requestModel)
    {
        PostDetailPublicResponse responseModel;
        if (requestModel.PostID.HasValue && requestModel.PostID.Value > 0)
        {
            if (!_mmc.TryGetValue($"PublicPostDetailID_{requestModel.PostID}", out List<PostDetailPublicResponse.PostDetails> cachedPostDetailByID))
            {
                var postDetail = await PostDataAccess.PublicPostDetailByID(requestModel.PostID.Value);
                if (postDetail is null || postDetail.ErrorException is not null || postDetail.PostsDetail is null || postDetail.PostsDetail.Count == 0)
                    return new() { ErrorException = postDetail?.ErrorException ?? new() { ErrorCode = "D0" } };

                cachedPostDetailByID = postDetail.PostsDetail;
                _mmc.Set($"PublicPostDetailID_{requestModel.PostID}", cachedPostDetailByID, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));
            }
            responseModel = new() { PostsDetail = cachedPostDetailByID };
        }
        else
        {
            if (!_mmc.TryGetValue($"PublicPostDetailCode_{requestModel.PostCode}", out List<PostDetailPublicResponse.PostDetails> cachedPostDetailByCode))
            {
                var postDetail = await PostDataAccess.PublicPostDetailByCode(requestModel.PostCode);
                if (postDetail is null || postDetail.ErrorException is not null || postDetail.PostsDetail is null || postDetail.PostsDetail.Count == 0)
                    return new() { ErrorException = postDetail?.ErrorException ?? new() { ErrorCode = "D0" } };

                cachedPostDetailByCode = postDetail.PostsDetail;
                _mmc.Set($"PublicPostDetailCode_{requestModel.PostCode}", cachedPostDetailByCode, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(15)));
            }
            responseModel = new() { PostsDetail = cachedPostDetailByCode };
        }

        return responseModel;
    }

    public async Task<PublicActionResponse> PublicPostAddVisitCount(PostDetailPublicRequest requestModel)
    {
        if ((string.IsNullOrEmpty(requestModel.PostCode) || requestModel.PostCode == "") && requestModel.PostID is null)
            return new() { ErrorException = new() { ErrorCode = "C0", Message = "Send PostID or PostCode" } };

        return await PostDataAccess.PublicPostAddVisitCount(requestModel);
    }


    #region SlideShow

    public async Task<PublicActionResponse> SlideShowInsert(SlideShowInsertRequest requestModel)
    {
        var responseModel = await PostDataAccess.SlideShowInsert(requestModel);
        if (responseModel.SuccessAction)
            FileService.FileMoveFromTMPFolder(new() { FileCode = requestModel.FileCode, NewFileCategory = "5", NewFilePath = responseModel.ObjectID });

        return responseModel;
    }

    public async Task<PublicActionResponse> SlideShowInsertLanguage(SlideShowInsertLanguageRequest requestModel)
    {
        return await PostDataAccess.SlideShowInsertLanguage(requestModel);
    }

    public async Task<PublicActionResponse> SlideShowRemoveLanguage(SlideShowRemoveLanguageRequest requestModel)
    {
        return await PostDataAccess.SlideShowRemoveLanguage(requestModel);
    }

    public async Task<SlideShowLanguageDetailResponse> SlideShowLanguageDetail(SlideShowLanguageDetailRequest requestModel)
    {
        return await PostDataAccess.SlideShowLanguageDetail(requestModel);
    }

    public async Task<SlideShowLanguageListResponse> SlideShowLanguageList(SlideShowLanguageListRequest requestModel)
    {
        return await PostDataAccess.SlideShowLanguageList(requestModel);
    }

    public async Task<PublicActionResponse> SlideShowUpdate(SlideShowUpdateRequest requestModel)
    {
        var responseModel = await PostDataAccess.SlideShowUpdate(requestModel);
        if (responseModel.SuccessAction && !string.IsNullOrEmpty(responseModel.ObjectID) && responseModel.ObjectID != "")
        {
            FileService.FileMoveFromTMPFolder(new() { FileCode = requestModel.FileCode, NewFileCategory = ((byte)FileCategoryID.SlideShow).ToString(), NewFilePath = requestModel.SlideShowID.ToString() });
            FileService.Remove(new() { FileCode = responseModel.ObjectID, FileCategoryID = (byte)FileCategoryID.SlideShow, FilePath = requestModel.SlideShowID.ToString() });
        }

        return responseModel;
    }

    public async Task<PublicActionResponse> SlideShowChangeStatus(SlideShowChangeStatusRequest requestModel)
    {
        return await PostDataAccess.SlideShowChangeStatus(requestModel);
    }

    public async Task<SlideShowDetailResponse> SlideShowDetail(SlideShowDetailRequest requestModel)
    {
        return await PostDataAccess.SlideShowDetail(requestModel);
    }

    public async Task<SlideShowListResponse> SlideShowList(SlideShowListRequest requestModel)
    {
        requestModel.OrderbyParams = requestModel.OrderbyParams.CheckForInjection(new List<string>() { "SlideShowID", "Title", "PageCode" });
        return await PostDataAccess.SlideShowList(requestModel);
    }

    public async Task<SlideShowWebsiteShowResponse> SlideShowSite(SlideShowWebsiteShowRequest requestModel)
    {
        requestModel.PageCode = requestModel.PageCode.ToUpper();
        return await PostDataAccess.SlideShowSite(requestModel);
    }

    public async Task<PublicActionResponse> RemoveSlideShowFile(SlideShowRemoveFileRequest requestModel)
    {
        var dbResponse = await PostDataAccess.RemoveSlideShowFile(requestModel);
        if (dbResponse is null || !dbResponse.SuccessAction || dbResponse.ErrorException is not null) return dbResponse;

        FileService.Remove(new() { FileCode = dbResponse.ObjectID, FileCategoryID = (byte)FileCategoryID.SlideShow, FilePath = requestModel.SlideShowID.ToString() });
        return dbResponse;
    }

    #endregion


}