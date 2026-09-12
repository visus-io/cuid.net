namespace Visus.Cuid.Tests;

using System.Numerics;
using AwesomeAssertions;

internal sealed class UtilsTests
{
    private const int RoundTripIterations = 1000;

    [Test]
    [Property("Category", "Decoding")]
    public void DecodeUlong_WithKnownValue_ShouldReturnExpectedValue()
    {
        ulong result = Utils.DecodeUlong("10");

        result.Should().Be(36UL);
    }

    [Test]
    [Property("Category", "Decoding")]
    [Arguments("0", 0UL)]
    [Arguments("1", 1UL)]
    [Arguments("9", 9UL)]
    [Arguments("a", 10UL)]
    [Arguments("z", 35UL)]
    public void DecodeUlong_WithSingleDigit_ShouldReturnDigitValue(string input, ulong expected)
    {
        ulong result = Utils.DecodeUlong(input);

        result.Should().Be(expected);
    }

    [Test]
    [Property("Category", "Decoding")]
    public void DecodeUlong_WithUlongMaxValueEncoded_ShouldRoundTrip()
    {
        string encoded = Utils.Encode(ulong.MaxValue);
        ulong decoded = Utils.DecodeUlong(encoded);

        decoded.Should().Be(ulong.MaxValue);
    }

    [Test]
    [Property("Category", "Decoding")]
    public void Decode_WithAllDigits_ShouldMatchManualCalculation()
    {
        long result = Utils.Decode("0123456789abcdefghijklmnopqrstuvwxyz");

        long expected = 0;

        foreach ( char c in "0123456789abcdefghijklmnopqrstuvwxyz" )
        {
            int digit = c is >= '0' and <= '9' ? c - '0' : 10 + c - 'a';
            expected = ( expected * 36 ) + digit;
        }

        result.Should().Be(expected);
    }

    [Test]
    [Property("Category", "Decoding")]
    public void Decode_WithKnownValue_ShouldReturnExpectedLong()
    {
        long result = Utils.Decode("10");

        result.Should().Be(36);
    }

    [Test]
    [Property("Category", "Decoding")]
    [Arguments("0", 0L)]
    [Arguments("1", 1L)]
    [Arguments("9", 9L)]
    [Arguments("a", 10L)]
    [Arguments("z", 35L)]
    public void Decode_WithSingleDigit_ShouldReturnDigitValue(string input, long expected)
    {
        long result = Utils.Decode(input);

        result.Should().Be(expected);
    }

    [Test]
    [Property("Category", "Decoding")]
    public void Decode_WithZero_ShouldReturnZero()
    {
        long result = Utils.Decode("0");

        result.Should().Be(0);
    }

    [Test]
    [Property("Category", "Encoding")]
    [Arguments(1UL, "1")]
    [Arguments(9UL, "9")]
    [Arguments(10UL, "a")]
    [Arguments(35UL, "z")]
    [Arguments(36UL, "10")]
    public void Encode_UlongOverload_WithKnownValue_ShouldMatchExpectedBase36Value(ulong value, string expected)
    {
        string result = Utils.Encode(value);

        result.Should().Be(expected);
    }

    [Test]
    [Property("Category", "Encoding")]
    public void Encode_UlongOverload_WithMaxValue_ShouldOnlyContainLowercaseAlphanumericCharacters()
    {
        string result = Utils.Encode(ulong.MaxValue);

        result.Should().MatchRegex("^[0-9a-z]+$");
    }

    [Test]
    [Property("Category", "Encoding")]
    #pragma warning disable CA5394 // Random is fine for generating deterministic test data
    public void Encode_UlongOverload_WithRandomValues_ShouldRoundTripThroughDecodeUlong()
    {
        Random random = new(54321);
        byte[] bytes = new byte[8];

        for ( int iteration = 0; iteration < RoundTripIterations; iteration++ )
        {
            random.NextBytes(bytes);

            ulong expected = 0;

            for ( int i = 7; i >= 0; i-- )
            {
                expected = ( expected << 8 ) | bytes[i];
            }

            string encoded = Utils.Encode(expected);
            ulong decoded = Utils.DecodeUlong(encoded);

            decoded.Should().Be(expected);
        }
    }
    #pragma warning restore CA5394

    [Test]
    [Property("Category", "Encoding")]
    public void Encode_UlongOverload_WithZero_ShouldReturnEmptyString()
    {
        string result = Utils.Encode(0UL);

        result.Should().BeEmpty();
    }

    [Test]
    [Property("Category", "Encoding")]
    [Arguments(1)]
    [Arguments(8)]
    [Arguments(16)]
    [Arguments(64)]
    public void Encode_WithAllZeroBytes_ShouldReturnEmptyString(int length)
    {
        byte[] value = new byte[length];

        string result = Utils.Encode(value);

        result.Should().BeEmpty();
    }

    [Test]
    [Property("Category", "Encoding")]
    public void Encode_WithEmptySpan_ShouldReturnEmptyString()
    {
        string result = Utils.Encode([]);

        result.Should().BeEmpty();
    }

    [Test]
    [Property("Category", "Encoding")]
    public void Encode_WithKnownMultiByteValue_ShouldMatchExpectedBase36Value()
    {
        // Little-endian bytes for 36 (1 * 36 + 0).
        string result = Utils.Encode(new byte[]
        {
            36,
            0,
        });

        result.Should().Be("10");
    }

