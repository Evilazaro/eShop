namespace WebApp.EmailValidation.UnitTests
{

    /// <summary>
    /// Comprehensive unit tests for the EmailValidation class.
    /// Tests cover standard formats, edge cases, length validations, and error messaging.
    /// </summary>  
    [TestClass]
    public class EmailValidationTests
    {
        private EmailValidation _validator = null!;

        [TestInitialize]
        public void Setup()
        {
            _validator = new EmailValidation();
        }

        #region Valid Email Tests

        /// <summary>
        /// Test standard valid email formats that should pass validation.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_StandardValidFormats_ReturnsTrue()
        {
            // Arrange
            var validEmails = new[]
            {
            "user@example.com",
            "john.doe@company.com",
            "test_user@domain.co.uk",
            "admin+tag@website.org",
            "contact-us@business.net",
            "info123@test456.com"
        };

            // Act & Assert
            foreach (var email in validEmails)
            {
                var result = _validator.ValidateEmail(email);
                Assert.IsTrue(result.IsValid, $"Expected '{email}' to be valid but got: {result.ErrorMessage}");
                Assert.AreEqual(string.Empty, result.ErrorMessage);
            }
        }

        /// <summary>
        /// Test valid emails with numbers in various positions.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_WithNumbers_ReturnsTrue()
        {
            // Arrange
            var validEmails = new[]
            {
            "user123@example.com",
            "123user@example.com",
            "user@example123.com",
            "user@123example.com",
            "1@example.com"
        };

            // Act & Assert
            foreach (var email in validEmails)
            {
                var result = _validator.ValidateEmail(email);
                Assert.IsTrue(result.IsValid, $"Expected '{email}' to be valid but got: {result.ErrorMessage}");
            }
        }

        /// <summary>
        /// Test valid emails with special characters allowed in local part.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_WithAllowedSpecialCharacters_ReturnsTrue()
        {
            // Arrange
            var validEmails = new[]
            {
            "user.name@example.com",
            "user_name@example.com",
            "user+tag@example.com",
            "user-name@example.com",
            "first.last@example.com"
        };

            // Act & Assert
            foreach (var email in validEmails)
            {
                var result = _validator.ValidateEmail(email);
                Assert.IsTrue(result.IsValid, $"Expected '{email}' to be valid but got: {result.ErrorMessage}");
            }
        }

        /// <summary>
        /// Test valid emails with various TLD lengths.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_WithVariousTLDs_ReturnsTrue()
        {
            // Arrange
            var validEmails = new[]
            {
            "user@example.co",
            "user@example.com",
            "user@example.info",
            "user@example.photography"
        };

            // Act & Assert
            foreach (var email in validEmails)
            {
                var result = _validator.ValidateEmail(email);
                Assert.IsTrue(result.IsValid, $"Expected '{email}' to be valid but got: {result.ErrorMessage}");
            }
        }

        /// <summary>
        /// Test valid emails with subdomains.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_WithSubdomains_ReturnsTrue()
        {
            // Arrange
            var validEmails = new[]
            {
            "user@mail.example.com",
            "user@sub.mail.example.com",
            "user@a.b.c.example.com"
        };

            // Act & Assert
            foreach (var email in validEmails)
            {
                var result = _validator.ValidateEmail(email);
                Assert.IsTrue(result.IsValid, $"Expected '{email}' to be valid but got: {result.ErrorMessage}");
            }
        }

        /// <summary>
        /// Test valid email with maximum allowed lengths.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_AtMaximumLength_ReturnsTrue()
        {
            // Arrange - Create email with 64 char local part + @ + 188 char domain = 253 chars total
            var localPart = new string('a', 63) + "b"; // 64 characters
            var domainPart = new string('d', 61) + "." + new string('d', 61) + "." + new string('d', 61) + ".com"; // Under 255
            var email = $"{localPart}@{domainPart}";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsTrue(result.IsValid, $"Expected max length email to be valid but got: {result.ErrorMessage}");
        }

        /// <summary>
        /// Test that email validation trims whitespace.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_WithSurroundingWhitespace_TrimsAndValidates()
        {
            // Arrange
            var email = "  user@example.com  ";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsTrue(result.IsValid, $"Expected trimmed email to be valid but got: {result.ErrorMessage}");
        }

        #endregion

        #region Invalid Email Tests - Format Issues

        /// <summary>
        /// Test emails with invalid formats that should fail validation.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_InvalidFormats_ReturnsFalse()
        {
            // Arrange
            var invalidEmails = new[]
            {
            "plaintext",
            "missing-at-sign.com",
            "@example.com",
            "user@",
            "user@@example.com",
            "user@example",
            "user @example.com",
            "user@exam ple.com",
            "user@.example.com",
            "user@example..com"
        };

            // Act & Assert
            foreach (var email in invalidEmails)
            {
                var result = _validator.ValidateEmail(email);
                Assert.IsFalse(result.IsValid, $"Expected '{email}' to be invalid");
                Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage), $"Expected error message for '{email}'");
            }
        }

        /// <summary>
        /// Test null email address.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_NullEmail_ReturnsFalseWithMessage()
        {
            // Arrange
            string? email = null;

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Email address cannot be empty.", result.ErrorMessage);
        }

        /// <summary>
        /// Test empty email address.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_EmptyEmail_ReturnsFalseWithMessage()
        {
            // Arrange
            var email = string.Empty;

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Email address cannot be empty.", result.ErrorMessage);
        }

        /// <summary>
        /// Test whitespace-only email address.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_WhitespaceOnly_ReturnsFalseWithMessage()
        {
            // Arrange
            var email = "   ";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Email address cannot be empty.", result.ErrorMessage);
        }

        /// <summary>
        /// Test email missing @ symbol.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_MissingAtSymbol_ReturnsFalseWithSpecificMessage()
        {
            // Arrange
            var email = "userexample.com";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Email address must contain an '@' symbol.", result.ErrorMessage);
        }

        /// <summary>
        /// Test email with multiple @ symbols.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_MultipleAtSymbols_ReturnsFalseWithSpecificMessage()
        {
            // Arrange
            var email = "user@@example.com";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Email address cannot contain multiple '@' symbols.", result.ErrorMessage);
        }

        /// <summary>
        /// Test email with consecutive dots.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_ConsecutiveDots_ReturnsFalseWithSpecificMessage()
        {
            // Arrange
            var invalidEmails = new[]
            {
            "user..name@example.com",
            "user@example..com",
            "user@exam..ple.com"
        };

            // Act & Assert
            foreach (var email in invalidEmails)
            {
                var result = _validator.ValidateEmail(email);
                Assert.IsFalse(result.IsValid, $"Expected '{email}' to be invalid");
                Assert.AreEqual("Email address cannot contain consecutive dots.", result.ErrorMessage);
            }
        }

        /// <summary>
        /// Test email with dots at start or end of local part.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_DotsAtLocalPartBoundaries_ReturnsFalseWithSpecificMessage()
        {
            // Arrange
            var invalidEmails = new[]
            {
            ".user@example.com",
            "user.@example.com",
            ".user.@example.com"
        };

            // Act & Assert
            foreach (var email in invalidEmails)
            {
                var result = _validator.ValidateEmail(email);
                Assert.IsFalse(result.IsValid, $"Expected '{email}' to be invalid");
                Assert.AreEqual("Email local part cannot start or end with a dot.", result.ErrorMessage);
            }
        }

        /// <summary>
        /// Test email with invalid domain format.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_InvalidDomainFormat_ReturnsFalseWithSpecificMessage()
        {
            // Arrange
            var invalidEmails = new[]
            {
            "user@.example.com",
            "user@example.com.",
            "user@-example.com",
            "user@example-.com"
        };

            // Act & Assert
            foreach (var email in invalidEmails)
            {
                var result = _validator.ValidateEmail(email);
                Assert.IsFalse(result.IsValid, $"Expected '{email}' to be invalid");
                Assert.AreEqual("Email domain has invalid format.", result.ErrorMessage);
            }
        }

        #endregion

        #region Length Validation Tests

        /// <summary>
        /// Test email exceeding maximum total length (254 characters per RFC 5321).
        /// </summary>
        [TestMethod]
        public void ValidateEmail_ExceedsMaximumLength_ReturnsFalseWithSpecificMessage()
        {
            // Arrange - Create email longer than 254 characters
            var localPart = new string('a', 64);
            var domainPart = new string('d', 200) + ".com"; // Total will exceed 254
            var email = $"{localPart}@{domainPart}";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Email address exceeds maximum length of 254 characters.", result.ErrorMessage);
        }

        /// <summary>
        /// Test email with local part exceeding 64 characters.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_LocalPartExceedsMaxLength_ReturnsFalseWithSpecificMessage()
        {
            // Arrange - Create local part with 65 characters
            var localPart = new string('a', 65);
            var email = $"{localPart}@example.com";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Email local part exceeds maximum length of 64 characters.", result.ErrorMessage);
        }

        /// <summary>
        /// Test email with domain part exceeding 255 characters.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_DomainExceedsMaxLength_ReturnsFalseWithSpecificMessage()
        {
            // Arrange - Create domain longer than 255 characters
            var domainPart = new string('d', 256) + ".com";
            var email = $"user@{domainPart}";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Email domain exceeds maximum length of 255 characters.", result.ErrorMessage);
        }

        /// <summary>
        /// Test email with empty local part.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_EmptyLocalPart_ReturnsFalseWithSpecificMessage()
        {
            // Arrange
            var email = "@example.com";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Email address local part cannot be empty.", result.ErrorMessage);
        }

        /// <summary>
        /// Test email with empty domain.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_EmptyDomain_ReturnsFalseWithSpecificMessage()
        {
            // Arrange
            var email = "user@";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Email address domain cannot be empty.", result.ErrorMessage);
        }

        /// <summary>
        /// Test minimum valid email length.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_MinimumLength_ReturnsTrue()
        {
            // Arrange - Shortest possible valid email: a@b.co (6 characters)
            var email = "a@b.co";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsTrue(result.IsValid, $"Expected minimum length email to be valid but got: {result.ErrorMessage}");
        }

        #endregion

        #region Edge Cases and Uncommon Formats

        /// <summary>
        /// Test email with single character local part.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_SingleCharacterLocalPart_ReturnsTrue()
        {
            // Arrange
            var email = "a@example.com";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsTrue(result.IsValid, $"Expected single char local part to be valid but got: {result.ErrorMessage}");
        }

        /// <summary>
        /// Test email with single character domain labels.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_SingleCharacterDomainLabels_ReturnsTrue()
        {
            // Arrange
            var email = "user@a.b.com";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsTrue(result.IsValid, $"Expected single char domain labels to be valid but got: {result.ErrorMessage}");
        }

        /// <summary>
        /// Test email with all allowed special characters in local part.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_AllAllowedSpecialChars_ReturnsTrue()
        {
            // Arrange
            var email = "user.name_test+tag-here@example.com";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsTrue(result.IsValid, $"Expected all special chars to be valid but got: {result.ErrorMessage}");
        }

        /// <summary>
        /// Test email with numbers only.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_NumbersOnly_ReturnsTrue()
        {
            // Arrange
            var email = "123456@789012.com";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsTrue(result.IsValid, $"Expected numbers-only email to be valid but got: {result.ErrorMessage}");
        }

        /// <summary>
        /// Test email with mixed case.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_MixedCase_ReturnsTrue()
        {
            // Arrange
            var valid Emails = new[]
            {
            "User@Example.Com",
            "USER@EXAMPLE.COM",
            "uSeR@eXaMpLe.CoM"
        };

            // Act & Assert
            foreach (var email in valid Emails)
            {
                var result = _validator.ValidateEmail(email);
                Assert.IsTrue(result.IsValid, $"Expected mixed case '{email}' to be valid but got: {result.ErrorMessage}");
            }
        }

        /// <summary>
        /// Test email with long TLD.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_LongTLD_ReturnsTrue()
        {
            // Arrange
            var email = "user@example.photography";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsTrue(result.IsValid, $"Expected long TLD to be valid but got: {result.ErrorMessage}");
        }

        /// <summary>
        /// Test email with hyphenated domain.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_HyphenatedDomain_ReturnsTrue()
        {
            // Arrange
            var email = "user@my-domain.com";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsTrue(result.IsValid, $"Expected hyphenated domain to be valid but got: {result.ErrorMessage}");
        }

        /// <summary>
        /// Test emails with disallowed special characters.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_DisallowedSpecialCharacters_ReturnsFalse()
        {
            // Arrange
            var invalidEmails = new[]
            {
            "user#name@example.com",
            "user$name@example.com",
            "user%name@example.com",
            "user&name@example.com",
            "user*name@example.com",
            "user=name@example.com",
            "user!name@example.com",
            "user?name@example.com",
            "user/name@example.com",
            "user\\name@example.com",
            "user name@example.com",
            "user@exam!ple.com"
        };

            // Act & Assert
            foreach (var email in invalidEmails)
            {
                var result = _validator.ValidateEmail(email);
                Assert.IsFalse(result.IsValid, $"Expected '{email}' with disallowed chars to be invalid");
            }
        }

        /// <summary>
        /// Test email without TLD (top-level domain).
        /// </summary>
        [TestMethod]
        public void ValidateEmail_WithoutTLD_ReturnsFalse()
        {
            // Arrange
            var email = "user@localhost";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
        }

        /// <summary>
        /// Test email with single character TLD (invalid per current standards).
        /// </summary>
        [TestMethod]
        public void ValidateEmail_SingleCharTLD_ReturnsFalse()
        {
            // Arrange
            var email = "user@example.c";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage));
        }

        #endregion

        #region Error Handling Tests

        /// <summary>
        /// Test that validation returns appropriate error messages for various invalid scenarios.
        /// This ensures error messages are clear and actionable.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_VariousInvalidInputs_ReturnsAppropriateErrorMessages()
        {
            // Arrange & Act & Assert
            var testCases = new Dictionary<string?, string>
        {
            { null, "Email address cannot be empty." },
            { "", "Email address cannot be empty." },
            { "   ", "Email address cannot be empty." },
            { "no-at-sign.com", "Email address must contain an '@' symbol." },
            { "double@@at.com", "Email address cannot contain multiple '@' symbols." },
            { "@example.com", "Email address local part cannot be empty." },
            { "user@", "Email address domain cannot be empty." },
            { "user..name@example.com", "Email address cannot contain consecutive dots." },
            { ".user@example.com", "Email local part cannot start or end with a dot." },
            { "user@.example.com", "Email domain has invalid format." }
        };

            foreach (var testCase in testCases)
            {
                var result = _validator.ValidateEmail(testCase.Key);
                Assert.IsFalse(result.IsValid, $"Expected '{testCase.Key}' to be invalid");
                Assert.AreEqual(testCase.Value, result.ErrorMessage,
                    $"Expected specific error message for '{testCase.Key}'");
            }
        }

        /// <summary>
        /// Test that valid emails return empty error messages.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_ValidEmails_ReturnsEmptyErrorMessage()
        {
            // Arrange
            var validEmails = new[]
            {
            "user@example.com",
            "test@domain.co.uk",
            "admin+tag@site.org"
        };

            // Act & Assert
            foreach (var email in validEmails)
            {
                var result = _validator.ValidateEmail(email);
                Assert.IsTrue(result.IsValid, $"Expected '{email}' to be valid");
                Assert.AreEqual(string.Empty, result.ErrorMessage,
                    $"Expected empty error message for valid email '{email}'");
            }
        }

        #endregion

        #region Boundary and Performance Tests

        /// <summary>
        /// Test email at exactly 254 characters (RFC 5321 maximum).
        /// </summary>
        [TestMethod]
        public void ValidateEmail_Exactly254Characters_ReturnsTrue()
        {
            // Arrange - Build email with exactly 254 characters
            // Format: 64-char-local@189-char-domain (64 + 1 + 189 = 254)
            var localPart = new string('a', 64);
            var domainPart = new string('d', 61) + "." + new string('d', 61) + "." + new string('d', 61) + ".com"; // 61+1+61+1+61+1+3 = 189
            var email = $"{localPart}@{domainPart}";

            Assert.AreEqual(254, email.Length, "Test setup error: email should be exactly 254 characters");

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsTrue(result.IsValid, $"Expected 254-char email to be valid but got: {result.ErrorMessage}");
        }

        /// <summary>
        /// Test email at exactly 255 characters (should fail).
        /// </summary>
        [TestMethod]
        public void ValidateEmail_Exactly255Characters_ReturnsFalse()
        {
            // Arrange - Build email with exactly 255 characters
            var localPart = new string('a', 64);
            var domainPart = new string('d', 190) + ".com";
            var email = $"{localPart}@{domainPart}";

            Assert.AreEqual(255, email.Length, "Test setup error: email should be exactly 255 characters");

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert
            Assert.IsFalse(result.IsValid);
            Assert.AreEqual("Email address exceeds maximum length of 254 characters.", result.ErrorMessage);
        }

        /// <summary>
        /// Test that regex timeout is handled gracefully.
        /// Note: With current regex and timeout of 250ms, it's difficult to trigger timeout.
        /// This test documents the expected behavior if timeout occurs.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_RegexTimeout_ReturnsErrorMessage()
        {
            // Arrange - Create a potentially problematic email
            // (Current regex is well-designed and shouldn't timeout on normal input)
            var email = new string('a', 100) + "@" + new string('b', 100) + ".com";

            // Act
            var result = _validator.ValidateEmail(email);

            // Assert - Should either validate normally or handle timeout gracefully
            Assert.IsFalse(string.IsNullOrEmpty(result.ErrorMessage) || result.IsValid,
                "Should return either valid result or error message");
        }

        #endregion

        #region International and Unicode Tests (Currently Not Supported)

        /// <summary>
        /// Test that internationalized domain names (IDN) are currently not supported.
        /// These tests document current behavior and can be updated if support is added.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_InternationalCharacters_ReturnsFalse()
        {
            // Arrange - These are valid internationalized emails per RFC 6531
            // but not supported by current ASCII-only implementation
            var internationalEmails = new[]
            {
            "user@münchen.de",
            "测试@example.com",
            "user@例え.jp"
        };

            // Act & Assert
            foreach (var email in internationalEmails)
            {
                var result = _validator.ValidateEmail(email);
                Assert.IsFalse(result.IsValid,
                    $"Current implementation does not support international email '{email}'");
            }
        }

        #endregion

        #region Multiple Validation Tests (Ensure Statelessness)

        /// <summary>
        /// Test that validator is stateless and can handle multiple sequential validations.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_MultipleSequentialCalls_MaintainsStatelessBehavior()
        {
            // Arrange
            var validEmail = "user@example.com";
            var invalidEmail = "invalid-email";

            // Act & Assert - Multiple calls should produce consistent results
            for (int i = 0; i < 5; i++)
            {
                var validResult = _validator.ValidateEmail(validEmail);
                Assert.IsTrue(validResult.IsValid, $"Iteration {i}: Valid email should remain valid");

                var invalidResult = _validator.ValidateEmail(invalidEmail);
                Assert.IsFalse(invalidResult.IsValid, $"Iteration {i}: Invalid email should remain invalid");
            }
        }

        /// <summary>
        /// Test that validator can be instantiated multiple times with same behavior.
        /// </summary>
        [TestMethod]
        public void ValidateEmail_MultipleInstances_ProduceSameResults()
        {
            // Arrange
            var validator1 = new EmailValidation();
            var validator2 = new EmailValidation();
            var testEmail = "test@example.com";

            // Act
            var result1 = validator1.ValidateEmail(testEmail);
            var result2 = validator2.ValidateEmail(testEmail);

            // Assert
            Assert.AreEqual(result1.IsValid, result2.IsValid,
                "Multiple instances should produce same validation result");
            Assert.AreEqual(result1.ErrorMessage, result2.ErrorMessage,
                "Multiple instances should produce same error message");
        }

        #endregion
    }
}
