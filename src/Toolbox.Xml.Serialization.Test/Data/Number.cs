using System.Numerics;

namespace Toolbox.Xml.Serialization.Test.Data
{
	internal class Number<T> where T : IUnsignedNumber<T>, IComparisonOperators<T, T, bool>
	{
		public T Zero { get; set; } = T.Zero;
		public T One { get; set; } = T.One;

		public Dictionary<T, T> Values { get; set; } = [];
	}
}
