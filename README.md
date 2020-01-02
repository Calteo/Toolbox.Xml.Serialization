# Toolbox.Xml.Serialization
A simple framework to serialize objects to xml.

Currrent status: [![Build status](https://ci.appveyor.com/api/projects/status/58yntgqha1wp71vg?svg=true)](https://ci.appveyor.com/project/Calteo/toolbox-xml-serialization)

Often we need to serialize data object to xml and various techniques can be used. This is a simpe class to handle the most used cases. 
The basic idea is the serialize the maximum possible and do not bother about the the thinds that can not be serialized. They are simple obmitted. Also no declaration (i.e. `Serializable` attribute) is neccessary to include an object.

* Serializes all properties with getter and setter, if
  * it is a simple type witch can be converted to string. So all basic types of `int`, `bool`, `DateTime`, `string`, `float`,`decimal` and ... are coverved.
  * or the object implements a default constructor
  
If a property type implements `ICollection<T>` its contents get also serialized. So the generic collections like `List<T>` also get serialized.
  
Properties can be marked with the attribute `NotSerialized` so they get not included.

## Sample object
```
class DataContainer
{   
  public string Name { get; set; }
  public int ZipCode { get; set; }  
  [NotSerialized] 
  public string Passcode { get; set; }
  
  public FunkyData OtherData1 { get; private set; }
  public FunkyData OtherData2 { get;  set; }
}
```
Here the properties `Name` and `ZipCode` get serialized. `Passcode` is obmitted due to the `NotSerialized` attribute. While `OtherData1` and `OtherData1` will be included if `FunkyData` has a default constructor. For the content of `FunkyData` the same rules apply.
    
## usage

Simply create a instance of `XmlFormatter<T>` and use the methods.
```
void Serialize(T obj, string fileName)
```
to serialize and
```
T Deserialize(string fileName)
```
to deserialize.

## Disclaimer
Check the created files to see if all content is include or read. There maybe some flaws that prevent things from working right.
