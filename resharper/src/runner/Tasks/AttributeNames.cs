namespace XunitContrib.Runner.ReSharper.RemoteRunner.Tasks
{
    public static class AttributeNames
    {
        public const string AssemblyLocation = "AssemblyLocation";
        public const string TypeName = "TypeName";
        public const string MethodName = "MethodName";
        public const string TheoryName = "TheoryName";
        public const string Explicitly = "Explicitly";
        public const string Dynamic = "Dynamic";
        public const string ProjectId = "ProjectId";

        // Note the case. TestRemoteChannelMessageListener strips this attribute
        // when running tests. Makes for cleaner gold output
        public const string ParentId = "parentId";
    }
}