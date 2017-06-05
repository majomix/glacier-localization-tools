
namespace GlacierTextConverter
{
    public class FileVersionSpecificationsBase : IFileVersionSpecifications
    {
        public ICypherStrategy CypherStrategy { get; private set; }
        public int NumberOfLanguages { get; private set; }

        public FileVersionSpecificationsBase()
        {
            CypherStrategy = new CypherStrategyPermutation();
            NumberOfLanguages = 10;
        }
    }
}
