namespace Visus.Cuid.Tests;

using System.Text.Json;
using AwesomeAssertions;

internal sealed class Cuid2Tests
{
    // CUID v2 Length Constants
    private const int DefaultCuid2Length = 24;

    private const int HashDistributionThreshold = 950;
    private const int HighConcurrencyIterations = 10000;
    private const int MediumConcurrencyIterations = 1000;

    // Test Iteration Constants
    private const int StandardTestIterations = 100;

    [Test]
    [Property("Category", "Construction")]
    [Arguments(3)]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(33)]
    [Arguments(100)]
    [Arguments(-100)]
    public void Constructor_WithInvalidLength_ShouldThrowArgumentOutOfRangeException(int length)
    {
        Action act = () => _ = new Cuid2(length);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    [Property("Category", "Construction")]
    [Arguments(4)]
    [Arguments(8)]
    [Arguments(12)]
    [Arguments(16)]
    [Arguments(20)]
    [Arguments(24)]
    [Arguments(28)]
    [Arguments(32)]
    public void Constructor_WithValidLength_ShouldCreateCuidOfCorrectLength(int length)
    {
        Cuid2 cuid = new(length);
        string result = cuid.ToString();

        result.Should().HaveLength(length);
        char.IsLower(result[0]).Should().BeTrue();
        char.IsLetter(result[0]).Should().BeTrue();
    }

    [Test]
    [Property("Category", "Equality")]
    public void EqualityOperators_ShouldWorkCorrectly()
    {
        Cuid2 cuid1 = new();
        Cuid2 cuid2 = cuid1;
        Cuid2 cuid3 = new();

        // Same CUID should be equal
        ( cuid1 == cuid2 ).Should().BeTrue();
        ( cuid1 != cuid2 ).Should().BeFalse();
        cuid1.Equals(cuid2).Should().BeTrue();

        // Different CUIDs should not be equal
        ( cuid1 == cuid3 ).Should().BeFalse();
        ( cuid1 != cuid3 ).Should().BeTrue();
        cuid1.Equals(cuid3).Should().BeFalse();
    }

    [Test]
    [Property("Category", "Equality")]
    public void Equals_WithEmpty_ShouldReturnTrueForEmpty()
    {
        Cuid2 empty1 = default;
        Cuid2 empty2 = default;

        empty1.Equals(empty2).Should().BeTrue();
        ( empty1 == empty2 ).Should().BeTrue();
    }

    [Test]
    [Property("Category", "Equality")]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        Cuid2 cuid = new();

        cuid.Equals(null).Should().BeFalse();
    }

    [Test]
    [Property("Category", "Equality")]
    public void Equals_WithObject_ShouldWork()
    {
        Cuid2 cuid1 = new();
        object cuid2 = cuid1;

        cuid1.Equals(cuid2).Should().BeTrue();
    }

    [Test]
    [Property("Category", "Equality")]
    public void Equals_WithWrongType_ShouldReturnFalse()
    {
        Cuid2 cuid = new();
        object other = "string";

        cuid.Equals(other).Should().BeFalse();
    }

    [Test]
    [Property("Category", "HashCode")]
    public void GetHashCode_ConsistencyAndUniqueness()
    {
        Cuid2 cuid1 = new();
        Cuid2 cuid2 = new();

        // Same CUID should have consistent hash
        cuid1.GetHashCode().Should().Be(cuid1.GetHashCode());

        // Default CUID should have consistent hash
        default(Cuid2).GetHashCode().Should().Be(default(Cuid2).GetHashCode());

        // Different CUIDs should have different hashes
        cuid1.GetHashCode().Should().NotBe(cuid2.GetHashCode());
    }

    [Test]
    [Property("Category", "HashCode")]
    public void GetHashCode_ShouldDistributeWell()
    {
        HashSet<int> hashes = [];

        for ( int i = 0; i < MediumConcurrencyIterations; i++ )
        {
            hashes.Add(new Cuid2().GetHashCode());
        }

        // Expect good distribution - allow for some collisions
        hashes.Count.Should().BeGreaterThan(HashDistributionThreshold);
    }
    
    [Test]
    [Property("Category", "Concurrency")]
    public void NewCuid2_ShouldGenerateUniqueIds_InParallel()
    {
        HashSet<string> cuids = new(HighConcurrencyIterations, StringComparer.Ordinal);

#if NET10_0_OR_GREATER
        Lock lockObj = new();
#else
        object lockObj = new();
#endif

        Parallel.For(0, HighConcurrencyIterations, _ =>
        {
            Cuid2 cuid = new();
            string cuidString = cuid.ToString();

            lock ( lockObj )
            {
                cuids.Add(cuidString);
            }
        });

        cuids.Should().HaveCount(HighConcurrencyIterations);
    }

    [Test]
    [Property("Category", "Format")]
    public void ToString_DefaultStruct_ShouldReturnZeros()
    {
        Cuid2 defaultCuid = default;
        string result = defaultCuid.ToString();

        result.Should().Be(new string('0', DefaultCuid2Length));
    }

    [Test]
    [Property("Category", "Format")]
    public void ToString_ShouldBeLowercaseAlphanumeric()
    {
        Cuid2 cuid = new();
        string result = cuid.ToString();

        // Should be lowercase alphanumeric only
        result.Should().Be(result.ToLowerInvariant());
        result.Should().Match(s => s.All(char.IsLetterOrDigit));
    }

    [Test]
    [Property("Category", "Format")]
    public void ToString_ShouldStartWithLowercaseLetter()
    {
        for ( int i = 0; i < StandardTestIterations; i++ )
        {
            Cuid2 cuid = new();
            string result = cuid.ToString();

            char firstChar = result[0];
            char.IsLower(firstChar).Should().BeTrue();
            char.IsLetter(firstChar).Should().BeTrue();
        }
    }
}
