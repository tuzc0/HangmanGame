namespace Hangman.DataAccess.Transporters
{
    public class UpdatePlayerActiveStatusTransporter
    {
        public int PlayerId { get; set; }

        public bool IsActive { get; set; }
    }
}
