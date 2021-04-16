namespace GlacierTextConverter.Model
{
    public interface ICypherStrategy
    {
        string Decypher(byte[] input);
        byte[] Cypher(string input);
    }
}
