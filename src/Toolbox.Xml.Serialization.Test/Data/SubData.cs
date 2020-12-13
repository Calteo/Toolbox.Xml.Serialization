namespace Toolbox.Xml.Serialization.Test.Data
{
    class SubData
    {
        public SubData()
        {
            Info = $"Created at {GetHashCode()}";
        }

        public string Info { get; set; }
    }
}
