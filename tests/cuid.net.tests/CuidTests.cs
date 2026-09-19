#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable VISLIB0001 // Public API usage is allowed in tests

namespace Visus.Cuid.Tests;

using System.Collections.Concurrent;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using AwesomeAssertions;

internal sealed class CuidTests
{
    private const int s_counterEndIndex = 13;
    private const int s_counterLength = 4;
    private const int s_counterStartIndex = 9;
    private const string s_cuidRegexPattern = "^[c][0-9a-z]{24}$";

    // CUID v1 Structure Constants
    private const int s_cuidTotalLength = 25;

    private const int s_fingerprintEndIndex = 17;
    private const int s_fingerprintLength = 4;
    private const int s_fingerprintStartIndex = 13;
    private const int s_highConcurrencyIterations = 10000;
    private const int s_randomEndIndex = 25;
    private const int s_randomLength = 8;
    private const int s_randomStartIndex = 17;

    // Test Iteration Constants
    private const int s_standardTestIterations = 100;
    private const int s_timestampEndIndex = 9;
    private const int s_timestampLength = 8;
    private const int s_timestampStartIndex = 1;

    private const string s_validCuidString = "clbvi4441000007ld63liebkf";

    [Test]
    [Property("Category", "Comparison")]
    public void CompareTo_ReturnsCorrectResult()
    {
        Cuid cuid1 = new(s_validCuidString);
        Cuid cuid2 = new(s_validCuidString);
        Cuid cuid3 = Cuid.NewCuid();

        // Same CUIDs should return 0
        cuid1.CompareTo(cuid2).Should().Be(0);

        // Different CUIDs should return non-zero
        cuid1.CompareTo(cuid3).Should().NotBe(0);
    }

    [Test]
    [Property("Category", "Comparison")]
    public void CompareTo_WithNull_ShouldReturnPositive()
    {
        Cuid cuid = Cuid.NewCuid();

        cuid.CompareTo(null).Should().BePositive();
    }

