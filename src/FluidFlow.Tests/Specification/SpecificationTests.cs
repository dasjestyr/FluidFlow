using System;
using System.Collections.Generic;
using System.Linq;
using FluidFlow.Specification;
using Xunit;

namespace FluidFlow.Tests.Specification
{
    public class SpecificationTests
    {
        private readonly List<FakePerson> _users;

        public SpecificationTests()
        {
            _users = GetUsers(1000);
        }

        [Fact]
        public void IsSatisfiedBy_IsEnforced()
        {
            // arrange
            var spec = new MaleSpec();

            // act
            var results = _users.Where(u => spec.IsSatisfiedBy(u));

            // assert
            Assert.True(results.All(u => u.Gender != Gender.Female));
        }

        [Fact]
        public void IsSatisfiedBy_AndSpec_IsEnforced()
        {
            // arrange
            var isMale = new MaleSpec();
            var collegeAged = new CollegeAgedSpec();

            var spec = isMale.And(collegeAged);

            // act
            var collegeAgedMen = _users.Where(u => spec.IsSatisfiedBy(u)).ToList();

            // assert
            Assert.True(collegeAgedMen.All(u => u.Gender != Gender.Female), "There were some users that were not male");
            Assert.True(collegeAgedMen.All(u => u.Age >= 18 && u.Age <= 27), "Some users were outside of the age range");
        }

        [Fact]
        public void IsSatisfiedBy_OrSpec_IsEnforced()
        {
            // arrange
            var isMale = new MaleSpec();
            var canDrink = new OldEnoughToDrinkSpec();

            var spec = isMale.Or(canDrink);

            // act
            var menOrCanDrink = _users.Where(u => spec.IsSatisfiedBy(u)).ToList();

            // assert
            Assert.True(menOrCanDrink.All(u => u.Gender != Gender.Female), "There are some users that were not male");
            Assert.True(menOrCanDrink.Any(u => u.Age >= 21), "There should have been some users that were old enough to drink. But this test could suck.");
        }

        [Fact]
        public void IsSatisfiedBy_NotSpec_IsEnforced()
        {
            // arrange
            var isMale = new MaleSpec();
            var canDrink = new OldEnoughToDrinkSpec();

            var spec = canDrink.Not(isMale);

            // act
            var womenThatCanDrink = _users.Where(u => spec.IsSatisfiedBy(u)).ToList();

            // assert
            Assert.True(womenThatCanDrink.All(u => u.Gender == Gender.Female), "There were some users that were not female");
            Assert.True(womenThatCanDrink.All(u => u.Age >= 21), "There were some users that were not old enough to drink");
        }

        private static List<FakePerson> GetUsers(int count)
        {
            var users = GenFu.GenFu.ListOf<FakePerson>(count);
            return users;
        }
    }

    internal class MaleSpec : Specification<FakePerson>
    {
        public override bool IsSatisfiedBy(FakePerson target)
        {
            return target.Gender == Gender.Male;
        }
    }

    internal class OldEnoughToDrinkSpec : Specification<FakePerson>
    {
        public override bool IsSatisfiedBy(FakePerson target)
        {
            return target.Age >= 21;
        }
    }
    
    internal class CollegeAgedSpec : Specification<FakePerson>
    {
        public override bool IsSatisfiedBy(FakePerson target)
        {
            return target.Age >= 18 && target.Age <= 27;
        }
    }

    internal class FakePerson
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }

        public Gender Gender { get; set; }
    }

    internal enum Gender
    {
        Male,
        Female
    }
}
