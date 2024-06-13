namespace StreamApi.Extensions;

public static class BiteArrayExtensions
{
    public static IEnumerable<byte[]> Split(this byte[] value, int maxLength = 4000)
    {
        if (value.Length <= maxLength)
            return new List<byte[]>() { value };
        
        var list = new List<byte[]>();

        var index = 0;
        var countIndex = 0;

        var subArray = new byte[maxLength];
        int arrayLength;

        foreach (var b in value)
        {
            subArray[index] = b;

            index++;
            countIndex++;

            if ((countIndex > 0 && countIndex % maxLength == 0) || countIndex == value.Length)
            {
                list.Add(subArray);
                arrayLength = (index + maxLength) > value.Length ? (value.Length - index) : maxLength;
                if (arrayLength > 0)
                {
                    subArray = new byte[arrayLength];
                    index = 0;
                }
            }
        }

        return list;
    }
}