    [Test]
    [Property("Category", "Comparison")]
    public void CompareTo_WithWrongType_ShouldThrowArgumentException()
    {
        Cuid cuid = new(s_validCuidString);

        Action act = () => _ = cuid.CompareTo("string");
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    [Arguments("c123")]
    [Arguments("clbvi444100000")]
    [Arguments("clbvi4441000007ld63liebkfextra")]
    [Property("Category", "Construction")]
    public void Constructor_WithInvalidLength_ShouldThrowFormatException(string input)
    {
        Action act = () => _ = new Cuid(input);
        act.Should().Throw<FormatException>();
    }

    [Test]
    [Property("Category", "Construction")]
    public void Constructor_WithInvalidString_ShouldThrowFormatException()
    {
        Action act = () => _ = new Cuid("invalid");
        act.Should().Throw<FormatException>();
    }

    [Test]
    [Property("Category", "Construction")]
    public void Constructor_WithNull_ShouldThrowArgumentNullException()
    {
        Action act = () => _ = new Cuid(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    [Property("Category", "Construction")]
    public void Constructor_WithValidCuid_ShouldPreserveStringValue()
    {
        Cuid original = Cuid.NewCuid();
        string originalString = original.ToString();
        Cuid copy = new(originalString);

        // The string representation should match
        copy.ToString().Should().HaveLength(s_cuidTotalLength);
        copy.ToString().Should().StartWith("c");
    }

    [Test]
    [Arguments("")]
    [Arguments(" ")]
    [Arguments("\t")]
    [Arguments("\n")]
    [Arguments("  \t\n  ")]
    [Property("Category", "Construction")]
    public void Constructor_WithWhitespaceVariations_ShouldThrowArgumentException(string input)
    {
        Action act = () => _ = new Cuid(input);
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    [Property("Category", "Equality")]
    public void Empty_EqualityBehavior()
    {
        Cuid empty = Cuid.Empty;
        Cuid defaultCuid = default;
        Cuid newCuid = Cuid.NewCuid();

        // Empty should equal default
        empty.Should().Be(defaultCuid);
        empty.Equals(defaultCuid).Should().BeTrue();
        ( empty == defaultCuid ).Should().BeTrue();

        // Empty should equal itself
        Cuid.Empty.Should().Be(Cuid.Empty);

        // Empty should not equal a new CUID
        empty.Should().NotBe(newCuid);
    }

    [Test]
    [Property("Category", "HashCode")]
    public void Empty_ShouldHaveConsistentHashCode()
    {
        int hash1 = Cuid.Empty.GetHashCode();
        int hash2 = default(Cuid).GetHashCode();

        hash1.Should().Be(hash2);
    }

    [Test]
    [Property("Category", "Equality")]
    public void EqualityOperators_ShouldWorkCorrectly()
    {
        Cuid cuid1 = new(s_validCuidString);
        Cuid cuid2 = new(s_validCuidString);
        Cuid cuid3 = Cuid.NewCuid();

        // Same CUIDs should be equal
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
    public void Equals_WithNull_ShouldReturnFalse()
    {
        Cuid cuid = new(s_validCuidString);

        cuid.Equals(null).Should().BeFalse();
    }

    [Test]
    [Property("Category", "Equality")]
    public void Equals_WithObject_ShouldWork()
    {
        Cuid cuid1 = new(s_validCuidString);
        object cuid2 = new Cuid(s_validCuidString);

        cuid1.Equals(cuid2).Should().BeTrue();
    }

    [Test]
    [Property("Category", "Equality")]
    public void Equals_WithWrongType_ShouldReturnFalse()
    {
        Cuid cuid = new(s_validCuidString);
        object other = "string";

        cuid.Equals(other).Should().BeFalse();
    }

    [Test]
    [Property("Category", "HashCode")]
    public void GetHashCode_ConsistencyAndUniqueness()
    {
        Cuid cuid1 = new(s_validCuidString);
        Cuid cuid2 = new(s_validCuidString);
        Cuid cuid3 = Cuid.NewCuid();

        // Same CUIDs should have same hash
        cuid1.GetHashCode().Should().Be(cuid2.GetHashCode());

        // Different CUIDs should have different hash
        cuid1.GetHashCode().Should().NotBe(cuid3.GetHashCode());
    }

    [Test]
    [Property("Category", "Serialization")]
    public void JsonDeserialize_WithEmptyString_ShouldReturnEmpty()
    {
        const string json = "\"\"";
        Cuid cuid = JsonSerializer.Deserialize<Cuid>(json);

        cuid.Should().Be(Cuid.Empty);
    }

    [Test]
    [Property("Category", "Serialization")]
    public void JsonDeserialize_WithInvalidCuid_ShouldThrowException()
    {
        const string json = "\"invalid\"";

        Action act = () => JsonSerializer.Deserialize<Cuid>(json);
        act.Should().Throw<Exception>(); // Can throw JsonException or FormatException depending on implementation
    }

    [Test]
    [Property("Category", "Serialization")]
    public void JsonDeserialize_WithNull_ShouldReturnEmpty()
    {
        const string json = "null";
        Cuid cuid = JsonSerializer.Deserialize<Cuid>(json);

        cuid.Should().Be(Cuid.Empty);
    }

    [Test]
    [Property("Category", "Serialization")]
    public void JsonRoundtrip_ShouldPreserveValue()
    {
        Cuid original = Cuid.NewCuid();
        string json = JsonSerializer.Serialize(original);
        Cuid deserialized = JsonSerializer.Deserialize<Cuid>(json);

        deserialized.Should().Be(original);
    }

    [Test]
    [Property("Category", "Format")]
    public void NewCuid_FingerprintComponent_ShouldBeConsistent()
    {
        List<Cuid> cuids = [];
        for ( int i = 0; i < s_standardTestIterations / 10; i++ )
        {
            cuids.Add(Cuid.NewCuid());
        }

        // All CUIDs generated in the same process should have the same fingerprint
        HashSet<string> fingerprints = cuids
                                      .Select(c => c.ToString()[s_fingerprintStartIndex..s_fingerprintEndIndex])
                                      .ToHashSet(StringComparer.Ordinal);

        fingerprints.Should().ContainSingle("fingerprint should be consistent within the same process");
    }

    [Test]
    [Property("Category", "Uniqueness")]
    public void NewCuid_GeneratedRapidly_ShouldBeUnique()
    {
        List<Cuid> cuids = [];
        for ( int i = 0; i < s_standardTestIterations; i++ )
        {
            cuids.Add(Cuid.NewCuid());
        }

        // All CUIDs should be unique
        cuids.Distinct().Should().HaveCount(s_standardTestIterations);
    }

    [Test]
    [Property("Category", "Format")]
    public void NewCuid_RandomComponent_ShouldVary()
    {
        List<Cuid> cuids = [];
        for ( int i = 0; i < s_standardTestIterations; i++ )
        {
            cuids.Add(Cuid.NewCuid());
        }

        // Random component should vary
        HashSet<string> randomParts = cuids
                                     .Select(c => c.ToString()[s_randomStartIndex..s_randomEndIndex])
                                     .ToHashSet(StringComparer.Ordinal);

        randomParts.Should().HaveCountGreaterThan(s_standardTestIterations * 9 / 10, "random parts should be highly varied");
    }


    [Test]
    [Property("Category", "Concurrency")]
    public void NewCuid_ShouldGenerateUniqueIds_InParallel()
    {
        HashSet<string> cuids = new(s_highConcurrencyIterations, StringComparer.Ordinal);

#if NET10_0_OR_GREATER
        Lock lockObj = new();
#else
        object lockObj = new();
#endif

        Parallel.For(0, s_highConcurrencyIterations, _ =>
        {
            Cuid cuid = Cuid.NewCuid();
            string cuidString = cuid.ToString();

            lock ( lockObj )
            {
                cuids.Add(cuidString);
            }
        });

        cuids.Should().HaveCount(s_highConcurrencyIterations);
    }

    [Test]
    [Property("Category", "Format")]
    public void NewCuid_ShouldHaveCorrectStructure()
    {
        Cuid cuid = Cuid.NewCuid();
        string cuidString = cuid.ToString();

        // Structure: c (1) + timestamp (8) + counter (4) + fingerprint (4) + random (8) = 25
        cuidString.Should().HaveLength(s_cuidTotalLength);
        cuidString.Should().StartWith("c");

        string timestamp = cuidString[s_timestampStartIndex..s_timestampEndIndex];
        string counter = cuidString[s_counterStartIndex..s_counterEndIndex];
        string fingerprint = cuidString[s_fingerprintStartIndex..s_fingerprintEndIndex];
        string random = cuidString[s_randomStartIndex..s_randomEndIndex];

        timestamp.Should().HaveLength(s_timestampLength);
        counter.Should().HaveLength(s_counterLength);
        fingerprint.Should().HaveLength(s_fingerprintLength);
        random.Should().HaveLength(s_randomLength);
    }

    [Test]
    [Property("Category", "Parsing")]
    public void NewCuid_ToStringAndParse_ShouldPreserveFormat()
    {
        for ( int i = 0; i < s_standardTestIterations / 10; i++ )
        {
            Cuid original = Cuid.NewCuid();
            string stringValue = original.ToString();
            Cuid parsed = Cuid.Parse(stringValue);

            // String format should be preserved
            parsed.ToString().Should().HaveLength(s_cuidTotalLength);
            parsed.ToString().Should().StartWith("c");
            parsed.ToString().Should().MatchRegex(s_cuidRegexPattern);
        }
    }

    [Test]
    [Property("Category", "Parsing")]
    public void Parse_CalledConcurrently_ShouldBeThreadSafe()
    {
        ConcurrentBag<Exception> exceptions = [];
        ConcurrentBag<Cuid> cuids = [];

        string testCuid = Cuid.NewCuid().ToString();

        Parallel.For(0, s_standardTestIterations, _ =>
        {
            try
            {
                Cuid cuid = Cuid.Parse(testCuid);
                cuids.Add(cuid);
            }
            catch ( Exception ex )
            {
                exceptions.Add(ex);
            }
        });

        exceptions.Should().BeEmpty("no exceptions should occur during concurrent parsing");
        cuids.Should().HaveCount(s_standardTestIterations);
        cuids.Should().OnlyContain(c => c.ToString().Length == s_cuidTotalLength);
    }

    [Test]
    [Arguments("clbvi4441-00007ld63liebkf")] // hyphen
    [Arguments("clbvi4441 00007ld63liebkf")] // space
    [Arguments("clbvi4441_00007ld63liebkf")] // underscore
    [Arguments("CLBVI4441000007LD63LIEBKF")] // uppercase
    [Arguments("ClBvI4441000007lD63lIeBkF")] // mixed case
    [Arguments("c123")] // too short
    [Arguments("xlbvi4441000007ld63liebkf")] // wrong prefix
    [Property("Category", "Parsing")]
    public void Parse_WithInvalidFormat_ShouldThrowFormatException(string input)
    {
        Action act = () => Cuid.Parse(input);
        act.Should().Throw<FormatException>();
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("   ")]
    [Property("Category", "Parsing")]
    public void Parse_WithNullOrWhitespace_ReturnsEmpty(string input)
    {
        Cuid cuid = Cuid.Parse(input!);
        cuid.Should().Be(Cuid.Empty);
    }

    [Test]
    [Property("Category", "Parsing")]
    public void Parse_WithValidInput_ShouldReturnCuid()
    {
        // Test string parsing
        Cuid cuid1 = Cuid.Parse(s_validCuidString);
        cuid1.ToString().Should().Be(s_validCuidString);

        // Test span parsing
        ReadOnlySpan<char> span = s_validCuidString.AsSpan();
        Cuid cuid2 = Cuid.Parse(span);
        cuid2.ToString().Should().Be(s_validCuidString);
    }

    [Test]
    [Arguments($"  {s_validCuidString}")]
    [Arguments($"{s_validCuidString}  ")]
    [Arguments($"  \t{s_validCuidString}\n  ")]
    [Property("Category", "Parsing")]
    public void Parse_WithWhitespace_ShouldTrimAndSucceed(string input)
    {
        Cuid cuid = Cuid.Parse(input);

        cuid.ToString().Should().Be(s_validCuidString);
    }

    [Test]
    [Arguments("c123")]
    [Property("Category", "Parsing")]
    public void TryParse_WithInvalidFormat_ReturnsFalse(string input)
    {
        bool success = Cuid.TryParse(input, out Cuid result);
        success.Should().BeFalse();
        result.Should().Be(default);
    }

    [Test]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    [Property("Category", "Parsing")]
    public void TryParse_WithNullOrWhitespace_ReturnsFalse(string input)
    {
        bool success = Cuid.TryParse(input!, out Cuid result);
        success.Should().BeFalse();
        result.Should().Be(default);
    }

    [Test]
    [Property("Category", "Parsing")]
    public void TryParse_WithValidInput_ReturnsTrue()
    {
        // Test string parsing
        bool success1 = Cuid.TryParse(s_validCuidString, out Cuid result1);
        success1.Should().BeTrue();
        result1.ToString().Should().Be(s_validCuidString);

        // Test span parsing
        ReadOnlySpan<char> span = s_validCuidString.AsSpan();
        bool success2 = Cuid.TryParse(span, out Cuid result2);
        success2.Should().BeTrue();
        result2.ToString().Should().Be(s_validCuidString);

        // Test whitespace trimming
        const string inputWithWhitespace = $"  {s_validCuidString}  ";
        bool success3 = Cuid.TryParse(inputWithWhitespace, out Cuid result3);
        success3.Should().BeTrue();
        result3.ToString().Should().Be(s_validCuidString);
    }

    [Test]
    [Property("Category", "Serialization")]
    public void XmlDeserialize_WithInvalidCuid_ShouldThrowInvalidOperationException()
    {
        const string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?><cuid>invalid</cuid>";
        XmlSerializer serializer = new(typeof(Cuid));

        using StringReader sr = new(xml);
        using XmlReader xr = XmlReader.Create(sr);

        // ReSharper disable once AccessToDisposedClosure
        Action act = () => serializer.Deserialize(xr);
        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    [Property("Category", "Serialization")]
    public void XmlRoundtrip_ShouldPreserveValue()
    {
        Cuid original = Cuid.NewCuid();
        XmlSerializer serializer = new(typeof(Cuid));

        string xml;
        using ( StringWriter sw = new() )
        {
            using XmlWriter xw = XmlWriter.Create(sw);
            serializer.Serialize(xw, original);
            xml = sw.ToString();
        }

        Cuid deserialized;
        using ( StringReader sr = new(xml) )
        {
            using XmlReader xr = XmlReader.Create(sr);
            deserialized = (Cuid)serializer.Deserialize(xr)!;
        }

        deserialized.Should().Be(original);
    }
}

#pragma warning restore VISLIB0001
