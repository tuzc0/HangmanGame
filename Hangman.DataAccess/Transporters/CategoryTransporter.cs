namespace Hangman.DataAccess.Transporters
{
    public class CategoryTransporter
    {
        public int CategoryId { get; set; }

        public string Name { get; set; }

        public string LanguageCode { get; set; }

        public bool IsActive { get; set; }
    }
}
