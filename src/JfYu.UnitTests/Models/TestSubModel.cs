using System.Diagnostics.CodeAnalysis;

namespace JfYu.UnitTests.Models
{
    public sealed class TestSubModel : IEquatable<TestSubModel>
    {
        public int Id { get; set; }
        public string CardNum { get; set; } = "";
        public DateTime ExpiresIn { get; set; }
              
        public static bool operator ==(TestSubModel x, TestSubModel y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.Equals(y);
        }

        public static bool operator !=(TestSubModel x, TestSubModel y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (string.IsNullOrEmpty(CardNum)? 0 : CardNum.GetHashCode());
                hash = hash * 23 + ExpiresIn.GetHashCode();
                return hash;
            }
        }
        public override bool Equals(object? obj)
        {
            if (obj is TestSubModel other)
            {
                return CardNum == other.CardNum && ExpiresIn.ToUniversalTime() == other.ExpiresIn.ToUniversalTime();
            }
            return false;
        }

        public bool Equals(TestSubModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return CardNum == other.CardNum && ExpiresIn.ToUniversalTime() == other.ExpiresIn.ToUniversalTime();

        }
    }
}