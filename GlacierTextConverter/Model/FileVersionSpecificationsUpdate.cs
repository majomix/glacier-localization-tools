
namespace GlacierTextConverter.Model
{
    public class FileVersionSpecificationsUpdate : IFileVersionSpecifications
    {
        public ICypherStrategy CypherStrategy { get; private set; }
        public int NumberOfLanguages { get; private set; }

        public FileVersionSpecificationsUpdate()
        {
            CypherStrategy = new CypherStrategyXXTEA();
            NumberOfLanguages = 12;
        }
    }
}
