using System.Drawing;
using JetBrains.CommonControls;
using JetBrains.ReSharper.CodeView.TreePsiBrowser;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.TreeModels;
using JetBrains.UI.TreeView;

namespace Xunit.Runner.ReSharper
{
    public class XunitBrowserPresenter : TreeModelBrowserPresenter
    {
        public XunitBrowserPresenter()
        {
            Present<XunitTestElementClass>(PresentTestFixture);
            Present<XunitTestElementMethod>(PresentTest);
        }

        protected override bool IsNaturalParent(object parentValue,
                                                object childValue)
        {
            UnitTestNamespace @namespace = parentValue as UnitTestNamespace;
            XunitTestElementClass test = childValue as XunitTestElementClass;

            if (test != null && @namespace != null)
                return @namespace.Equals(test.GetNamespace());

            return base.IsNaturalParent(parentValue, childValue);
        }

        protected virtual void PresentTest(XunitTestElementMethod value,
                                           IPresentableItem item,
                                           TreeModelNode modelNode,
                                           PresentationState state)
        {
            if (value.Class.GetTypeClrName() != value.GetTypeClrName())
                item.RichText = string.Format("{0}.{1}", new CLRTypeName(value.GetTypeClrName()).ShortName, value.MethodName);
            else
                item.RichText = value.MethodName;

            if (value.IsExplicit)
                item.RichText.SetForeColor(SystemColors.GrayText);

            Image stateImage = UnitTestManager.GetStateImage(state);
            Image typeImage = UnitTestManager.GetStandardImage(UnitTestElementImage.Test);

            if (stateImage != null)
                item.Images.Add(stateImage);
            else if (typeImage != null)
                item.Images.Add(typeImage);
        }

        protected virtual void PresentTestFixture(XunitTestElementClass value,
                                                  IPresentableItem item,
                                                  TreeModelNode modelNode,
                                                  PresentationState state)
        {
            CLRTypeName name = new CLRTypeName(value.GetTypeClrName());

            if (IsNodeParentNatural(modelNode, value))
                item.RichText = name.ShortName;
            else
            {
                if (string.IsNullOrEmpty(name.NamespaceName))
                    item.RichText = name.ShortName;
                else
                    item.RichText = string.Format("{0}.{1}", name.NamespaceName, name.ShortName);
            }

            Image stateImage = UnitTestManager.GetStateImage(state);
            Image typeImage = UnitTestManager.GetStandardImage(UnitTestElementImage.TestContainer);

            if (stateImage != null)
                item.Images.Add(stateImage);
            else if (typeImage != null)
                item.Images.Add(typeImage);

            AppendOccurencesCount(item, modelNode, "test");
        }

        protected override object Unwrap(object value)
        {
            XunitTestElementMethod testMethod = value as XunitTestElementMethod;
            if (testMethod != null)
                value = testMethod.GetDeclaredElement();

            XunitTestElementClass testClass = value as XunitTestElementClass;
            if (testClass != null)
                value = testClass.GetDeclaredElement();

            return base.Unwrap(value);
        }
    }
}