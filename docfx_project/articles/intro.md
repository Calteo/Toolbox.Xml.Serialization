# Introduction

The basic idea is the serialize the maximum possible and do not bother 
about the the things that can not be serialized. They are simple obmitted. 
Also no declaration (i.e. `Serializable` attribute) is neccessary to include an object in the process.
It is simply assumed that you want to serialize all of the referenced objects.

* Serializes all properties with getter and setter, if
  * it is a simple type witch can be converted to string. 
    So all basic types of `int`, `bool`, `DateTime`, `string`, `float`,`decimal` and ... are coverved.
  * or the object implements a default constructor.
    Otherwise it would hard to construct these objects on deserialization.
  
If a property type implements `ICollection<T>` its contents get also serialized. 
So the generic collections like `List<T>` also get serialized.
  
Properties can be marked with the attribute <xref:Toolbox.Xml.Serialization.NotSerializedAttribute> 
so they get not included.


