using LashBooking.Web.MVC.Services;

namespace LashBooking.Tests
{
    public class PasswordHasherTests
    {
        [Fact]
        public void Verify_CorrectPassword_ReturnsTrue() // верный пароль проходит проверку
        {       
            var password = "MySecret123";
            var hash = PasswordHasher.Hash(password);

            var result = PasswordHasher.Verify(password, hash);

            Assert.True(result);
        }

        [Fact]
        public void Verify_WrongPassword_ReturnsFalse() // неверный пароль не проходит
        {
            var correctPassword = "MySecret123";
            var wrongPassword = "WrongPassword";
            var hash = PasswordHasher.Hash(correctPassword);

            var result = PasswordHasher.Verify(wrongPassword, hash);

            Assert.False(result);
        }

        [Fact]
        public void Hash_SamePasswordTwice_ProducesDifferentHashes()
        {
            var password = "MySecret123";

            var hash1 = PasswordHasher.Hash(password);
            var hash2 = PasswordHasher.Hash(password);

            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void Verify_InvalidHash_ReturnsFalse()
        {
            var password = "MySecret123";
            var invalidHash = "это_не_base64_хэш!!!";

            var result = PasswordHasher.Verify(password, invalidHash);

            Assert.False(result);
        }


        [Fact]
        public void Hash_NullPassword_ThrowsException() // +
        {
            Assert.Throws<ArgumentNullException>(() => PasswordHasher.Hash(null!));
        }

        [Fact]
        public void Verify_NullPassword_ReturnsFalse()
        {
            var result = PasswordHasher.Verify(null!, "someHash");
            Assert.False(result);
        }

        [Fact]
        public void Verify_WrongLengthHash_ReturnsFalse()
        {
            var shortHash = Convert.ToBase64String(new byte[10]);
            var result = PasswordHasher.Verify("password", shortHash);
            Assert.False(result);
        }
    }
}