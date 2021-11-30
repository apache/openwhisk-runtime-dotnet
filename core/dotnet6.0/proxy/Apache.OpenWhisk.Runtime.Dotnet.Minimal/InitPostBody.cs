namespace Apache.Openwhisk.Runtime.Minimal
{
    /// <summary>
    /// For more information, see https://github.com/apache/openwhisk/blob/master/docs/actions-new.md#initialization
    /// </summary>
    public class InitPostBody
    {
        public Value value { get; set; }
    }

    public class Value
    {
        public string name { get; set; }
        public string main { get; set; }
        public string code { get; set; }
        public bool binary { get; set; }
        public Env env { get; set; }
    }

    public class Env
    {
        public string __OW_API_KEY { get; set; }
        public string __OW_NAMESPACE { get; set; }
        public string __OW_ACTION_NAME { get; set; }
        public string __OW_ACTION_VERSION { get; set; }
        public string __OW_ACTIVATION_ID { get; set; }
        public long __OW_DEADLINE { get; set; }
    }

}
