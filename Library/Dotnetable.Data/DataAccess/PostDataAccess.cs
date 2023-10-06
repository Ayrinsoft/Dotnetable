using Dotnetable.Data.DBContext;
using Dotnetable.Shared.DTO.Post;
using Dotnetable.Shared.DTO.Public;
using Dotnetable.Shared.Tools;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Data.DataAccess;
public class PostDataAccess
{

    #region PostCategory
    public static async Task<PublicActionResponse> PostCategoryInsert(PostCategoryInsertRequest requestModel)
    {
        using DotnetableEntity db = new();

        if (await (from i in db.TB_Post_Categories where i.Title == requestModel.Title select i.PostCategoryID).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "C7" } };

        string fetchLanguage = (await WebsiteDataAccess.FetchSettingByKey("DEFAULT_LANGUAGE_CODE")) ?? "EN";

        var postCategoryModel = new TB_Post_Category()
        {
            FooterView = requestModel.FooterView,
            MenuView = requestModel.MenuView,
            MetaDescription = requestModel.MetaDescription,
            MetaKeywords = requestModel.MetaKeywords,
            ParentID = requestModel.ParentID,
            Priority = 20,
            Tags = requestModel.Tags,
            Title = requestModel.Title,
            Active = true,
            Description = requestModel.Description,
            LanguageCode = fetchLanguage
        };
        db.TB_Post_Categories.Add(postCategoryModel);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new()
        {
            SuccessAction = true,
            ObjectID = postCategoryModel.PostCategoryID.ToString()
        };
    }

    public static async Task<PostCategoryPublicListResponse> PublicPostCategoryList()
    {
        using DotnetableEntity db = new();
        var publicPostCategories = await (from pc in db.TB_Post_Categories
                                          where pc.Active && pc.MenuView
                                          select new PostCategoryPublicListResponse.PostCategoryDetail { Description = pc.Description, LanguageCode = pc.LanguageCode, ParentID = pc.ParentID, PostCategoryID = pc.PostCategoryID, Priority = pc.Priority, Title = pc.Title })
                                          .Concat(from pc in db.TB_Post_Categories
                                                  join lc in db.TB_Post_Category_Languages on pc.PostCategoryID equals lc.PostCategoryID
                                                  where pc.Active && pc.MenuView
                                                  select new PostCategoryPublicListResponse.PostCategoryDetail { Description = lc.Description, LanguageCode = lc.LanguageCode, ParentID = pc.ParentID, PostCategoryID = pc.PostCategoryID, Priority = pc.Priority, Title = lc.Title }).OrderBy(i => i.Priority).ToListAsync();

        return new() { PostCategories = publicPostCategories };
    }


    public static async Task<PostCategoryListResponse> PostCategoryList()
    {
        using DotnetableEntity db = new();
        var postCategories = await (from pc in db.TB_Post_Categories
                                    where pc.Title != "Websiteobjects"
                                    select new PostCategoryListResponse.PostCategoryDetail { LanguageCode = pc.LanguageCode, ParentID = pc.ParentID, PostCategoryID = pc.PostCategoryID, Priority = pc.Priority, Title = pc.Title, Active = pc.Active })
                                          .Concat(from pc in db.TB_Post_Categories
                                                  join lc in db.TB_Post_Category_Languages on pc.PostCategoryID equals lc.PostCategoryID
                                                  where pc.Title != "Websiteobjects"
                                                  select new PostCategoryListResponse.PostCategoryDetail { LanguageCode = lc.LanguageCode, ParentID = pc.ParentID, PostCategoryID = pc.PostCategoryID, Priority = pc.Priority, Title = lc.Title, Active = pc.Active }).ToListAsync();

        return new() { PostCategories = postCategories };
    }



    public static async Task<PostCategoryDetailResponse> PostCategoryDetail(PostCategoryDetailRequest requestModel)
    {
        using DotnetableEntity db = new();

        if (await (from i in db.TB_Post_Categories where i.PostCategoryID == requestModel.PostCategoryID && i.LanguageCode == requestModel.LanguageCode select i).AnyAsync())
            return await (from pc in db.TB_Post_Categories
                          where pc.PostCategoryID == requestModel.PostCategoryID && pc.LanguageCode == requestModel.LanguageCode
                          select new PostCategoryDetailResponse { LanguageCode = pc.LanguageCode, PostCategoryID = pc.PostCategoryID, Priority = pc.Priority, Title = pc.Title, Active = pc.Active, Description = pc.Description, FooterView = pc.FooterView, MenuView = pc.FooterView, MetaDescription = pc.MetaDescription, MetaKeywords = pc.MetaKeywords, Tags = pc.Tags }).FirstOrDefaultAsync();
        else

            return await (from pc in db.TB_Post_Categories
                          join lc in db.TB_Post_Category_Languages on pc.PostCategoryID equals lc.PostCategoryID
                          where pc.PostCategoryID == requestModel.PostCategoryID && lc.LanguageCode == requestModel.LanguageCode
                          select new PostCategoryDetailResponse { LanguageCode = lc.LanguageCode, PostCategoryID = pc.PostCategoryID, Priority = pc.Priority, Title = lc.Title, Active = pc.Active, Description = lc.Description, FooterView = pc.FooterView, MenuView = pc.FooterView, MetaDescription = lc.MetaDescription, MetaKeywords = lc.MetaKeywords, Tags = lc.Tags }).FirstOrDefaultAsync();
    }

    public static async Task<PublicActionResponse> PostCategoryUpdate(PostCategoryUpdateRequest requestModel)
    {
        using DotnetableEntity db = new();
        if ((await (from i in db.TB_Post_Categories where i.Title == requestModel.Title && i.PostCategoryID != requestModel.PostCategoryID select i.PostCategoryID).AnyAsync()))
            return new() { ErrorException = new() { ErrorCode = "C7" } };


        var postCategoryModel = await (from i in db.TB_Post_Categories where i.PostCategoryID == requestModel.PostCategoryID select i).FirstOrDefaultAsync();
        if (postCategoryModel is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };


        postCategoryModel.MenuView = requestModel.MenuView;
        postCategoryModel.FooterView = requestModel.FooterView;
        postCategoryModel.Tags = requestModel.Tags;
        postCategoryModel.MetaDescription = requestModel.MetaDescription;
        postCategoryModel.MetaKeywords = requestModel.MetaKeywords;
        postCategoryModel.Title = requestModel.Title;
        postCategoryModel.Description = requestModel.Description;

        try
        {
            db.Entry(postCategoryModel).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = ex.Message } };
        }

        return new() { SuccessAction = true, ObjectID = requestModel.PostCategoryID.ToString() };
    }

    public static async Task<PublicActionResponse> PostCategoryChangeStatus(PostCategoryChangeStatusRequest requestModel)
    {
        using DotnetableEntity db = new();
        var postCategoryModel = await (from i in db.TB_Post_Categories where i.PostCategoryID == requestModel.PostCategoryID select i).FirstOrDefaultAsync();
        if (postCategoryModel is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };


        postCategoryModel.Active = !postCategoryModel.Active;

        try
        {
            db.Entry(postCategoryModel).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = ex.Message } };
        }

        return new() { SuccessAction = true, ObjectID = requestModel.PostCategoryID.ToString() };
    }

    public static async Task<PublicActionResponse> PostCategoryUpdatePriorityAndParent(List<PostCategoryUpdatePriorityAndParentRequest> requestModel)
    {
        using DotnetableEntity db = new();
        var postCategoryIDs = requestModel.Select(i => i.PostCategoryID).ToList();
        var postcats = await (from i in db.TB_Post_Categories where postCategoryIDs.Any(c => c == i.PostCategoryID) select i).ToListAsync();

        foreach (var j in requestModel)
        {
            var fetchDbItem = (from i in postcats where i.PostCategoryID == j.PostCategoryID select i).FirstOrDefault();
            fetchDbItem.Priority = j.Priority;
            fetchDbItem.ParentID = j.ParentID;
            db.Entry(fetchDbItem).State = EntityState.Modified;
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }

    public static async Task<PublicActionResponse> PostCategoryUpdateOtherLanguage(PostCategoryUpdateOtherLanguageRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchFromDB = await (from i in db.TB_Post_Category_Languages where i.PostCategoryID == requestModel.PostCategoryID && i.LanguageCode == requestModel.LanguageCode select i).FirstOrDefaultAsync();
        if (fetchFromDB is null)
        {
            db.TB_Post_Category_Languages.Add(new TB_Post_Category_Language()
            {
                Description = requestModel.Description,
                LanguageCode = requestModel.LanguageCode,
                MetaDescription = requestModel.MetaDescription,
                MetaKeywords = requestModel.MetaKeywords,
                PostCategoryID = requestModel.PostCategoryID,
                Tags = requestModel.Tags,
                Title = requestModel.Title
            });

            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception x)
            {
                return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
            }
        }
        else
        {
            fetchFromDB.Title = requestModel.Title;
            fetchFromDB.Tags = requestModel.Tags;
            fetchFromDB.MetaKeywords = requestModel.MetaKeywords;
            fetchFromDB.MetaDescription = requestModel.MetaDescription;
            fetchFromDB.Description = requestModel.Description;

            try
            {
                db.Entry(fetchFromDB).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            catch (Exception x)
            {
                return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
            }
        }

        return new() { SuccessAction = true };
    }


    #endregion

    #region Post

    public static async Task<PostListFetchResponse> AdminPostList(PostListFetchRequest requestModel)
    {
        using DotnetableEntity db = new();

        var reportQuery = db.TB_Posts.Join(db.TB_Post_Categories, p => p.PostCategoryID, pc => pc.PostCategoryID, (p, pc) => new { p.PostID, PostCategoryName = pc.Title, p.PostCategoryID, ModifyDate = p.LogTime, p.Title, p.Summary, p.Tags, p.MetaKeywords, p.MetaDescription, p.Active, p.MemberID }).Where(i => i.PostCategoryName != "Websiteobjects").AsQueryable();

        if (!string.IsNullOrEmpty(requestModel.Title) && requestModel.Title != "")
            reportQuery = reportQuery.Where(i => i.Title.Contains(requestModel.Title));


        int dbCount = await reportQuery.CountAsync();

        if (string.IsNullOrEmpty(requestModel.OrderbyParams) || requestModel.OrderbyParams == "")
            reportQuery = reportQuery.OrderBy(i => i.PostID);
        else
            reportQuery = reportQuery.OrderUsingSortExpression(requestModel.OrderbyParams);


        var finalQuery = (from p in reportQuery
                          join m in db.TB_Members on p.MemberID equals m.MemberID
                          select new { ModifierName = (m.Givenname + " " + m.Surname), p.PostID, p.PostCategoryName, p.PostCategoryID, p.ModifyDate, p.Title, p.Summary, p.Tags, p.MetaKeywords, p.MetaDescription, p.Active, p.MemberID, LanguageCodes = (from i in db.TB_Post_Languages.DefaultIfEmpty() where i.PostID == p.PostID select i.LanguageCode) });

        var fetchList = await finalQuery.Skip(requestModel.SkipCount).Take(requestModel.TakeCount).Select(i => new PostListFetchResponse.PostDetail { Active = i.Active, PostID = i.PostID, LanguageCodes = string.Join(", ", i.LanguageCodes.Select(x => x)), MetaDescription = i.MetaDescription, MetaKeywords = i.MetaKeywords, ModifierName = i.ModifierName, ModifyDate = i.ModifyDate, PostCategoryID = i.PostCategoryID, PostCategoryName = i.PostCategoryName, Summary = i.Summary, Tags = i.Tags, Title = i.Title }).ToListAsync();

        return new() { Posts = fetchList, DatabaseRecords = dbCount };
    }


    public static async Task<PublicActionResponse> Insert(PostInsertRequest requestModel)
    {
        using DotnetableEntity db = new();

        if (await (from i in db.TB_Posts where i.Title == requestModel.Title select i.PostCategoryID).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "C7" } };

        string fetchLanguage = (await WebsiteDataAccess.FetchSettingByKey("DEFAULT_LANGUAGE_CODE")) ?? "EN";

        var postModel = new TB_Post()
        {
            Active = requestModel.Active,
            Title = requestModel.Title,
            Body = requestModel.Body,
            Summary = requestModel.Summary,
            LogTime = DateTime.Now,
            PostCategoryID = requestModel.PostCategoryID,
            MemberID = requestModel.CurrentMemberID.Value,
            MetaDescription = requestModel.MetaDescription,
            MetaKeywords = requestModel.MetaKeywords,
            LanguageCode = fetchLanguage,
            NormalBody = true,
            PostCode = "",
            Tags = requestModel.Tags
        };

        if (requestModel.MainImage.HasValue) postModel.FileCode = requestModel.MainImage;

        db.TB_Posts.Add(postModel);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        if (requestModel.FileCodes is not null && requestModel.FileCodes.Count > 0)
        {
            foreach (var j in requestModel.FileCodes)
            {
                Guid fileCode = new(j);
                var fetchDBFile = await (from i in db.TB_Files where i.FileCode == fileCode select i).FirstOrDefaultAsync();
                if (fetchDBFile is null) continue;
                fetchDBFile.FileCategoryID = (byte)FileCategoryID.Post;
                fetchDBFile.FilePath = postModel.PostID.ToString();
                db.Entry(fetchDBFile).State = EntityState.Modified;

                try
                {
                    db.TBM_Post_Files.Add(new() { FileID = fetchDBFile.FileID, PostID = postModel.PostID, ShowGallery = false });
                    await db.SaveChangesAsync();
                }
                catch (Exception) { }
            }
        }

        return new()
        {
            SuccessAction = true,
            ObjectID = postModel.PostID.ToString()
        };
    }

    public static async Task<PostDetailResponse> AdminDetail(PostDetailRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchPost = await (from i in db.TB_Posts where i.PostID == requestModel.PostID select new PostDetailResponse { Active = i.Active, Body = i.Body, ErrorException = null, MetaDescription = i.MetaDescription, PostCategoryID = i.PostCategoryID, MetaKeywords = i.MetaKeywords, PostID = i.PostID, Summary = i.Summary, Tags = i.Tags, Title = i.Title, LanguageCode = i.LanguageCode, MainImage = i.FileCode }).FirstOrDefaultAsync();
        if (fetchPost is null)
            return new PostDetailResponse() { ErrorException = new() { ErrorCode = "D0" } };

        fetchPost.FileList = await (from i in db.TBM_Post_Files join j in db.TB_Files on i.FileID equals j.FileID where i.PostID == requestModel.PostID select new PostDetailResponse.PostFiles { FileCode = j.FileCode, FileName = j.FileName }).ToListAsync();

        return fetchPost;
    }

    public static async Task<PublicActionResponse> RemoveSlideShowFile(SlideShowRemoveFileRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchSlideShowFile = await (from i in db.TB_SlideShows where i.SlideShowID == requestModel.SlideShowID select new { i.FileCode }).FirstOrDefaultAsync();
        if (fetchSlideShowFile is null)
            return new() { ErrorException = new() { ErrorCode = "D0", Message = "SlideShow is not availabe" } };

        var fetchFile = await (from i in db.TB_Files where i.FileCode == fetchSlideShowFile.FileCode select i).FirstOrDefaultAsync();
        if (fetchFile is null)
            return new() { ErrorException = new() { ErrorCode = "D0", Message = "File is not availabe" } };

        db.TB_Files.Remove(fetchFile);
        db.Entry(fetchFile).State = EntityState.Deleted;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true, ObjectID = fetchSlideShowFile.FileCode.ToString() };
    }

    public static async Task<PublicActionResponse> Update(PostUpdateRequest requestModel)
    {
        using DotnetableEntity db = new();

        if (await (from i in db.TB_Posts where i.Title == requestModel.Title && i.PostID != requestModel.PostID select i.PostCategoryID).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "C7" } };

        var fetchPost = await (from i in db.TB_Posts where i.PostID == requestModel.PostID select i).FirstOrDefaultAsync();
        if (fetchPost is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        fetchPost.Title = requestModel.Title;
        fetchPost.Summary = requestModel.Summary;
        fetchPost.Body = requestModel.Body;
        fetchPost.PostCategoryID = requestModel.PostCategoryID;
        fetchPost.Tags = requestModel.Tags;
        fetchPost.MetaDescription = requestModel.MetaDescription;
        fetchPost.MetaKeywords = requestModel.MetaKeywords;
        fetchPost.LogTime = DateTime.Now;
        fetchPost.MemberID = requestModel.CurrentMemberID.Value;
        fetchPost.FileCode = requestModel.MainImage;


        try
        {
            db.Entry(fetchPost).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = ex.Message } };
        }

        if (requestModel.FileCodes is not null && requestModel.FileCodes.Count > 0)
        {
            foreach (var j in requestModel.FileCodes)
            {
                Guid fileCode = new(j);
                var fetchDBFile = await (from i in db.TB_Files where i.FileCode == fileCode select i).FirstOrDefaultAsync();
                if (fetchDBFile is null) continue;
                if (await (from i in db.TBM_Post_Files where i.PostID == requestModel.PostID && i.FileID == fetchDBFile.FileID select i.PostFileID).AnyAsync()) continue;

                fetchDBFile.FileCategoryID = (byte)FileCategoryID.Post;
                fetchDBFile.FilePath = requestModel.PostID.ToString();
                db.Entry(fetchDBFile).State = EntityState.Modified;

                try
                {
                    db.TBM_Post_Files.Add(new() { FileID = fetchDBFile.FileID, PostID = requestModel.PostID, ShowGallery = false });
                    await db.SaveChangesAsync();
                }
                catch (Exception) { }
            }
        }

        return new() { SuccessAction = true };
    }

    public static async Task<PublicActionResponse> PostAddLanguage(PostUpdateRequest requestModel)
    {
        using DotnetableEntity db = new();
        if (!await (from i in db.TB_Posts where i.PostID == requestModel.PostID select i.PostID).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        var fetchPostLanguageg = await (from i in db.TB_Post_Languages where i.LanguageCode == requestModel.LanguageCode && i.PostID == requestModel.PostID select i).FirstOrDefaultAsync();

        if (fetchPostLanguageg is null)
        {
            var postLanguage = new TB_Post_Language()
            {
                PostID = requestModel.PostID,
                Body = requestModel.Body,
                Title = requestModel.Title,
                Summary = requestModel.Summary,
                LanguageCode = requestModel.LanguageCode,
                MetaDescription = requestModel.MetaDescription,
                MetaKeywords = requestModel.MetaKeywords,
                Tags = requestModel.Tags
            };
            db.TB_Post_Languages.Add(postLanguage);
        }
        else
        {
            fetchPostLanguageg.Body = requestModel.Body;
            fetchPostLanguageg.Title = requestModel.Title;
            fetchPostLanguageg.Summary = requestModel.Summary;
            fetchPostLanguageg.MetaDescription = requestModel.MetaDescription;
            fetchPostLanguageg.MetaKeywords = requestModel.MetaKeywords;
            fetchPostLanguageg.Tags = requestModel.Tags;

            db.Entry(fetchPostLanguageg).State = EntityState.Modified;
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = ex.Message } };
        }

        return new() { SuccessAction = true };
    }

    public static async Task<PostLanguageDetailResponse> PostLanguageDetail(PostLanguageDetailRequest requestModel)
    {
        using DotnetableEntity db = new();
        var responseObject = await (from i in db.TB_Post_Languages where i.PostID == requestModel.PostID && i.LanguageCode == requestModel.LanguageCode select new PostLanguageDetailResponse { LanguageCode = i.LanguageCode, MetaDescription = i.MetaDescription, MetaKeywords = i.MetaKeywords, PostID = i.PostID, PostLanguageID = i.PostLanguageID, Summary = i.Summary, Tags = i.Tags, Title = i.Title, Body = i.Body }).FirstOrDefaultAsync();
        if (responseObject is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        return responseObject;
    }

    public static async Task<PublicActionResponse> PostDeleteLangauge(PostLanguageDeleteRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchPostLanguage = await (from i in db.TB_Post_Languages where i.PostID == requestModel.PostID && i.LanguageCode == requestModel.LanguageCode select i).FirstOrDefaultAsync();
        if (fetchPostLanguage is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        db.TB_Post_Languages.Remove(fetchPostLanguage);

        try
        {
            db.Entry(fetchPostLanguage).State = EntityState.Deleted;
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }



    public static async Task<PublicActionResponse> ChangeStatus(PostChangeStatusRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchPost = await (from i in db.TB_Posts where i.PostID == requestModel.PostID select i).FirstOrDefaultAsync();
        if (fetchPost is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        fetchPost.Active = !fetchPost.Active;

        try
        {
            db.Entry(fetchPost).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = ex.Message } };
        }

        return new() { SuccessAction = true };
    }


    public static async Task<PublicActionResponse> PublicPostUpdate(PostPublicUpdateRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchContact = await (from i in db.TB_Posts where i.PostCode != null && i.PostCode == requestModel.PostCode select i).FirstOrDefaultAsync();
        if (fetchContact is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        if (fetchContact.LanguageCode == requestModel.PublicPostDetail.LanguageCode)
        {
            fetchContact.Body = requestModel.FinalPostBody;
            fetchContact.Title = requestModel.PublicPostDetail.Title;
            fetchContact.Summary = requestModel.PublicPostDetail.Summary;
            fetchContact.Tags = requestModel.PublicPostDetail.Tags;
            fetchContact.MetaDescription = requestModel.PublicPostDetail.MetaDescription;
            fetchContact.MetaKeywords = requestModel.PublicPostDetail.MetaKeywords;

            try
            {
                db.Entry(fetchContact).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new() { ErrorException = new() { ErrorCode = "D1", Message = ex.Message } };
            }
        }
        else
        {
            var fetchContactLanguage = await (from i in db.TB_Post_Languages where i.PostID == fetchContact.PostID && i.LanguageCode == requestModel.PublicPostDetail.LanguageCode select i).FirstOrDefaultAsync();
            if (fetchContactLanguage is not null)
            {
                fetchContactLanguage.Body = requestModel.FinalPostBody;
                fetchContactLanguage.Title = requestModel.PublicPostDetail.Title;
                fetchContactLanguage.Summary = requestModel.PublicPostDetail.Summary;
                fetchContactLanguage.Tags = requestModel.PublicPostDetail.Tags;
                fetchContactLanguage.MetaDescription = requestModel.PublicPostDetail.MetaDescription;
                fetchContactLanguage.MetaKeywords = requestModel.PublicPostDetail.MetaKeywords;
                db.Entry(fetchContactLanguage).State = EntityState.Modified;
            }
            else
            {
                db.TB_Post_Languages.Add(new()
                {
                    PostID = fetchContact.PostID,
                    LanguageCode = requestModel.PublicPostDetail.LanguageCode,
                    Body = requestModel.FinalPostBody,
                    MetaDescription = requestModel.PublicPostDetail.MetaDescription,
                    MetaKeywords = requestModel.PublicPostDetail.MetaKeywords,
                    Summary = requestModel.PublicPostDetail.Summary,
                    Tags = requestModel.PublicPostDetail.Tags,
                    Title = requestModel.PublicPostDetail.Title
                });
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new() { ErrorException = new() { ErrorCode = "D1", Message = ex.Message } };
            }
        }

        return new() { SuccessAction = true };
    }


    #endregion



    public static async Task<PostCategoryPublicDetailRsesponse> PublicPostCategoryDetail(PostCategoryPublicDetailRequest requestModel)
    {
        using DotnetableEntity db = new();
        List<PostCategoryPublicDetailRsesponse.PostCategoryDetail> responseItem = new()
        {
            await (from i in db.TB_Post_Categories where i.PostCategoryID == requestModel.PostCategoryID && i.Active select new PostCategoryPublicDetailRsesponse.PostCategoryDetail { PostCategoryID = i.PostCategoryID, MetaDescription = i.MetaDescription, Description = i.Description, MetaKeywords = i.MetaKeywords, Tags = i.Tags, Title = i.Title, LanguageCode = i.LanguageCode }).FirstOrDefaultAsync()
        };

        if (responseItem is null || responseItem.Count == 0)
            return new PostCategoryPublicDetailRsesponse() { ErrorException = new() { ErrorCode = "D0" } };

        responseItem.AddRange(await (from i in db.TB_Post_Category_Languages where i.PostCategoryID == requestModel.PostCategoryID select new PostCategoryPublicDetailRsesponse.PostCategoryDetail { PostCategoryID = i.PostCategoryID, MetaDescription = i.MetaDescription, Description = i.Description, MetaKeywords = i.MetaKeywords, Tags = i.Tags, Title = i.Title, LanguageCode = i.LanguageCode }).ToListAsync());

        return new() { PostCategories = responseItem };
    }

    public static async Task<PostListPublicResponse> PublicPostList(PostListPublicRequest requestModel)
    {
        using DotnetableEntity db = new();
        var posts = await (from pc in db.TB_Post_Categories
                           join p in db.TB_Posts on pc.PostCategoryID equals p.PostCategoryID
                           where pc.PostCategoryID == requestModel.PostCategoryID && pc.Active && p.Active
                           select new PostListPublicResponse.PostDetail { FileCode = p.FileCode, LanguageCode = p.LanguageCode, LogTime = p.LogTime, PostID = p.PostID, Summary = p.Summary, Title = p.Title })
                           .Concat(from pc in db.TB_Post_Categories
                                   join p in db.TB_Posts on pc.PostCategoryID equals p.PostCategoryID
                                   join pl in db.TB_Post_Languages on p.PostID equals pl.PostID
                                   where pc.PostCategoryID == requestModel.PostCategoryID && pc.Active && p.Active
                                   select new PostListPublicResponse.PostDetail { FileCode = p.FileCode, LanguageCode = pl.LanguageCode, LogTime = p.LogTime, PostID = p.PostID, Summary = pl.Summary, Title = pl.Title })
                           .Concat(from pc in db.TB_Post_Categories
                                   join pr in db.TBM_Post_Category_Relations on pc.PostCategoryID equals pr.PostCategoryID
                                   join p in db.TB_Posts on pr.PostCategoryID equals p.PostCategoryID
                                   where pc.PostCategoryID == requestModel.PostCategoryID && pc.Active && p.Active
                                   select new PostListPublicResponse.PostDetail { FileCode = p.FileCode, LanguageCode = p.LanguageCode, LogTime = p.LogTime, PostID = p.PostID, Summary = p.Summary, Title = p.Title })
                           .Concat(from pc in db.TB_Post_Categories
                                   join pr in db.TBM_Post_Category_Relations on pc.PostCategoryID equals pr.PostCategoryID
                                   join p in db.TB_Posts on pr.PostCategoryID equals p.PostCategoryID
                                   join pl in db.TB_Post_Languages on p.PostID equals pl.PostID
                                   where pc.PostCategoryID == requestModel.PostCategoryID && pc.Active && p.Active
                                   select new PostListPublicResponse.PostDetail { FileCode = p.FileCode, LanguageCode = pl.LanguageCode, LogTime = p.LogTime, PostID = p.PostID, Summary = pl.Summary, Title = pl.Title })
                           .OrderBy(i => i.PostID).ToListAsync();

        return new() { Posts = posts };
    }


    public static async Task<PostDetailPublicResponse> PublicPostDetailByID(int postID)
    {
        using DotnetableEntity db = new();

        var postDetails = await (from p in db.TB_Posts
                                 join pc in db.TB_Post_Categories on p.PostCategoryID equals pc.PostCategoryID
                                 join m in db.TB_Members on p.MemberID equals m.MemberID
                                 where pc.Active && p.Active && p.PostID == postID
                                 select new PostDetailPublicResponse.PostDetails { FileCode = p.FileCode, LanguageCode = p.LanguageCode, LogTime = p.LogTime, PostID = p.PostID, Summary = p.Summary, Title = p.Title, Body = p.Body, MemberGivenName = m.Givenname, MemberSurname = m.Surname, MetaDescription = p.MetaDescription, MetaKeywords = p.MetaKeywords, NormalBody = p.NormalBody, PostCategoryTitle = pc.Title, Tags = p.Tags })
                          .Concat(from p in db.TB_Posts
                                  join pc in db.TB_Post_Categories on p.PostCategoryID equals pc.PostCategoryID
                                  join m in db.TB_Members on p.MemberID equals m.MemberID
                                  join pl in db.TB_Post_Languages on p.PostID equals pl.PostID
                                  where pc.Active && p.Active && p.PostID == postID
                                  select new PostDetailPublicResponse.PostDetails { FileCode = p.FileCode, LanguageCode = pl.LanguageCode, LogTime = p.LogTime, PostID = p.PostID, Summary = pl.Summary, Title = pl.Title, Body = pl.Body, MemberGivenName = m.Givenname, MemberSurname = m.Surname, MetaDescription = pl.MetaDescription, MetaKeywords = pl.MetaKeywords, NormalBody = p.NormalBody, PostCategoryTitle = pc.Title, Tags = p.Tags })
                          .ToListAsync();

        return new() { PostsDetail = postDetails };
    }

    public static async Task<PostDetailPublicResponse> PublicPostDetailByCode(string postCode)
    {
        using DotnetableEntity db = new();
        var postDetails = await (from p in db.TB_Posts
                                 join pc in db.TB_Post_Categories on p.PostCategoryID equals pc.PostCategoryID
                                 join m in db.TB_Members on p.MemberID equals m.MemberID
                                 where pc.Active && p.Active && p.PostCode == postCode
                                 select new PostDetailPublicResponse.PostDetails { FileCode = p.FileCode, LanguageCode = p.LanguageCode, LogTime = p.LogTime, PostID = p.PostID, Summary = p.Summary, Title = p.Title, Body = p.Body, MemberGivenName = m.Givenname, MemberSurname = m.Surname, MetaDescription = p.MetaDescription, MetaKeywords = p.MetaKeywords, NormalBody = p.NormalBody, PostCategoryTitle = pc.Title, Tags = p.Tags })
                           .Concat(from p in db.TB_Posts
                                   join pc in db.TB_Post_Categories on p.PostCategoryID equals pc.PostCategoryID
                                   join m in db.TB_Members on p.MemberID equals m.MemberID
                                   join pl in db.TB_Post_Languages on p.PostID equals pl.PostID
                                   where pc.Active && p.Active && p.PostCode == postCode
                                   select new PostDetailPublicResponse.PostDetails { FileCode = p.FileCode, LanguageCode = pl.LanguageCode, LogTime = p.LogTime, PostID = p.PostID, Summary = pl.Summary, Title = pl.Title, Body = pl.Body, MemberGivenName = m.Givenname, MemberSurname = m.Surname, MetaDescription = pl.MetaDescription, MetaKeywords = pl.MetaKeywords, NormalBody = p.NormalBody, PostCategoryTitle = pc.Title, Tags = p.Tags })
                           .ToListAsync();

        return new() { PostsDetail = postDetails };
    }

    public static async Task<PublicActionResponse> PublicPostAddVisitCount(PostDetailPublicRequest requestModel)
    {
        using DotnetableEntity db = new();
        TB_Post fetchPost = await (from i in db.TB_Posts where (!requestModel.PostID.HasValue && i.PostCode == requestModel.PostCode) || i.PostID == requestModel.PostID.Value select i).FirstOrDefaultAsync();
        if (fetchPost is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        fetchPost.VisitCount++;
        db.Entry(fetchPost).State = EntityState.Modified;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }


    #region SlideShow

    public static async Task<PublicActionResponse> SlideShowInsert(SlideShowInsertRequest requestModel)
    {
        using DotnetableEntity db = new();
        if (await (from i in db.TB_SlideShows where i.Title == requestModel.Title select i.SlideShowID).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "D2" } };

        Guid fileCode = new(requestModel.FileCode);
        var fetchFile = await (from i in db.TB_Files where i.FileCode == fileCode select i).FirstOrDefaultAsync();
        if (fetchFile is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        string fetchLanguage = (await WebsiteDataAccess.FetchSettingByKey("DEFAULT_LANGUAGE_CODE")) ?? "EN";

        TB_SlideShow slideShowOBJ = new()
        {
            Active = true,
            FileCode = fileCode,
            PageCode = requestModel.PageCode.ToUpper(),
            LanguageCode = fetchLanguage,
            Title = requestModel.Title,
            SettingArray = requestModel.SettingsArray,
            Priority = requestModel.Priority
        };
        db.TB_SlideShows.Add(slideShowOBJ);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        try
        {
            fetchFile.FileCategoryID = (byte)FileCategoryID.SlideShow;
            fetchFile.FilePath = slideShowOBJ.SlideShowID.ToString();
            db.Entry(fetchFile).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true, ObjectID = slideShowOBJ.SlideShowID.ToString() };
    }

    public static async Task<PublicActionResponse> SlideShowInsertLanguage(SlideShowInsertLanguageRequest requestModel)
    {
        using DotnetableEntity db = new();
        if (!await (from i in db.TB_SlideShows where i.SlideShowID == requestModel.SlideShowID select i.SlideShowID).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        var fetchSlideShowLanguage = await (from i in db.TB_SlideShow_Languages where i.SlideShowID == requestModel.SlideShowID && i.LanguageCode == requestModel.LanguageCode select i).FirstOrDefaultAsync();
        if (fetchSlideShowLanguage is null)
        {
            db.TB_SlideShow_Languages.Add(new()
            {
                LanguageCode = requestModel.LanguageCode,
                SlideShowID = requestModel.SlideShowID,
                Title = requestModel.Title,
                SettingArray = requestModel.SettingsArray ?? ""
            });
        }
        else
        {
            if (!string.IsNullOrEmpty(requestModel.SettingsArray)) fetchSlideShowLanguage.SettingArray = requestModel.SettingsArray;
            if (!string.IsNullOrEmpty(requestModel.Title) && requestModel.Title != "") fetchSlideShowLanguage.Title = requestModel.Title;
            db.Entry(fetchSlideShowLanguage).State = EntityState.Modified;
        }

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }

    public static async Task<PublicActionResponse> SlideShowRemoveLanguage(SlideShowRemoveLanguageRequest requestModel)
    {
        using DotnetableEntity db = new();
        if (!await (from i in db.TB_SlideShows where i.SlideShowID == requestModel.SlideShowID select i.SlideShowID).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        var fetchSlideShowLanguage = await (from i in db.TB_SlideShow_Languages where i.SlideShowID == requestModel.SlideShowID && i.LanguageCode == requestModel.LanguageCode select i).FirstOrDefaultAsync();
        if (fetchSlideShowLanguage is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        db.TB_SlideShow_Languages.Remove(fetchSlideShowLanguage);

        try
        {
            db.Entry(fetchSlideShowLanguage).State = EntityState.Deleted;
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }

    public static async Task<SlideShowLanguageDetailResponse> SlideShowLanguageDetail(SlideShowLanguageDetailRequest requestModel)
    {
        using DotnetableEntity db = new();
        if (!await (from i in db.TB_SlideShows where i.SlideShowID == requestModel.SlideShowID select i.SlideShowID).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        var fetchSlideShowLanguage = await (from i in db.TB_SlideShow_Languages where i.SlideShowID == requestModel.SlideShowID && i.LanguageCode == requestModel.LanguageCode select i).FirstOrDefaultAsync();
        if (fetchSlideShowLanguage is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        return new() { LanguageCode = fetchSlideShowLanguage.LanguageCode, ErrorException = null, SettingsArray = fetchSlideShowLanguage.SettingArray, SlideShowID = fetchSlideShowLanguage.SlideShowID, SlideShowLanguageID = fetchSlideShowLanguage.SlideShowLanguageID, Title = fetchSlideShowLanguage.Title };
    }

    public static async Task<SlideShowLanguageListResponse> SlideShowLanguageList(SlideShowLanguageListRequest requestModel)
    {
        using DotnetableEntity db = new();
        if (!await (from i in db.TB_SlideShows where i.SlideShowID == requestModel.SlideShowID select i.SlideShowID).AnyAsync())
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        var fetchSlideShowLanguage = await (from i in db.TB_SlideShow_Languages where i.SlideShowID == requestModel.SlideShowID select new SlideShowLanguageListResponse.LanguagesDetail { LanguageCode = i.LanguageCode, SettingsArray = i.SettingArray, SlideShowLanguageID = i.SlideShowLanguageID, Title = i.Title }).ToListAsync();

        return new() { SlideShowLanguages = fetchSlideShowLanguage };
    }

    public static async Task<PublicActionResponse> SlideShowUpdate(SlideShowUpdateRequest requestModel)
    {
        using DotnetableEntity db = new();

        var fetchSlideShow = await (from i in db.TB_SlideShows where i.SlideShowID == requestModel.SlideShowID select i).FirstOrDefaultAsync();

        if (fetchSlideShow is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        if (!string.IsNullOrEmpty(requestModel.Title) && requestModel.Title != "") fetchSlideShow.Title = requestModel.Title;
        if (!string.IsNullOrEmpty(requestModel.SettingsArray)) fetchSlideShow.SettingArray = requestModel.SettingsArray;
        if (!string.IsNullOrEmpty(requestModel.PageCode) && requestModel.PageCode != "") fetchSlideShow.PageCode = requestModel.PageCode.ToUpper();
        if (requestModel.Priority is not null) fetchSlideShow.Priority = requestModel.Priority.Value;

        string responseObjectID = "";
        if (!string.IsNullOrEmpty(requestModel.FileCode) && requestModel.FileCode != "" && new Guid(requestModel.FileCode) != fetchSlideShow.FileCode)
        {
            Guid fileCode = new(requestModel.FileCode);
            var fetchFile = await (from i in db.TB_Files where i.FileCode == fileCode select i).FirstOrDefaultAsync();
            if (fetchFile is null)
                return new() { ErrorException = new() { ErrorCode = "D0" } };

            fetchFile.FileCategoryID = (byte)FileCategoryID.SlideShow;
            fetchFile.FilePath = requestModel.SlideShowID.ToString();
            db.Entry(fetchFile).State = EntityState.Modified;

            responseObjectID = fetchSlideShow.FileCode.ToString();
            fetchSlideShow.FileCode = new(requestModel.FileCode);
        }

        try
        {
            db.Entry(fetchSlideShow).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true, ObjectID = responseObjectID };
    }

    public static async Task<PublicActionResponse> SlideShowChangeStatus(SlideShowChangeStatusRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchSlideShow = await (from i in db.TB_SlideShows where i.SlideShowID == requestModel.SlideShowID select i).FirstOrDefaultAsync();
        if (fetchSlideShow is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        fetchSlideShow.Active = !fetchSlideShow.Active;

        try
        {
            db.Entry(fetchSlideShow).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }

    public static async Task<SlideShowDetailResponse> SlideShowDetail(SlideShowDetailRequest requestModel)
    {
        using DotnetableEntity db = new();
        var fetchSlideShow = await (from i in db.TB_SlideShows where i.SlideShowID == requestModel.SlideShowID select new SlideShowDetailResponse { Active = i.Active, Priority = i.Priority, ErrorException = null, LanguageCode = i.LanguageCode, PageCode = i.PageCode, SettingsArray = i.SettingArray, SlideShowID = i.SlideShowID, Title = i.Title, FileCode = i.FileCode }).FirstOrDefaultAsync();
        if (fetchSlideShow is null)
            return new() { ErrorException = new() { ErrorCode = "D0" } };

        return fetchSlideShow;
    }

    public static async Task<SlideShowListResponse> SlideShowList(SlideShowListRequest requestModel)
    {
        using DotnetableEntity db = new();
        var reportQuery = db.TB_SlideShows.AsQueryable();

        if (!string.IsNullOrEmpty(requestModel.Title) && requestModel.Title != "")
            reportQuery = reportQuery.Where(i => i.Title.Contains(requestModel.Title));

        if (!string.IsNullOrEmpty(requestModel.PageCode) && requestModel.PageCode != "")
            reportQuery = reportQuery.Where(i => i.PageCode.Contains(requestModel.PageCode));


        int dbCount = await reportQuery.CountAsync();

        if (string.IsNullOrEmpty(requestModel.OrderbyParams) || requestModel.OrderbyParams == "")
            reportQuery = reportQuery.OrderBy(i => i.SlideShowID);
        else
            reportQuery = reportQuery.OrderUsingSortExpression(requestModel.OrderbyParams);


        var finalQuery = (from s in reportQuery
                          let languageCodes = db.TB_SlideShow_Languages.DefaultIfEmpty().Where(i => i.SlideShowID == s.SlideShowID).Select(i => i.LanguageCode).ToList()
                          select new { s.SlideShowID, s.Title, s.PageCode, s.Active, s.LanguageCode, s.Priority, s.FileCode, LanguageCodes = languageCodes });

        var fetchList = await finalQuery.Skip(requestModel.SkipCount).Take(requestModel.TakeCount).Select(i => new SlideShowListResponse.SlideShowDetail { Active = i.Active, SlideShowID = i.SlideShowID, LanguageCodes = string.Join(", ", i.LanguageCodes), Title = i.Title, FileCode = i.FileCode, LanguageCode = i.LanguageCode, PageCode = i.PageCode, Priority = i.Priority }).ToListAsync();

        return new() { SlideShows = fetchList, DatabaseRecords = dbCount };
    }

    public static async Task<SlideShowWebsiteShowResponse> SlideShowSite(SlideShowWebsiteShowRequest requestModel)
    {
        using DotnetableEntity db = new();

        var slides = await (from s in db.TB_SlideShows where s.Active && s.PageCode == requestModel.PageCode select new SlideShowWebsiteShowResponse.SlideShowDetail { FileCode = s.FileCode, LanguageCode = s.LanguageCode, Priority = s.Priority, SettingArray = s.SettingArray, SlideShowID = s.SlideShowID, Title = s.Title })
            .Concat(from s in db.TB_SlideShows join sl in db.TB_SlideShow_Languages on s.SlideShowID equals sl.SlideShowID where s.Active && s.PageCode == requestModel.PageCode select new SlideShowWebsiteShowResponse.SlideShowDetail { FileCode = s.FileCode, LanguageCode = sl.LanguageCode, Priority = s.Priority, SettingArray = sl.SettingArray, SlideShowID = s.SlideShowID, Title = sl.Title }).ToListAsync();

        return new() { SlideShows = slides };
    }


    public static async Task<PublicActionResponse> RemovePostFile(PostFileRemoveRequest requestModel)
    {
        using DotnetableEntity db = new();
        Guid fileCode = new(requestModel.FileCode);
        var fetchFile = await (from i in db.TB_Files where i.FileCode == fileCode select i).FirstOrDefaultAsync();
        if (fetchFile is null)
            return new() { ErrorException = new() { ErrorCode = "D0", Message = "File is not availabe" } };

        var fetchPostFile = await (from i in db.TBM_Post_Files where i.PostID == requestModel.PostID && i.FileID == fetchFile.FileID select i).FirstOrDefaultAsync();
        if (fetchPostFile is null)
            return new() { ErrorException = new() { ErrorCode = "D0", Message = "Post file is not available" } };

        db.TBM_Post_Files.Remove(fetchPostFile);
        db.Entry(fetchPostFile).State = EntityState.Deleted;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        db.TB_Files.Remove(fetchFile);
        db.Entry(fetchFile).State = EntityState.Deleted;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (Exception x)
        {
            return new() { ErrorException = new() { ErrorCode = "D1", Message = x.Message } };
        }

        return new() { SuccessAction = true };
    }


    #endregion




}