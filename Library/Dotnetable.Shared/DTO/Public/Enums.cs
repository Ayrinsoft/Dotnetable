namespace Dotnetable.Shared.DTO.Public;

public enum GridViewSortStatus
{
    NoSort = 0,
    ASC,
    DESC
}

public enum SearchColumnType
{
    Text,
    CheckBox,
    Date,
    DropDown
}

public enum CommentType
{
    Normal = 1,
    Edit,
    Wrong,
    BadWords
}

public enum CommentCategory
{
    Post = 1
}

public enum EmailType
{
    INFO = 1,
    CONTACT,
    MANAGER,
    HR,
    PUBLIC,
    NOREPLY
}

public enum FileCategoryID
{
    Temporary = 0,
    Member_Gallery,
    Post,
    Post_Category,
    Brand,
    SlideShow
}

public enum RequestContentType
{
    None = 0,
    Json,
    XML,
    UrlEncode
}

public enum MemberRole
{
    MemberManager,
    Settings,
    PostCategoryManager,
    PostManager,
    FileManager,
    PolicyManager,
    MessageManager,
    CommentManager,
    ProductManager
}

public enum DatabaseType
{
    MSSQL = 1,
    POSTGRESQL,
    MYSQL,
    MARIADB,
    //IBMDB2,
    //ORACLE,
    //SQLLITE
}