    [Test]
    [Property("Category", "Encoding")]
    public void Encode_WithMaxByteValues_ShouldOnlyContainLowercaseAlphanumericCharacters()
    {
        byte[] value = new byte[64];

        for ( int i = 0; i < value.Length; i++ )
        {
            value[i] = byte.MaxValue;
        }

        string result = Utils.Encode(value);

        result.Should().MatchRegex("^[0-9a-z]+$");
    }

    [Test]
    [Property("Category", "Encoding")]
    public void Encode_WithMaxByteValues_ShouldMatchIndependentBase36Reference()
    {
        byte[] value = new byte[64];

        for ( int i = 0; i < value.Length; i++ )
        {
            value[i] = byte.MaxValue;
        }

        string expected = EncodeWithBigIntegerReference(value);
        string result = Utils.Encode(value);

        result.Should().Be(expected);
    }

    [Test]
    [Property("Category", "Encoding")]
    #pragma warning disable CA5394 // Random is fine for generating deterministic test data
    public void Encode_WithSha3DigestSizedInput_ShouldMatchIndependentBase36Reference()
    {
        Random random = new(2026);

        for ( int iteration = 0; iteration < RoundTripIterations; iteration++ )
        {
            byte[] value = new byte[64];
            random.NextBytes(value);

            string expected = EncodeWithBigIntegerReference(value);
            string result = Utils.Encode(value);

            result.Should().Be(expected);
        }
    }
    #pragma warning restore CA5394

    [Test]
    [Property("Category", "Encoding")]
    #pragma warning disable CA5394 // Random is fine for generating deterministic test data
    public void Encode_WithRandomBytes_ShouldRoundTripThroughDecodeUlong()
    {
        Random random = new(12345);

        for ( int iteration = 0; iteration < RoundTripIterations; iteration++ )
        {
            byte[] bytes = new byte[8];
            random.NextBytes(bytes);

            ulong expected = 0;

            for ( int i = 0; i < 8; i++ )
            {
                expected |= (ulong)bytes[i] << ( 8 * i );
            }

            string encoded = Utils.Encode(bytes);
            ulong decoded = Utils.DecodeUlong(encoded);

            decoded.Should().Be(expected);
        }
    }
    #pragma warning restore CA5394

    [Test]
    [Property("Category", "Encoding")]
    public void Encode_WithSingleByteMaxValue_ShouldReturnHighestBase36Digit()
    {
        string result = Utils.Encode([35,]);

        result.Should().Be("z");
    }

    [Test]
    [Property("Category", "Random")]
    public void GenerateCharacterPrefix_ShouldProduceVariedOutput()
    {
        HashSet<char> results = [];

        for ( int i = 0; i < RoundTripIterations; i++ )
        {
            results.Add(Utils.GenerateCharacterPrefix());
        }

        results.Count.Should().BeGreaterThan(1);
    }

    [Test]
    [Property("Category", "Random")]
    public void GenerateCharacterPrefix_ShouldReturnLowercaseLetter()
    {
        for ( int i = 0; i < RoundTripIterations; i++ )
        {
            char result = Utils.GenerateCharacterPrefix();

            char.IsLower(result).Should().BeTrue();
            char.IsLetter(result).Should().BeTrue();
        }
    }

    [Test]
    [Property("Category", "Random")]
    public void GenerateRandom_ShouldProduceDifferentValues()
    {
        byte[] first = Utils.GenerateRandom(16);
        byte[] second = Utils.GenerateRandom(16);

        first.Should().NotBeEquivalentTo(second);
    }

    [Test]
    [Property("Category", "Random")]
    [Arguments(1)]
    [Arguments(8)]
    [Arguments(16)]
    [Arguments(32)]
    [Arguments(64)]
    public void GenerateRandom_WithCustomLength_ShouldReturnRequestedLength(int length)
    {
        byte[] result = Utils.GenerateRandom(length);

        result.Should().HaveCount(length);
    }

    [Test]
    [Property("Category", "Random")]
    public void GenerateRandom_WithDefaultLength_ShouldReturnEightBytes()
    {
        byte[] result = Utils.GenerateRandom();

        result.Should().HaveCount(8);
    }

    /// <summary>
    /// Encodes a little-endian byte span to base-36 using <see cref="BigInteger" />, independently
    /// of <see cref="Utils.Encode(ReadOnlySpan{byte})" />, to validate multi-limb inputs such as the
    /// 64-byte SHA-3 512 digest that <c>Cuid2</c> encodes.
    /// </summary>
    private static string EncodeWithBigIntegerReference(byte[] littleEndianValue)
    {
        byte[] unsigned = new byte[littleEndianValue.Length + 1];
        Array.Copy(littleEndianValue, unsigned, littleEndianValue.Length);

        BigInteger magnitude = new(unsigned);

        if ( magnitude.IsZero )
        {
            return string.Empty;
        }

        const string digits = "0123456789abcdefghijklmnopqrstuvwxyz";
        System.Text.StringBuilder builder = new();

        while ( magnitude > 0 )
        {
            builder.Insert(0, digits[(int)( magnitude % 36 )]);
            magnitude /= 36;
        }

        return builder.ToString();
    }
}
