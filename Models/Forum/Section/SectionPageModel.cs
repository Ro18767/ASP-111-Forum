
using ASP_111.Models.Forum.Index;

namespace ASP_111.Models.Forum.Section
{
    public class SectionPageModel
    {
        public string SectionId { get; set; } = null!;
        public ForumSectionViewModel Section { get; set; } = null!;
        public Dictionary<string, string?>? ErrorMessages { get; set; }

        public TopicFormModel? FormModel { get; set; }

        public List<TopicViewModel> Topics { get; set; } = null!;
    }
}
