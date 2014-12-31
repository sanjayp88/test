namespace PayrollOneService
{
    public class Connection
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string Uid { get; set; }
        public string Pwd { get; set; }
        public API.Company Company { get; set; }

        public string PR1ConnectionString
        {
            get
            {
                return string.Format(@"{0}.{1}.{2}.$clear${3}.SQL",
                    Server,
                    Database,
                    Uid,
                    Pwd);
            }
        }

        public string SqlConnectionString
        {
            get
            {
                return string.Format(@"server={0};database={1};uid={2};pwd={3}",
                   Server,
                   Database,
                   Uid,
                   Pwd);
            }
        }
    }
}
