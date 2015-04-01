# How the runner works

The test provider creates test elements for each class and method that is a test. When it's about to start a test run, ReSharper asks each element for a sequence of tasks that describe what needs to be done in order to run the test. Each task has a runner ID, which is the same runner ID exposed by the provider. ReSharper uses this to ask the provider for the assembly used to run the task. The task also includes any data required by the provider's runner code to be able to execute the steps the task represents. The task itself has no behaviour - it must be `Serializable`, and implement `IEquatable<T>`.

Once ReSharper has gathered a list of sequences of tasks (one for each test element), it then runs an external process that will host the test runner code - the task runner process. Together they set up inter-process communication and the runner process requests the list of task sequences. It then collapses this list into a tree, by comparing nodes in each sequences - if they're the same, they become a single node in the tree. Realistically, this means we end up with a tree of nodes that begin with an assembly load task, whose children are fixture tasks, or class tasks, and which in turn have children that are method tasks. There may be more than one root node, if there is more than assembly being run.

The task runner process then passes this tree of nodes to the runner identified by the ID of the root
node. It calls that runner's `RecursiveRemoteTaskRunner.ExecuteRecursive` method, passing in
the tree of nodes. The runner now uses the data in the node's task to perform a specific task,
such as creating an AppDomain, loading an assembly or running tests.

While running tests, the runner uses `IRemoteTaskServer` to notify ReSharper (the server) of
the progress of each task, including starting, finished, failed, skipped, etc.

## Details for ReSharper 7.1 and earlier

In ReSharper 7.1 and earlier (and dotCover 2.2 and earlier - they share source code), ReSharper
is responsible for creating the AppDomain that the tests run in. In other words, the external
task runner host process does this:

1. Talks to ReSharper and requests the tasks
2. Collapses the list of task sequences into a tree, with one or more root nodes
3. Creates an AppDomain
4. Loads the provider's test runner into the AppDomain
5. Calls the provider's test runner's `ExecuteRecursive` method, again, in the new AppDomain.

(At this point, the test runner code will run the test framework, all isolated in the new AppDomain)

The details of how this happens are important, because the xUnit.net runner doesn't want ReSharper
to create the AppDomain - the xunit API can handle that for us.

The task runner process creates an instance of `StartupTaskRunnerHost` and calls `ExecuteNodes`.
This in turn loops over the root nodes of the tree, gets the runner matching the runner ID of that
node and calls the runner's `ExecuteRecursive` method.

The task runner process hard-codes the runner ID "AssemblyLoadTaskRunner". ReSharper 7.1 also adds a
runner ID of "NotIsolatedAssemblyLoadTaskRunner". These IDs map to instances of `IsolatedAssemblyTaskRunner`
and `NotIsolatedAssemblyLoadTaskRunner` respectively.

When the built-in providers (e.g. NUnint) create their task sequences, the first task is an
instance of `AssemblyLoadTask`, with a runner ID of "AssemblyLoadTaskRunner" (NOT "nUnit"),
and then adds the normal nUnit tasks (with an ID of "nUnit").

So now, when looping over the root nodes, the task runner process sees an instance of `AsssemblyLoadTask`,
and does the following:

1. Call `IsolatedAssemblyTaskRunner.ExecuteRecursive`
2. This creates an instance of `IsolatedAppDomainHost`, wrapped in a using statement
3. `IsolatedAppDomainHost` creates a new AppDomain, enabling shadow copy based on the user's options
4. An instance of `CurrentAppDomainHost` is created in the new AppDomain
5. `IsolatedAssemblyTaskRunner` now calls `IsolatedAppDomainHost.ExecuteRecursive`, passing in the *children* of the `AssemblyLoadTask`
6. `IsolatedAppDomainHost.ExecuteRecursive` serialises the tasks into an XML string, and passes to the `CurrentAppDomainHost` in the new AppDomain
7. `CurrentAppDomainHost` deserialises the tasks, loops over them and gets the runner from the runner ID
8. The runner has a chance to configure the AppDomain. If it requires a different thread priority or COM apartment, a new thread is created
9. The runner is then called to run the tasks (remember, this happens in the new AppDomain)
7. Once the `ExecuteRecursive` methods complete, `IsolatedAppDomain.Dispose` is called, and this unloads the AppDomain

