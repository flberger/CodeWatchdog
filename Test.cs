using System;

public class Test
{
    int i = 2;
	int j = 3;

    public void TestMethod()
    {
        if (i == j)
        {
            Console.WriteLine("42");
        }
    }

    /// <summary>
    /// Test
    /// </summary>
    public void AnotherTest()
//  {
    {
        // Console.Writeline("Commented");

        i += 1; // Comment on statement line

        return;
    }
}
