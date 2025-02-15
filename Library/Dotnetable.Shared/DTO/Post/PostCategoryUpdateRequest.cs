using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotnetable.Shared.DTO.Post;

public class PostCategoryUpdateRequest
{
    public int PostCategoryID { get; set; }
    public bool MenuView { get; set; }
    public string Title { get; set; }
    public string Tags { get; set; }
    public string MetaKeywords { get; set; }
    public string MetaDescription { get; set; }
    public bool FooterView { get; set; }
    public string Description { get; set; }
}
