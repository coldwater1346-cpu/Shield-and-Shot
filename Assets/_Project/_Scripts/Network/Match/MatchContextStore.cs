namespace Shield_Shot.GameplayCore.Network.Match
{
    public static class MatchContextStore
    {
        public static MatchContext Current { get; private set; }

        public static void Set(MatchContext matchContext)
        {
            Current = matchContext;
        }

        public static bool TryGet(out MatchContext matchContext)
        {
            matchContext = Current;
            return matchContext != null;
        }

        public static void Clear()
        {
            Current = null;
        }
    }
}