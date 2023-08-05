namespace ASP_111.Models.Forum.Theme
{
    public class CommenrFromModel
    {
            
        public String Id { get; set; } = null!;
        public String Content { get; set; } = null!;
        public String CreateDt { get; set; } = null!;

        public CommenrFromModel()
        {
        }

        public CommenrFromModel(Data.Entities.Comment comment)
        {
            Id = comment.Id.ToString();
            Content = comment.Content;
            CreateDt = comment.CreateDt.ToShortDateString();
        }
    }
}