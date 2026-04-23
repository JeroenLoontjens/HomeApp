namespace DataAccess.Model
{
    public enum RabobankImportStatus
    {
        Pending,    // Imported, waiting for user to process
        Processed,  // Converted to a real Transaction
        Skipped,    // User deliberately skipped this row
        Duplicate   // Volgnr already existed → auto-marked
    }
}
