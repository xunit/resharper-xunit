Windows Phone 7
===============

Two quick comments:

Currently, the namespace of tests are incorrectly reported as XunitContrib.Runner.Silverlight.Toolkit
Jeff has just checked in a change to the testing framework that will allow me to fix this, so
this is just temporary.

When loading the wp7 solution, you will be prompted if you are sure that you want to 
reference a Silverlight assembly. Just click yes. The Silverlight 3 assemblies work
just fine. And you only get this prompt with project references. Adding the Silverlight 3
assemblies as file references will work without prompting. (Of course, you need the wp7
runner assembly, not the sl3 one)

