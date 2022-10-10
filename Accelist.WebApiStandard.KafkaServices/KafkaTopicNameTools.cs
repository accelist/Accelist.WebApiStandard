namespace Accelist.WebApiStandard.KafkaServices
{
    public static class KafkaTopicNameTools
    {
        public static string GetTopicNameFromType<T>()
        {
            return typeof(T).FullName ?? $"[{typeof(T).Assembly.FullName ?? "Assembly?"}][{typeof(T).Name}]";
        }
    }
}
