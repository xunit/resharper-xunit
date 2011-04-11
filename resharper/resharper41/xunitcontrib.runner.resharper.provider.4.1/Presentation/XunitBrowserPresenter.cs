using System.Drawing;
using JetBrains.CommonControls;
using JetBrains.ReSharper.CodeView.TreePsiBrowser;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.TreeModels;
using JetBrains.UI.TreeView;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider
{
    internal class XunitBrowserPresenter : TreeModelBrowserPresenter
    {
        internal XunitBrowserPresenter()
        {
            Present<XunitTestElementClass>(PresentTestFixture);
            Present<XunitTestElementMethod>(PresentTest);
        }

        protected override bool IsNaturalParent(object parentValue,
                                                object childValue)
        {
            var @namespace = parentValue as UnitTestNamespace;
            var test = childValue as XunitTestElementClass;

            if (test != null && @namespace != null)
                return @namespace.Equals(test.GetNamespace());

            return base.IsNaturalParent(parentValue, childValue);
        }

        private static void PresentTest(XunitTestElementMethod value,
                                        IPresentableItem item,
                                        TreeModelNode modelNode,
                                        PresentationState state)
        {
            item.RichText = value.Class.GetTypeClrName() != value.GetTypeClrName() ? string.Format("{0}.{1}", new CLRTypeName(value.GetTypeClrName()).ShortName, value.MethodName) : value.MethodName;

            if (value.IsExplicit)
                item.RichText.SetForeColor(SystemColors.GrayText);

            var stateImage = UnitTestManager.GetStateImage(state);
            var typeImage = UnitTestManager.GetStandardImage(UnitTestElementImage.Test);

            if (stateImage != null)
                item.Images.Add(stateImage);
            else if (typeImage != null)
                item.Images.Add(typeImage);
        }

        private void PresentTestFixture(XunitTestElementClass value,
                                        IPresentableItem item,
                                        TreeModelNode modelNode,
                                        PresentationState state)
        {
            var name = new CLRTypeName(value.GetTypeClrName());

            if (IsNodeParentNatural(modelNode, value))
                item.RichText = name.ShortName;
            else
                item.RichText = string.IsNullOrEmpty(name.NamespaceName) ? name.ShortName : string.Format("{0}.{1}", name.NamespaceName, name.ShortName);

            var stateImage = UnitTestManager.GetStateImage(state);
            var typeImage = UnitTestManager.GetStandardImage(UnitTestElementImage.TestContainer);

            if (stateImage != null)
                item.Images.Add(stateImage);
            else if (typeImage != null)
                item.Images.Add(typeImage);

            AppendOccurencesCount(item, modelNode, "test");
        }

        protected override object Unwrap(object value)
        {
            if (value is XunitTestElementMethod || value is XunitTestElementClass)
                value = ((XunitTestElement) value).GetDeclaredElement();

            return base.Unwrap(value);
        }
    }
}