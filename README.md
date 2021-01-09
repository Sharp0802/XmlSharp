# XmlSharp
easy serialization to xml in C#




How to use?



class Test

{

  [XmlSerialization]
  
  private string exampleString {get; set;}
  
  /* must be property.
  
  Ignore access modifiers.
  
  The value of the property being serialized is object.ToString() */
  
}


Test obj = new Test();



obj.ToXmlFrom(); //obj to xml string

obj.ToXmlFrom().Serialize("../path"); //write xml string of obj in rom



obj = "path".Deserialize().FromXmlTo() as Test; //read xml and convert to object
