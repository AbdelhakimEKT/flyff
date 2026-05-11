using Hellion.Core.Cryptography;
using Xunit;

namespace Hellion.Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void Hash_produces_bcrypt_prefixed_string()
        {
            string hash = PasswordHasher.Hash("hunter2!");
            Assert.True(PasswordHasher.IsBCryptHash(hash));
            Assert.NotEqual("hunter2!", hash);
        }

        [Fact]
        public void Verify_accepts_correct_password()
        {
            string hash = PasswordHasher.Hash("CorrectHorseBattery42!");
            Assert.True(PasswordHasher.Verify("CorrectHorseBattery42!", hash));
        }

        [Fact]
        public void Verify_rejects_wrong_password()
        {
            string hash = PasswordHasher.Hash("CorrectHorseBattery42!");
            Assert.False(PasswordHasher.Verify("wrong", hash));
        }

        [Fact]
        public void Verify_falls_back_to_legacy_string_compare_for_non_bcrypt()
        {
            string legacyMd5 = "89d1ed22aac58f5bbea53b2fde81a946";
            Assert.True(PasswordHasher.Verify(legacyMd5, legacyMd5));
            Assert.True(PasswordHasher.Verify(legacyMd5.ToUpper(), legacyMd5));
            Assert.False(PasswordHasher.Verify("nope", legacyMd5));
        }

        [Theory]
        [InlineData("", "any")]
        [InlineData("any", "")]
        [InlineData("", "")]
        public void Verify_returns_false_for_empty_inputs(string password, string hash)
        {
            Assert.False(PasswordHasher.Verify(password, hash));
        }
    }
}
