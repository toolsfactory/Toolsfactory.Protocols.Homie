using System.Text.RegularExpressions;

namespace Toolsfactory.Protocols.Homie
{
    public static class IDVerifier
    {
        private static readonly Regex _regex = new Regex("(^(?!\\-)[a-z0-9\\-]+(?<!\\-)$)");

        public static bool VerifyHomieID(string id)
        {
            return _regex.IsMatch(id);
        }
    }
}
