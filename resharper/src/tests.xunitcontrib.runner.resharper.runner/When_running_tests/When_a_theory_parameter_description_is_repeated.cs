using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Extensions;
using System.Linq;

namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tests.When_running_tests
{
    public class When_a_theory_parameter_description_is_repeated : SingleClassTestRunContext
    {
        private Method GetMethodWithTheoryAndRepeatedValues(int attributeCount = 2)
        {
            var attributes = new List<Attribute> { new TheoryAttribute() };
            for (int i = 0; i < attributeCount; i++)
                attributes.Add(new InlineDataAttribute(11));    // Will produce TestMethod1(value: 11)
            return testClass.AddMethod("TestMethod1", _ => { }, new[] { Parameter.Create<int>("value") },
                                       attributes.ToArray());
        }

        private class Data
        {
            public string Value;

            public override string ToString()
            {
                return "AlwaysTheSame.Data";
            }
        }

        private Method GetMethodWithTheoryAndParamterWithRepeatedToString(int attributeCount = 2)
        {
            // This creates a method with parameters that xunit displays as all the same:
            // eg. TestMethod1(data: AlwaysTheSame.Data)
            var attributes = new List<Attribute> {new TheoryAttribute()};
            for (int i = 0; i < attributeCount; i++)
                attributes.Add(new InlineDataAttribute(new Data { Value = string.Format("param-{0}", i) }));
            return testClass.AddMethod("TestMethod1", _ => { }, new[] {Parameter.Create<Data>("data")},
                                       attributes.ToArray());
        }

        [Fact]
        public void Should_not_report_repeated_theory_name_more_than_once()
        {
            var method = GetMethodWithTheoryAndParamterWithRepeatedToString();

            Run();

            var theoryTasks = method.TheoryTasks;

            Messages.AssertEqualTask(theoryTasks[0]).TaskStarting();
        }

        [Fact]
        public void Should_rename_second_usage_of_repeated_theory_name()
        {
            var method = GetMethodWithTheoryAndParamterWithRepeatedToString();

            Run();

            var theoryTasks = method.TheoryTasks;
            var secondTheoryTask = new XunitTestTheoryTask(theoryTasks[1].ParentElementId, theoryTasks[1].Name + " [2]");

            Messages.AssertEqualTask(theoryTasks[0]).TaskStarting();
            Messages.AssertEqualTask(secondTheoryTask).TaskStarting();
        }

        [Fact]
        public void Should_rename_subequent_usages_of_repeated_theory_name()
        {
            var method = GetMethodWithTheoryAndParamterWithRepeatedToString(4);

            Run();

            var theoryTasks = method.TheoryTasks;
            var secondTheoryTask = new XunitTestTheoryTask(theoryTasks[1].ParentElementId, theoryTasks[1].Name + " [2]");
            var thirdTheoryTask = new XunitTestTheoryTask(theoryTasks[1].ParentElementId, theoryTasks[2].Name + " [3]");
            var fourthTheoryTask = new XunitTestTheoryTask(theoryTasks[1].ParentElementId, theoryTasks[3].Name + " [4]");

            Messages.AssertEqualTask(theoryTasks[0]).TaskStarting();
            Messages.AssertEqualTask(secondTheoryTask).TaskStarting();
            Messages.AssertEqualTask(thirdTheoryTask).TaskStarting();
            Messages.AssertEqualTask(fourthTheoryTask).TaskStarting();
        }

        [Fact]
        public void Should_rename_subsequent_usages_of_theories_with_the_same_parameter_value()
        {
            var method = GetMethodWithTheoryAndRepeatedValues(4);

            Run();

            var theoryTasks = method.TheoryTasks;
            var secondTheoryTask = new XunitTestTheoryTask(theoryTasks[1].ParentElementId, theoryTasks[1].Name + " [2]");
            var thirdTheoryTask = new XunitTestTheoryTask(theoryTasks[1].ParentElementId, theoryTasks[2].Name + " [3]");
            var fourthTheoryTask = new XunitTestTheoryTask(theoryTasks[1].ParentElementId, theoryTasks[3].Name + " [4]");

            Messages.AssertEqualTask(theoryTasks[0]).TaskStarting();
            Messages.AssertEqualTask(secondTheoryTask).TaskStarting();
            Messages.AssertEqualTask(thirdTheoryTask).TaskStarting();
            Messages.AssertEqualTask(fourthTheoryTask).TaskStarting();
        }
    }
}