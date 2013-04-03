using System.Reflection;
using JetBrains.Application.Env.Components;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi.Impl.Reflection2.ExternalAnnotations;

namespace XunitContrib.Runner.ReSharper.UnitTestProvider.Annotations
{
    // This is a rather cheeky hack. ExternalAnnotationsManager has a private
    // variable that is never set, but it's a path that is checked for annotations.
    // We're going to set it, so that we can install our anntoations file without
    // having to write to Program Files\JetBrains\ReSharper\vX.X\bin\ExternalAnnotations
    // This is rather presumptuous of us.
    // We have a couple of things in our favour:
    // 1. ReSharper doesn't use this value
    // 2. ReSharper 6.1 and 7.1 are done, so they won't be using it
    // 3. We don't do anything if the field can't be found, or has already been set (who by!?)
    // 4. We'll set it to a shared value, so others can use it
    //    (%LOCALAPPDATA%\JetBrains\ReSharper\vX.X\ExternalAnnotations)
    // 5. ReSharper 8.0 supports annotations as extensions, so we won't need it post 7.1
    [SolutionComponent]
    public class AnnotationsLoaderHack
    {
        public AnnotationsLoaderHack(ExternalAnnotationsManager manager, ProductSettingsLocation location)
        {
            var type = manager.GetType();
            var field = type.GetField("myPathToExternalAnnotations", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                var path = field.GetValue(manager) as string;
                if (string.IsNullOrEmpty(path))
                {
                    var settingsLocation = location.GetUserSettingsNonRoamingDir(ProductSettingsLocationFlag.ThisProductThisVersionAnyEnvironment);
                    field.SetValue(manager, settingsLocation.Combine("ExternalAnnotations"));
                }
            }
        }
    }
}