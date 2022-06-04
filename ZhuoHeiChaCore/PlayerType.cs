namespace ZhuoHeiChaCore
{
    public enum PlayerType
    {
        Normal,
        Ace,
        PublicAce
    }

    public static class PlayerTypeExtension
    {
        /// <summary>
        /// Determines if the two PlayerType objects represent the same party (BlackAce vs Normal)
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static bool IsOfSameParty(this PlayerType p1, PlayerType p2)
        {
            return (p1 == PlayerType.Normal && p2 == PlayerType.Normal) 
                || (p1 != PlayerType.Normal && p2 != PlayerType.Normal);
        }
    }
}
