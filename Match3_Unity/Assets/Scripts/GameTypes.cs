public enum MatchedType
{
    Blank = -1,
    None,
    Crash,
    MakeFour,
    MakeFive,
};

public enum IconType
{
    Common,
    Four,
    Five
}

public enum SwapResult
{
    NotAdjacent,
    Success,
    Using
}

public enum SearchType
{
    None = 0,
    Column = 1,
    Row = 2,
    Both = Column + Row
}