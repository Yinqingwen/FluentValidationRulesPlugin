using System.Linq;
using FluentValidation;
using NUnit.Framework;
using Plugin.FluentValidationRules.Extensions;

namespace Plugin.FluentValidationRules.Tests
{
    [TestFixture]
    public class Tests
    {
        public AbstractValidator<TestClass> Validator { get; set; }
        public Validatables ValidatableProps { get; set; }
        public Validatable<string> FromEmail { get; set; }
        public Validatable<string> ToName { get; set; }
        public Validatable<int?> Rating { get; set; }
        public Validatable<string> MessageHtml { get; set; }

        [SetUp]
        public void Setup()
        {
            // Let's pretend these are the properties that we would have in a ViewModel
            FromEmail = new Validatable<string>(nameof(TestClass.FromEmail));
            ToName = new Validatable<string>(nameof(TestClass.ToName));
            Rating = new Validatable<int?>(nameof(TestClass.Rating));
            MessageHtml = new Validatable<string>(nameof(TestClass.MessageHtml));

            Validator = new TestClassValidator();
            ValidatableProps = new Validatables(FromEmail, ToName, MessageHtml, Rating);
        }

        [Test]
        public void ApplyResultsTo_SingleBadProp_ResultAccuracy()
        {
            // Let's pretend these are the values our properties are currently bound to from our View
            Rating.Value = 11; // bad

            var testClassInstance = new TestClass
            {
                Rating = Rating.Value
            };

            var result = Validator.Validate(testClassInstance).ApplyResultsTo(Rating);

            Assert.IsFalse(result.IsValidOverall);
            Assert.AreEqual(4, result.AllErrors.Count);
            Assert.AreEqual(3, result.NonSplitErrors.Count);

            Assert.IsFalse(Rating.IsValid);
            Assert.AreEqual(1, Rating.Errors.Count);
        }

        [Test]
        public void ApplyResultsToThenClear_MultipleProps_ResultAccuracy()
        {
            // Let's pretend these are the values our properties are currently bound to from our View
            FromEmail.Value = "LOL_I_AM_NOT_AN_EMAIL!"; // bad
            ToName.Value = "X"; // bad
            MessageHtml.Value = "NOPE"; // bad
            Rating.Value = 5; // good

            var testClassInstance = new TestClass
            {
                FromEmail = FromEmail.Value,
                ToName = ToName.Value,
                MessageHtml =  MessageHtml.Value,
                Rating = Rating.Value
            };

            var result = Validator.Validate(testClassInstance).ApplyResultsTo(ValidatableProps);

            Assert.IsFalse(result.IsValidOverall);
            Assert.AreEqual(4, result.AllErrors.Count);

            Assert.IsTrue(result.IsValidForNonSplitErrors);
            Assert.AreEqual(0, result.NonSplitErrors.Count);

            Assert.IsFalse(ValidatableProps.AreValid);
            Assert.AreEqual(4, ValidatableProps.Errors.Count);

            Assert.IsFalse(FromEmail.IsValid);
            Assert.AreEqual(1, FromEmail.Errors.Count);

            Assert.IsFalse(ToName.IsValid);
            Assert.AreEqual(2, ToName.Errors.Count);

            Assert.IsFalse(MessageHtml.IsValid);
            Assert.AreEqual(1, MessageHtml.Errors.Count); // only 1 error expected because of CascadeMode set to StopOnFirstFailure

            Assert.IsTrue(Rating.IsValid);
            Assert.AreEqual(0, Rating.Errors.Count);

            ValidatableProps.Clear();

            Assert.IsTrue(ValidatableProps.AreValid);
            Assert.IsEmpty(ValidatableProps.Errors);
            Assert.IsEmpty(ValidatableProps.FirstError);

            Assert.IsTrue(FromEmail.IsValid);
            Assert.IsEmpty(FromEmail.Errors);
            Assert.IsEmpty(FromEmail.FirstError);

            Assert.IsTrue(ToName.IsValid);
            Assert.IsEmpty(ToName.Errors);
            Assert.IsEmpty(ToName.FirstError);

            Assert.IsTrue(MessageHtml.IsValid);
            Assert.IsEmpty(MessageHtml.Errors);
            Assert.IsEmpty(MessageHtml.FirstError);

            Assert.IsTrue(Rating.IsValid);
            Assert.IsEmpty(Rating.Errors);
            Assert.IsEmpty(Rating.FirstError);
        }

