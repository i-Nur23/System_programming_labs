namespace Lab1;

public static class Converters
{
    public static string ToTwoDigits(string initStr)
    {
        var strWithZeros = "0" + initStr;
        return strWithZeros.Substring(strWithZeros.Length - 2);
    }
    
    public static string ToSixDigits(string initStr)
    {
        var strWithZeros = "00000" + initStr;
        return strWithZeros.Substring(strWithZeros.Length - 6);
    }
}