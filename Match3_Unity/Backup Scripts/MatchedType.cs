public enum MatchedType
{
    EmptySpace = -1,
    None = 0,
    Row_Three = 1,
    Row_Four = 2,
    Row_Five = 4,

    Column_Three = 8,
    Column_Four = 16,
    Column_Five = 32,

    Cross_Three_Three = Row_Three + Column_Three,
    Cross_Three_Four = Row_Three + Column_Four,
    Cross_Three_Five = Row_Three + Column_Five,

    Cross_Four_Three = Row_Four + Column_Three,
    Cross_Four_Four = Row_Four + Column_Four,
    Cross_Four_Five = Row_Four + Column_Five,

    Cross_Five_Three = Row_Five + Column_Three,
    Cross_Five_Four = Row_Five + Column_Four,
    Cross_Five_Five = Row_Five + Column_Five,
};

public enum SwapType
{
    Fail,
    Success,
    Using
}