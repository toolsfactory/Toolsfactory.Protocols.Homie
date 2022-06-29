using System;

namespace Toolsfactory.Protocols.Homie
{
    public static class Constants
    {
        public const string ProtocolVersion = "4.0.0";
        public const string Implementation = "Toolsfactory.Protocols.Homie for .Net";
        public const string SetValueTopic = "set";
        public const string BroadcastTopic = "$broadcast";
        public const char LevelSeparator = '/';
        public const char MultiLevelWildcard = '#';
        public const char SingleLevelWildcard = '+';

        public static string TopicRoot { get { return _topicRoot; } set { _topicRoot = String.IsNullOrWhiteSpace(value) ? "homie" : value; } }
        private static string _topicRoot = "homie";

    }
}
