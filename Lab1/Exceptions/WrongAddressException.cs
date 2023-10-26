namespace Lab1.Exceptions;

public class WrongAddressException : Exception
{
    public WrongAddressException() {}
    
    public WrongAddressException(int index) : base($"Строка {index}: несоответствующий тип адрессации") {}
}