### Running the tasks

Running the tasks is not quite as simple as passing the nodes to the runner, and calling the appropriate notification methods on `IRemoteTaskServer`. It also needs to notify the "client controller". This mechanism is how dotCover can do code coverage per test. It requires a client side component - that is, code needs to be loaded into the new AppDomain, and this code needs to be notified when tasks start and finish. The client controller also needs to pass information to the server, i.e. dotCover, hooked into the ReSharper unit testing framework.

`CurrentAppDomainHost` creates the client controller in it's constructor, using the information from `IRemoteTaskServer.ClientControllerInfo`. It's a simple case of loading the assembly and instantiating a type. It then creates a wrapper for `IRemoteTaskServer` so that when `TaskStarting` or `TaskFinished` is called, it also notifies the client controller. The client controller can return a string at this point, and this string must be passed to `IRemoteTaskServer.ClientMessage`.

If `CurrentAppDomainHost` encounters another instance of `AssemblyLoadTask`, the ID is now mapped to the `AssemblyLoadTaskRunner` (and not `IsolatedAssemblyTaskRunner`). This class simply changes the current directory to the codebase of the assembly in the task, and uses a new instance of `CurrentAppDomainHost` to run the child nodes. No New AppDomain is created.

### MSTest

For ReSharper 7.1, the MSTest provider uses `NotIsolatedAssemblyTaskRunner` as its top-level task. This uses the `AssemblyLoadTaskRunner` to set the current directory (so the assembly codebase works) and continues running the task nodes in the current AppDomain, not a new one, allowing it more control of where the tests are run.

### xUnit.net

The xunit API creates an AppDomain for us, dealing with shadow copy and everything, so we don't want to use `AssemblyLoadTask`. We can't use `NotIsolatedAssemblyLoadTask` because that's only available in ReSharper 7.1, which would break 6.1 support. So, all of our tasks run in the current (default) AppDomain. Since we don't get invoked by `CurrentAppDomainHost`, we must also handle the client controller.

The `XunitTaskRunner` implements `RecursiveRemoveTaskRunner.ExecuteRecursive` by creating an abstraction that handles `IRemoteTaskServer` and the client controller details. It then walks the task tree and gathers all the details needed to run the tests. These are then passed to xunit, which creates a new AppDomain, handling shadow copy if the user has requested it. Xunit notifies us of test events, and we in turn notify the server through the `IRemoteTaskServer` abstraction.

## Cleaning up the cache folder

`IsolatedAppDomainHost` creates a temporary folder to hold the shadow copy cache. It attempts to delete the directory at the end of the run, but if the run is aborted, the folder might not get cleaned up. So, `IsolatedAppDomainHost` calls `IRemoteTaskServer.SetTempFolderPath` to notify ReSharper of the cache location. If the run is aborted by the user, ReSharper will attempt to delete the folder, sleeping for a second between retries.

The xunit API will also delete the cache directory after unloading the AppDomain. Currently, the runner doesn't notify ReSharper of the cache directory, so it doesn't get cleaned up on abort. `RecursiveRemoteTaskRunner.Abort` allows for us to co-operatively abort the test run (`IRunnerLogger.TestFinished` allows us to return stop running tests).

## Details for ReSharper 8.0

The overall architecture doesn't change much for 8.0. The biggest difference is that the task runner process will always create the client controller, in the default AppDomain, and the default implementation of `IRemoteTaskServer` handles notifying the client controller. We no longer have to do this ourselves, although the code still supports it in order to maintain backwards compatibility with dotCover and earlier versions of ReSharper.

## DotCover

The details for dotCover are essentially the same as for ReSharper - they share source code, but
not binaries. dotCover 2.2 is equivalent to ReSharper 7.1.

