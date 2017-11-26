using System;
using System.Security.Cryptography;
using System.Text;

namespace FG.Common.Utils
{
	public class MiniId : IEquatable<string>, IEquatable<MiniId>
	{
		private static readonly SHA1 Hasher;

		static MiniId()
		{
			Hasher = System.Security.Cryptography.SHA1.Create();
		}

		public MiniId()
		{
			var guid = Guid.NewGuid().ToByteArray();
			var hashBytes = Hasher.ComputeHash(guid);
			Id = System.Convert.ToBase64String(hashBytes).Substring(0, 6);
		}

		public string Id { get; private set; }

		public bool Equals(MiniId other)
		{
			if (other == null) return Id == null;
			return Id.Equals(other.Id, StringComparison.InvariantCultureIgnoreCase);
		}

		public bool Equals(string other)
		{
			if (other == null) return false;
			return ((other.Length == 6) && other.Equals(Id, StringComparison.InvariantCultureIgnoreCase));
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((MiniId) obj);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		public static implicit operator string(MiniId id)
		{
			return id.Id;
		}

		public override string ToString()
		{
			return Id;
		}
	}
}