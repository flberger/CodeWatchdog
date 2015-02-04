using System;
using System.Collections.Generic;

public class Test
{
    int i = 2;

    // Tab-indented line:
	int j = 3;
    
    string IdentifierInPascalCase = "default";
    public List<string> identifierInCamelCase;
    protected Dictionary<int, string> Disallowed_Character;
    
    const int anotherCamelCase = 42;
    const string ThisTimeInPascal = "The answer";
        
    public void TestMethod()
    {
        if (i == j)
        {
            Console.WriteLine("42");
            
            Console.WriteLine("Multiple statement on single line"); Console.WriteLine("Multiple statement on single line");
        }
    }

    /// <summary>
    /// Properly documented method
    /// </summary>
    public void anotherTest ()
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

// This does not suffice as a class documentation
public class NotWellDocumented: Object
{
    public static int k = 0;
}

/// <summary>
/// Now this is really nice.
/// </summary>
public class properlyDocumented : Object
{
    string thisClassHasBeen = "properly documented.";
    
    public string getterProperty
    {
        get;
        private set;
    }
    
    public string SetterProperty
    {
        private get;
        set;
    }
    
    // Neither is this a proper method documentation.
    public int Main()
    {
        return 12345;
    }
    
    string ReturnString()
    {
        return "test";
    }
}

public enum firstEnumeration
{
    foo,
    bar
}

enum SecondEnumeration
{
    value1,
    value2
}

interface ITestThingy
{
    void Pass();
}

interface HardlyRecognisable
{
    void Noop();
}
