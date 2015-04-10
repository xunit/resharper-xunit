using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Annotations
{
    [Category("External annotations")]
    public abstract class XunitAnnotationsTest : ExternalAnnotationsTestBase2
    {
        public override void SetUp()
        {
            base.SetUp();

            EnvironmentVariables.SetUp(BaseTestDataPath);
        }

        protected override IEnumerable<string> GetReferencedAssemblies()
        {
            return base.GetReferencedAssemblies().Select(a =>
            {
                if (!Path.IsPathRooted(a))
                    return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + a);
                return a;
            });
        }

        [Test]
        public void Should_mark_fact_as_means_implicit_use()
        {
            AssertIsMeansImplicitUseAttribute("T:Xunit.FactAttribute");
        }

#region Assert.Contains

        private const string AssertContainsXmlDocId =
            "M:Xunit.Assert.Contains``1(``0,System.Collections.Generic.IEnumerable{``0})";

        private const string AssertContainsWithComparerXmlDocId =
            "M:Xunit.Assert.Contains``1(``0,System.Collections.Generic.IEnumerable{``0},System.Collections.Generic.IEqualityComparer{``0})";

        // Don't think it should be marked as an assertion method. It's not asserting that anything is null
        // it'll just NRE if the parameter is null. It's an assertion method, but it's not one we can model
        //[Test]
        //public void Should_mark_assert_contains_as_assertion_method()
        //{
        //    AssertIsAssertionMethod(AssertContainsXmlDocId);
        //    AssertIsAssertionMethod(AssertContainsWithComparerXmlDocId);
        //}

        [Test]
        public void Should_mark_assert_contains_collection_parameter_as_instant_handle()
        {
            AssertParameterIsInstantHandle(AssertContainsXmlDocId, "collection");
            AssertParameterIsInstantHandle(AssertContainsWithComparerXmlDocId, "collection");
        }

        //[Test]
        //public void Should_mark_assert_contains_collection_parameter_with_not_null_assert()
        //{
        //    AssertParameterAssertCondition(AssertContainsXmlDocId, "collection", AssertionConditionType.IS_NOT_NULL);
        //    AssertParameterAssertCondition(AssertContainsWithComparerXmlDocId, "collection", AssertionConditionType.IS_NOT_NULL);
        //}

        [Test]
        public void Should_mark_comparer_as_not_null_assert()
        {
            AssertParameterIsNotNull(AssertContainsWithComparerXmlDocId, "comparer");
        }

        // TODO: Contract annotation asserts
        //AssertHelper(xmlDocId,
        //    (cache, element) =>
        //    {
        //        var method = element as IMethod;
        //        Assert.NotNull(method, "Declared element is not an IMethod {0}", xmlDocId);
        //        var contract = cache.GetContractAnnotation(method);

        //        Assert.NotNull(contract);
        //        contract.Rows.First().
        //    });

#endregion

        #region Assert.Contains with comparer
        #endregion
    }
    
    [Category("xunit1")]
    [TestReferences(@"xunit191\xunit.dll", @"xunit191\xunit.extensions.dll")]
    public class Xunit1AnnotationsTest : XunitAnnotationsTest
    {
        [Test]
        public void Should_mark_theory_attribute_as_means_implicit_use_due_to_inheritance()
        {
            AssertIsMeansImplicitUseAttribute("T:Xunit.Extensions.TheoryAttribute");
        }
    }

    [Category("xunit2")]
    [TestReferences(@"xunit2\xunit.abstractions.dll", @"xunit2\xunit.assert.dll",
        @"xunit2\xunit.core.dll", @"xunit2\xunit.execution.dll")]
    public class Xunit2AnnotationsTest : XunitAnnotationsTest
    {
        [Test]
        public void Should_mark_theory_attribute_as_means_implicit_use_due_to_inheritance()
        {
            AssertIsMeansImplicitUseAttribute("T:Xunit.TheoryAttribute");
        }
    }
}