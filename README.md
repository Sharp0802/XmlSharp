# XmlSharp
lib for easy serialization to xml in C#/.Net Standard 2.1



## How to use?

<pre>
<code>
class Test

{

  [XmlSerialization]
  
  private string exampleString {get; set;}
  
  /* must be property. Ignore access modifiers. */
  
}


Test obj = new Test();



obj.ToXmlFrom(); //obj to xml string

obj.ToXmlFrom().Serialize("../path"); //write xml string of obj in rom



obj = "path".Deserialize().FromXmlTo() as Test; //read xml and convert to object
</code>
</pre>
