using System;

namespace Hangman.DataAccess.Transporters
{
    public class WordTransporter
    {
        public int WordId { get; set; }

        public int CategoryId { get; set; }

        public string CategoryKey { get; set; }

        public string CategoryName { get; set; }

        public string WordText { get; set; }

        public string Description { get; set; }

        public string LanguageCode { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
