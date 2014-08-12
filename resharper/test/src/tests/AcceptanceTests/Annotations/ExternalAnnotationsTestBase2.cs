using System;
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CodeAnnotations;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.ReSharper.TestFramework;
using JetBrains.Util;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Annotations
{
    public abstract class ExternalAnnotationsTestBase2 : BaseTestWithSingleProject
    {
        protected void AssertHelper(string xmlDocId, Action<CodeAnnotationsCache, IDeclaredElement> assert)
        {
            WithSingleProject(EmptyList<string>.InstanceList, (lifetime, solution, project) => RunGuarded(() =>
            {
                var psiModule = solution.PsiModules().GetPrimaryPsiModule(project);
                var psiServices = solution.GetPsiServices();

                var declaredElement = XMLDocUtil.ResolveId(psiServices, xmlDocId, psiModule, true,
                    project.GetResolveContext());
                Assert.NotNull(declaredElement, "Declared element cannot be resolved from XML Doc ID {0}", xmlDocId);

                var annotationsCache = psiServices.GetCodeAnnotationsCache();

                assert(annotationsCache, declaredElement);
            }));
        }

        protected IParameter GetParameter(string xmlDocId, IDeclaredElement element, string parameterName)
        {
            var parametersOwner = element as IParametersOwner;
            Assert.NotNull(parametersOwner, "Declared element is not an IParametersOwner {0}", xmlDocId);

            var parameter = parametersOwner.Parameters.SingleOrDefault(p => p.ShortName == parameterName);
            Assert.NotNull(parameter, "Parameter \"{0}\" is not found on {1}", parameterName, xmlDocId);

            return parameter;
        }

        protected void AssertParameterAssertCondition(string xmlDocId, string parameterName,
            AssertionConditionType conditionType)
        {
            AssertHelper(xmlDocId, (cache, element) =>
            {
                var parameter = GetParameter(xmlDocId, element, parameterName);

                var actual = cache.GetParameterAssertionCondition(parameter);
                Assert.AreEqual(conditionType, actual);
            });
        }

        protected void AssertParameterCanBeNull(string xmlDocId, string parameterName)
        {
            AssertParameterNullableValue(xmlDocId, parameterName, CodeAnnotationNullableValue.NOT_NULL);
        }

        protected void AssertParameterIsNotNull(string xmlDocId, string parameterName)
        {
            AssertParameterNullableValue(xmlDocId, parameterName, CodeAnnotationNullableValue.NOT_NULL);
        }

        private void AssertParameterNullableValue(string xmlDocId, string parameterName, CodeAnnotationNullableValue? expected)
        {
            AssertHelper(xmlDocId, (cache, element) =>
            {
                var parameter = GetParameter(xmlDocId, element, parameterName);

                var actual = cache.GetNullableAttribute(parameter);
                Assert.AreEqual(expected, actual);
            });
        }

        protected void AssertIsMeansImplicitUseAttribute(string xmlDocId)
        {
            AssertHelper(xmlDocId, (cache, element) =>
            {
                var attributesOwner = element as IAttributesOwner;
                Assert.NotNull(attributesOwner, "Declared element is not an IAttributesOwner {0}", xmlDocId);

                var attributeInstances = attributesOwner.GetAttributeInstances(false);
                foreach (var attributeInstance in attributeInstances)
                {
                    ImplicitUseKindFlags kindFlags;
                    ImplicitUseTargetFlags targetFlags;
                    if (cache.IsMeansImplicitUse(attributeInstance, out kindFlags, out targetFlags))
                        return;
                }

                Assert.Fail("Declared element is not marked as implicit use {0}", xmlDocId);
            });
        }

        protected void AssertIsAssertionMethod(string xmlDocId)
        {
            AssertHelper(xmlDocId, (cache, element) =>
            {
                var method = element as IMethod;
                Assert.NotNull(method, "Declared element is not an IMethod {0}", xmlDocId);

                Assert.True(cache.IsAssertionMethod(method));
            });
        }

        protected void AssertParameterIsInstantHandle(string xmlDocId, string parameterName)
        {
            AssertHelper(xmlDocId, (cache, element) =>
            {
                var parametersOwner = element as IParametersOwner;
                Assert.NotNull(parametersOwner, "Declared element is not an IParametersOwner {0}", xmlDocId);

                var parameter = parametersOwner.Parameters.SingleOrDefault(p => p.ShortName == parameterName);
                Assert.NotNull(parameter, "Parameter \"{0}\" is not found on {1}", parameterName, xmlDocId);

                Assert.True(cache.GetInstantHandle(parameter));
            });
        }
    }
}