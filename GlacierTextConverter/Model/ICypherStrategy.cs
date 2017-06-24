using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlacierTextConverter.Model
{
    public interface ICypherStrategy
    {
        String Decypher(byte[] input);
    }
}
