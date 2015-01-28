using System;
using System.Collections.Generic;

public class Test
{
    int i = 2;

    // Tab-indented line:
	int j = 3;
    
    string CorrectPascalCase = "default";
    public List<string> incorrectCamelCase;
    protected Dictionary<int, string> Disallowed_Character;
        
    public void TestMethod()
    {
        if (i == j)
        {
            Console.WriteLine("42");
            
            Console.WriteLine("Multiple statement on single line"); Console.WriteLine("Multiple statement on single line");
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
        j += 1; // Another comment on statement line
        
        if (1 != 0)
            Console.WriteLine("Woops, forgot curly braces.");
        
        while (false)
            Console.WriteLine("Woops, forgot curly braces.");
            
        i += 1;    Console.WriteLine("Another attempt "
                                     + "at multiple statements");
        
        //I might have forgotten a space here.
        
        return;
    }
}
