
public static class Utility
{

    public static int RoundUp(float num)
    {
        int temp = (int)num;

        if (num == temp)
            return temp;
        else
            return temp + 1;
    }
}