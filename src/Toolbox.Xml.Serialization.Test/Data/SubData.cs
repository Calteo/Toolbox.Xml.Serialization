namespace Toolbox.Xml.Serialisation.Test.Data
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
