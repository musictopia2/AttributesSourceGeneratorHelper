namespace AttributesSourceGeneratorHelper;
public class SampleExtras
{
    public AttributeProperty GetFirstValueInfo => new("FirstValue", 0);
    public AttributeProperty GetSecondValueInfo => new("SecondValue", -1);
    public AttributeProperty GetThirdValueInfo => new("ThirdValue", 1);




    //i propose first looping through required ones.
    //so the proper numbers can be put in there.

    //i propose a new namespace for property helpers.
    //also, a global namespace as well so its easy to access when needed.

    //i also propose that if there are no properties, then skip it.



    //[Required]
    //public int FirstValue { get; set; }
    //public int SecondValue { get; set; }
    //[Required]
    //public int ThirdValue { get; set; }
}