        [Test]
        public void ApplyResultsTo_MultiplePropsAllGood_IsValidResults()
        {
            // Let's pretend these are the values our properties are currently bound to from our View
            FromEmail.Value = "IamTheRe@lDeal.com"; // good
            ToName.Value = "Maximus Decimus"; // good
            MessageHtml.Value = "<!DOCTYPE html> Sure!"; // good
            Rating.Value = 5; // good

            var testClassInstance = new TestClass
            {
                FromEmail = FromEmail.Value,
                ToName = ToName.Value,
                MessageHtml =  MessageHtml.Value,
                Rating = Rating.Value
            };

            var result = Validator.Validate(testClassInstance).ApplyResultsTo(ValidatableProps);

            Assert.IsTrue(result.IsValidOverall);
            Assert.IsEmpty(result.AllErrors);

            Assert.IsTrue(result.IsValidForNonSplitErrors);
            Assert.IsEmpty(result.NonSplitErrors);

            Assert.IsTrue(ValidatableProps.AreValid);
            Assert.IsEmpty(ValidatableProps.Errors);
            Assert.IsEmpty(ValidatableProps.FirstError);

            Assert.IsTrue(FromEmail.IsValid);
            Assert.IsEmpty(FromEmail.Errors);
            Assert.IsEmpty(FromEmail.FirstError);

            Assert.IsTrue(ToName.IsValid);
            Assert.IsEmpty(ToName.Errors);
            Assert.IsEmpty(ToName.FirstError);

            Assert.IsTrue(MessageHtml.IsValid);
            Assert.IsEmpty(MessageHtml.Errors);
            Assert.IsEmpty(MessageHtml.FirstError);

            Assert.IsTrue(Rating.IsValid);
            Assert.IsEmpty(Rating.Errors);
            Assert.IsEmpty(Rating.FirstError);
        }

        [Test]
        public void Validate_SingleBadPropWithoutCascadingWithCustomMessages_ErrorsMatch()
        {
            // Let's pretend these are the values our properties are currently bound to from our View
            ToName.Value = "XX"; // bad

            var testClassInstance = new TestClass
            {
                ToName = ToName.Value
            };

            var result = Validator.Validate(testClassInstance).ApplyResultsTo(ToName);

            Assert.IsFalse(ToName.IsValid);
            Assert.AreEqual(2, ToName.Errors.Count);
            Assert.AreEqual("How you bout to enter a FULL 'name' with less than 3 chars!?", ToName.Errors.First());
            Assert.AreEqual("Expecting at least first and last name separated by a space!", ToName.Errors.Last());
        }

        [Test]
        public void Clear_SingleProp_Success()
        {
            // Let's pretend these are the values our properties are currently bound to from our View
            ToName.Value = "XX"; // bad

            var testClassInstance = new TestClass
            {
                ToName = ToName.Value
            };

            var result = Validator.Validate(testClassInstance).ApplyResultsTo(ToName);

            Assert.IsFalse(ToName.IsValid);
            Assert.AreEqual(2, ToName.Errors.Count);
            Assert.AreEqual("How you bout to enter a FULL 'name' with less than 3 chars!?", ToName.FirstError);

            ToName.Clear();

            Assert.IsTrue(ToName.IsValid);
            Assert.IsEmpty(ToName.Errors);
            Assert.IsEmpty(ToName.FirstError);

            result.Clear();

            Assert.IsTrue(result.IsValidOverall);
            Assert.IsTrue(result.IsValidForNonSplitErrors);
            Assert.IsEmpty(result.AllErrors);
            Assert.IsEmpty(result.NonSplitErrors);
            Assert.IsEmpty(result.FirstOfAllErrors);
            Assert.IsEmpty(result.FirstOfNonSplitErrors);
        }

        
        [Test]
        public void Clear_SinglePropViaGroupLeavingOneBad_ErrorsUpdated()
        {
            // Let's pretend these are the values our properties are currently bound to from our View
            ToName.Value = "XX"; // bad
            Rating.Value = 0; // bad

            var testClassInstance = new TestClass
            {
                ToName = ToName.Value,
                Rating = Rating.Value
            };

            Validator.Validate(testClassInstance).ApplyResultsTo(ValidatableProps);

            Assert.IsFalse(ValidatableProps.AreValid);
            Assert.AreEqual(5, ValidatableProps.Errors.Count);

            Assert.IsFalse(Rating.IsValid);
            Assert.AreEqual(1, Rating.Errors.Count);

            ValidatableProps.Clear(nameof(TestClass.ToName));

            Assert.IsTrue(ToName.IsValid);
            Assert.IsEmpty(ToName.Errors);
            Assert.IsEmpty(ToName.FirstError);

            Assert.IsFalse(Rating.IsValid);
            Assert.AreEqual(1, Rating.Errors.Count);
            Assert.IsNotEmpty(Rating.FirstError);

            Assert.IsFalse(ValidatableProps.AreValid);
            Assert.AreEqual(3, ValidatableProps.Errors.Count); // count drops to 3 failed errors only
        }

        [Test]
        public void GetRulesFor_SingleProp_GetsRelevantRules()
        {
            var rules = Validator.GetRulesFor(ToName);
            Assert.AreEqual(2 , rules.Count);
        }

        [Test]
        public void GetRulesFor_MultipleProps_GetsRelevantRules()
        {
            var rules = Validator.GetRulesFor(ValidatableProps);
            Assert.AreEqual(5 , rules.Count);
        }
    }
}